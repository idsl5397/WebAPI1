using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[ApiController]
[Route("Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("RefreshToken")]
    public IActionResult RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh Token 不存在" });
        }

        var principal = _authService.ValidateRefreshToken(refreshToken);
        if (principal == null)
        {
            return Unauthorized(new { message = "Refresh Token 無效或過期" });
        }
        
        // ✅ 驗證 token_type 是否為 refresh
        var tokenType = principal.FindFirst("token_type")?.Value;
        if (tokenType != "refresh")
        {
            return Unauthorized(new { message = "Token 類型錯誤，僅允許 Refresh Token" });
        }
        
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? "";

        var newAccessToken = _authService.GenerateAccessToken(userId, email, name);
        var newRefreshToken = _authService.GenerateRefreshToken(userId);
        _authService.SetRefreshTokenCookie(newRefreshToken);

        return Ok(new { accessToken = newAccessToken });
    }
}