using System.Security.Claims;

namespace WebAPI1.Services;

public interface IAuthService
{
    Task<string> GenerateAccessToken(string userId, string email, string nickname, List<string> permissions);
    string GenerateRefreshToken(string userId);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
    void SetRefreshTokenCookie(string refreshToken);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<UserProfileDto?> GetCurrentUserAsync(ClaimsPrincipal user);
}