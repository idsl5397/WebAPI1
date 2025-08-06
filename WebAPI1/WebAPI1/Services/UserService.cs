using System.Security.Cryptography;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebAPI1.Context;
using WebAPI1.Controllers;
using WebAPI1.Entities;


namespace WebAPI1.Services;

public class ResetPasswordDto
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}
public class VerifyCodeDto
{
    public string Code { get; set; }
    public string Email { get; set; }
}
public class VerificationData
{
    public string Code { get; set; }
    public int Attempts { get; set; }
}
public class EmailRequestDto
{
    public string Email { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string NickName { get; set; }
}
public class RegisterUserDto
{
    public string UserName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public int OrganizationId { get; set; }
    public string Unit { get; set; }
    public string Position { get; set; }
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    
    public string RefreshToken { get; set; }
    public string Token { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    // 強制要求使用者變更密碼（過期或初次登入）
    public bool ForceChangePassword { get; set; } = false;
    // 🔔 新增：是否提醒密碼即將過期
    public bool ShowPasswordExpiryWarning { get; set; } = false;
}

public class UpdateUserDto
{
    public string? Nickname { get; set; }
    public string? Mobile { get; set; }
    public string? Unit { get; set; }
    public string? Position { get; set; }
}

public class ChangePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public interface IUserService
{
    Task<bool> CreateUserAsync(RegisterUserDto dto);
    Task<LoginResultDto> VerifyUserLoginAsync(LoginDto dto);
    
    //獲取委員資料名單
    Task<List<UserDto>> GetCommitteeUsers();
    
    Task<User?> GetUserByIdAsync(Guid id);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<bool> ChangePasswordAsync(Guid id, string oldPassword, string newPassword);
    Task<bool> SendVerificationCodeAsync(string email);
    Task<bool> VerifyEmailCodeAsync(string email, string inputCode);
    Task<bool> ResetPasswordAsync(string email, string newPassword);
}

public class UserService:IUserService
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;
    private readonly IDatabase _redisDb;
    private readonly IEmailService _emailService;
    public UserService(
        ISHAuditDbcontext db,
        ILogger<UserService> logger,IConfiguration configuration,IAuthService authService,IConnectionMultiplexer redis,IEmailService emailService)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
        _authService = authService;
        _redisDb = redis.GetDatabase(); 
        _emailService = emailService;
    }
    
