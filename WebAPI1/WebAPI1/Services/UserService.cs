using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI1.Controllers;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Services;


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
    public string Token { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
}

public interface IUserService
{
    Task<bool> CreateUserAsync(RegisterUserDto dto);
    Task<LoginResultDto> VerifyUserLoginAsync(LoginDto dto);
}
public class UserService:IUserService
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;
    public UserService(
        isha_sys_devContext db,
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
            PasswordHash = hashedPassword, // ✅ hash 密碼
            EmailVerified = true,
            EmailVerifiedAt = null,
            TokenExpiresAt = null,
            ForceChangePassword = true,
            PasswordChangedAt = null,
            PasswordExpiresAt = null,
            PasswordFailedAttempts = 0,
            PasswordLockedUntil = null,
            LastPasswordExpiryReminder = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }
    
    public async Task<LoginResultDto> VerifyUserLoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .Where(u => u.Email == dto.Usermail)
            .Select(u => new { u.Id,u.Nickname, u.Email, u.PasswordHash })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "使用者不存在"
            };
        }

        if (!Argon2.Verify(user.PasswordHash, dto.Password))
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "帳號或密碼錯誤"
            };
        }

        // 產生 JWT
        var accessToken = _authService.GenerateAccessToken(user.Id.ToString(), user.Email, user.Nickname);
        var refreshToken = _authService.GenerateRefreshToken(user.Id.ToString());
        _authService.SetRefreshTokenCookie(refreshToken);
        
        return new LoginResultDto
        {
            Success = true,
            Message = "登入成功",
            Token = accessToken,
            Nickname = user.Nickname,
            Email = user.Email
        };
    }
}