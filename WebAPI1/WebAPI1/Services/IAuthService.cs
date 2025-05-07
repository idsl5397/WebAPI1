using System.Security.Claims;

namespace WebAPI1.Services;

public interface IAuthService
{
    string GenerateAccessToken(string userId, string email, string nickname);
    string GenerateRefreshToken(string userId);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
    void SetRefreshTokenCookie(string refreshToken);
}