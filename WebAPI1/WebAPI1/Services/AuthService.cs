using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Context;

namespace WebAPI1.Services;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = "";
    public string Email { get; set; } = "";
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = "";
    public int OrganizationTypeId { get; set; }
    public string Role { get; set; } = "";  // "admin" or "company"
}

public class AuthService: IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISHAuditDbcontext _context;

    public AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ISHAuditDbcontext context)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public Task<string> GenerateAccessToken(string userId, string email, string nickname, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nickname),
            new Claim("token_type", "access"),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 🔑 加入每一個權限作為 claims（ClaimTypes.Role 或自定 key）
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: tool.GetTaiwanNow().AddMinutes(300),
            signingCredentials: creds
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    
    public string GenerateRefreshToken(string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("token_type", "refresh")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: tool.GetTaiwanNow().AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);

            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out _);

            // ✅ 加上這段判斷是否為 Refresh Token
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (tokenType != "refresh")
            {
                return null; // 若不是 refresh token，則視為驗證失敗
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    // public void SetRefreshTokenCookie(string refreshToken)
    // {
    //     var context = _httpContextAccessor.HttpContext;
    //     if (context != null)
    //     {
    //         context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
    //         {
    //             HttpOnly = true,
    //             Secure = true,
    //             SameSite = SameSiteMode.Strict,
    //             Path = "/",
    //             Expires = tool.GetTaiwanNow().AddDays(7)
    //         });
    //     }
    // }
    
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToListAsync();

        return permissions;
    }
    
    public async Task<UserProfileDto?> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return null;

        var dbUser = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (dbUser == null || dbUser.Organization == null)
            return null;

        var typeId = dbUser.Organization.TypeId;
        var role = typeId == 1 ? "admin" : "company";

        return new UserProfileDto
        {
            UserId = dbUser.Id,
            Nickname = dbUser.Nickname,
            Email = dbUser.Email,
            OrganizationId = dbUser.OrganizationId,
            OrganizationName = dbUser.Organization.Name,
            OrganizationTypeId = typeId,
            Role = role
        };
    }
}