    static string Generate8DigitNumber()
    {
        // 產生一個隨機的 8 碼數字
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[4]; // 4 bytes 可表示一個 32-bit 整數
            rng.GetBytes(bytes);
            int number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 100000000; // 限制在 8 位數
            return number.ToString("D8"); // 確保補滿 8 碼
        }
    }
    public async Task<bool> CreateUserAsync(RegisterUserDto dto)
    {
        string hashedPassword = Argon2.Hash(dto.Password,
            type: Argon2Type.HybridAddressing,
            timeCost: 2,
            memoryCost: 32768,
            parallelism: 2);

        var user = new User
        {
            Username = dto.UserName,
            Email = dto.Email,
            Mobile = dto.Phone,
            Nickname = dto.Name,
            Unit = dto.Unit,
            Position = dto.Position,
            OrganizationId = dto.OrganizationId,
            PasswordHash = hashedPassword,
            EmailVerified = true,
            EmailVerifiedAt = null,
            TokenExpiresAt = null,
            ForceChangePassword = false,
            PasswordPolicyId = 1,
            PasswordChangedAt = null,
            PasswordExpiresAt = null,
            PasswordFailedAttempts = 0,
            PasswordLockedUntil = null,
            LastPasswordExpiryReminder = null,
            CreatedAt = tool.GetTaiwanNow(),
            UpdatedAt = tool.GetTaiwanNow()
        };

        _db.Users.Add(user);
        var result = await _db.SaveChangesAsync();

        if (result <= 0) return false;

        // 取得該使用者組織的 TypeId
        var org = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);

        if (org == null)
            throw new Exception("無效的組織 ID，找不到對應的組織");

        // 根據 TypeId 決定 RoleId
        int roleId;
        if (new[] { 1, 5, 6 }.Contains(org.TypeId))
            roleId = 1;
        else if (new[] { 2, 3, 4 }.Contains(org.TypeId))
            roleId = 2;
        else
            throw new Exception($"尚未定義 TypeId {org.TypeId} 對應的角色");

        // 指派角色
        var userRole = new UserRole
        {
            UserId = user.Id, // 注意：user.Id 是 Entity Framework 自動生成的 Guid
            RoleId = roleId,
            AssignedAt = tool.GetTaiwanNow(),
            AssignedBy = Guid.Empty // ⚠️ 根據實務可改為系統帳號或管理者帳號
        };

        _db.UserRoles.Add(userRole);
        var roleResult = await _db.SaveChangesAsync();

        return roleResult > 0;
    }
    
    public async Task<LoginResultDto> VerifyUserLoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .Include(u => u.PasswordPolicy)
            .Where(u => u.Email == dto.Usermail)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return new LoginResultDto { Success = false, Message = "使用者不存在" };
        }

        // 檢查帳號是否被鎖定
        if (user.PasswordLockedUntil.HasValue && user.PasswordLockedUntil.Value > tool.GetTaiwanNow())
        {
            return new LoginResultDto { Success = false, Message = $"帳號已鎖定，請於 {user.PasswordLockedUntil.Value:yyyy-MM-dd HH:mm} 後再試" };
        }

        if (!Argon2.Verify(user.PasswordHash, dto.Password))
        {
            user.PasswordFailedAttempts++;
            
            // 檢查是否需要鎖定
            var policy = user.PasswordPolicy ?? await _db.PasswordPolicy.FirstOrDefaultAsync(p => p.IsDefault);
            if (policy != null && user.PasswordFailedAttempts >= policy.LockoutThreshold)
            {
                user.PasswordLockedUntil = tool.GetTaiwanNow().AddMinutes(policy.LockoutDurationMinutes);
            }

            await _db.SaveChangesAsync();

            return new LoginResultDto { Success = false, Message = "帳號或密碼錯誤" };
        }

        // 驗證成功：重置失敗次數與鎖定狀態
        user.PasswordFailedAttempts = 0;
        user.PasswordLockedUntil = null;
        user.LastLoginAt = tool.GetTaiwanNow();

        // 取得密碼策略
        var currentPolicy = user.PasswordPolicy ?? await _db.PasswordPolicy.FirstOrDefaultAsync(p => p.IsDefault);
        
        var permissions = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToListAsync();
        
        // 檢查密碼是否已過期或即將過期
        if (currentPolicy?.PasswordExpiryDays > 0)
        {
            var passwordSetTime = user.PasswordChangedAt ?? user.CreatedAt;
            var expiryDate = passwordSetTime.AddDays(currentPolicy.PasswordExpiryDays);
            var daysUntilExpiry = (expiryDate - tool.GetTaiwanNow()).TotalDays;
            var now = tool.GetTaiwanNow();
            
            if (tool.GetTaiwanNow() > expiryDate)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = "密碼已過期，請重設密碼",
                    ForceChangePassword = true
                };
            }
            else if (daysUntilExpiry <= currentPolicy.PasswordExpiryWarningDays)
            {
                if (user.LastPasswordExpiryReminder == null || user.LastPasswordExpiryReminder.Value.Date < now.Date)
                {
                    user.LastPasswordExpiryReminder = now;
                    await _db.SaveChangesAsync();

                    var remaining = expiryDate - now;
                    var message = $"密碼將於 {expiryDate:yyyy-MM-dd HH:mm:ss} 過期（剩餘 {remaining.Days} 天 {remaining.Hours} 小時 {remaining.Minutes} 分鐘），將會強制變更密碼";

                    return new LoginResultDto
                    {
                        Success = true,
                        Message = message,
                        Token = await _authService.GenerateAccessToken(user.Id.ToString()),
                        Nickname = user.Nickname,
                        Email = user.Email
                    };
                }
            }
        }

        await _db.SaveChangesAsync();

        var accessToken = await _authService.GenerateAccessToken(user.Id.ToString());
        var refreshToken = _authService.GenerateRefreshToken(user.Id.ToString());
        // _authService.SetRefreshTokenCookie(refreshToken);

        return new LoginResultDto
        {
            Success = true,
            Message = "登入成功",
            Token = accessToken,
            RefreshToken = refreshToken,
            Nickname = user.Nickname,
            Email = user.Email
        };
    }

    
    public async Task<List<UserDto>> GetCommitteeUsers()
    {
        var result = await _db.Users
            .Include(u => u.Organization) // 明確載入關聯資料
            .Where(u => u.OrganizationId != null &&
                        u.Organization != null &&
                        new[] { 5, 6, 7, 8, 9 }.Contains(u.Organization.TypeId))
            .Select(u => new UserDto
            {
                Id = u.Id,
                NickName = u.Nickname
            })
            .ToListAsync();

        return result;
    }
    
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        user.Nickname = dto.Nickname;
        user.Mobile = dto.Mobile;
        user.Unit = dto.Unit;
        user.Position = dto.Position;

        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ChangePasswordAsync(Guid id, string oldPassword, string newPassword)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        // ✅ 驗證舊密碼
        if (!Argon2.Verify(user.PasswordHash, oldPassword))
        {
            throw new ArgumentException("舊密碼錯誤");
        }

        // ✅ 更新新密碼（使用 Argon2 雜湊）
        user.PasswordHash = Argon2.Hash(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;
        
        // ✅ 更新新密碼（使用 Argon2 雜湊）
        user.PasswordHash = Argon2.Hash(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> SendVerificationCodeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email 不可為空", nameof(email));

        // 產生 8 碼驗證碼
        var verificationCode = Generate8DigitNumber();
        var redisKey = $"email_verification:{email}";

        // 儲存驗證碼，設定 5 分鐘過期，並初始化錯誤次數
        var data = new VerificationData { Code = verificationCode, Attempts = 0 };
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        await _redisDb.StringSetAsync(redisKey, JsonSerializer.Serialize(data, options), TimeSpan.FromMinutes(5));

        bool emailSent = await _emailService.SendVerificationEmailCodeAsync(email, verificationCode);
        if (!emailSent)
        {
            _logger.LogError("無法發送驗證碼至 {Email}", email);
            return false;
        }

        _logger.LogInformation("已發送驗證碼至 {Email}: {Code}", email, verificationCode);
        return true;
    }
    
    public async Task<bool> VerifyEmailCodeAsync(string email, string inputCode)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(inputCode))
            throw new ArgumentException("Email 和驗證碼不可為空");

        var redisKey = $"email_verification:{email}";

        var storedData = await _redisDb.StringGetAsync(redisKey);
        if (string.IsNullOrEmpty(storedData))
        {
            _logger.LogWarning("驗證碼不存在或已過期: {Email}", email);
            return false;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var data = JsonSerializer.Deserialize<VerificationData>(storedData, options);

        if (data == null)
        {
            _logger.LogWarning("無法解析存儲的驗證數據: {Email}", email);
            await _redisDb.KeyDeleteAsync(redisKey); // 刪除 Redis 記錄
            return false;
        }

        if (data.Attempts >= 3)
        {
            _logger.LogWarning("驗證碼已達最大嘗試次數: {Email}", email);
            await _redisDb.KeyDeleteAsync(redisKey); // 刪除 Redis 記錄
            return false;
        }

        if (data.Code != inputCode)
        {
            data.Attempts += 1;
            await _redisDb.StringSetAsync(redisKey, JsonSerializer.Serialize(data, options), TimeSpan.FromMinutes(5));
            _logger.LogWarning("驗證碼輸入錯誤 (第 {Attempts} 次): {Email}", data.Attempts, email);
            return false;
        }

        // 驗證成功，刪除 Redis 記錄
        await _redisDb.KeyDeleteAsync(redisKey);
        _logger.LogInformation("驗證碼驗證成功: {Email}", email);
        return true;
    }
}