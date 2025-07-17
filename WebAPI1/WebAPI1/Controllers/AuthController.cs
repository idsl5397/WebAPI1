using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> RefreshToken()
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

        var tokenType = principal.FindFirst("token_type")?.Value;
        if (tokenType != "refresh")
        {
            return Unauthorized(new { message = "Token 類型錯誤，僅允許 Refresh Token" });
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? "";

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "使用者資訊不完整" });
        }

        // ⛓ 取得該使用者的權限（你也可以抽成 service）
        var permissions = await _authService.GetUserPermissionsAsync(Guid.Parse(userId));

        var newAccessToken = await _authService.GenerateAccessToken(userId, email, name, permissions);
        var newRefreshToken = _authService.GenerateRefreshToken(userId);
        _authService.SetRefreshTokenCookie(newRefreshToken);

        return Ok(new { accessToken = newAccessToken });
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _authService.GetCurrentUserAsync(User);
        if (profile == null)
            return Unauthorized("無法取得使用者資訊");

        return Ok(profile);
    }
}