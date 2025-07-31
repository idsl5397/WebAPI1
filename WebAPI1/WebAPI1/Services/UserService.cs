using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI1.Context;
using WebAPI1.Controllers;
using WebAPI1.Entities;

namespace WebAPI1.Services;

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

public interface IUserService
{
    Task<bool> CreateUserAsync(RegisterUserDto dto);
    Task<LoginResultDto> VerifyUserLoginAsync(LoginDto dto);
    
    //獲取委員資料名單
    Task<List<UserDto>> GetCommitteeUsers();
}
public class UserService:IUserService
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;
    public UserService(
        ISHAuditDbcontext db,
        ILogger<UserService> logger,IConfiguration configuration,IAuthService authService)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
        _authService = authService;
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
}