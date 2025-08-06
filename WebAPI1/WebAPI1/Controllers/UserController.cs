using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI1.Context;
using WebAPI1.Entities;
using WebAPI1.Services;

namespace WebAPI1.Controllers
{
    
    public class LoginDto
    {
        public string Usermail { get; set; }
        public string Password { get; set; }
    }
    
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ISHAuditDbcontext _db;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public UserController(ISHAuditDbcontext db, IConfiguration configuration, IUserService userService)
        {
            _db = db;
            _configuration = configuration;
            _userService = userService;
        }
        
        private string GenerateJwtToken(string username, SymmetricSecurityKey key)
        {
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _userService.VerifyUserLoginAsync(loginDto);
            if (!result.Success)
                return Unauthorized(new { result.Message });

            return Ok(result
            );
        }
        
        
        [HttpPost("test")]
        public IActionResult test()
        {
            string hashedPassword = Argon2.Hash("T!es@t7QwQ033",
                type: Argon2Type.HybridAddressing,  
                timeCost: 2,  
                memoryCost: 32768,
                parallelism: 2);
            // var testUser = new User
            // {
            //     Id = Guid.NewGuid(),
            //     Username = "testuser",
            //     PasswordHash = hashedPassword, // 使用 Argon2 雜湊密碼
            //     Nickname = "測試用戶",
            //     OrganizationId = 1,
            //     Email = "testuser@example.com",
            //     Unit = "測試部門",
            //     Position = "測試職位",
            //     RegistrationDomain = "example.com",
            //     CreatedAt = tool.GetTaiwanNow(),
            //     UpdatedAt = tool.GetTaiwanNow(),
            //     IsActive = true,
            //     LastLoginAt = null,
            //     PasswordChangedAt = tool.GetTaiwanNow(),
            //     PasswordExpiresAt = tool.GetTaiwanNow().AddMonths(3),
            //     ForceChangePassword = false,
            //     PasswordFailedAttempts = 0,
            //     PasswordLockedUntil = null,
            //     LastPasswordExpiryReminder = null
            // };
            //
            // _db.Users.Add(testUser);
            // _db.SaveChanges();
            
            
            // 更新所有使用者的密碼
            var users = _db.Users.ToList(); // 取得所有使用者
            foreach (var user in users)
            {
                user.PasswordHash = hashedPassword;
            }
            _db.SaveChanges();

            return Ok(new { message = "Test Successful" });
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            var success = await _userService.UpdateUserAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }
        
        [HttpPut("{id}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var success = await _userService.ChangePasswordAsync(id, dto.OldPassword, dto.NewPassword);
                if (!success) return NotFound();
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }
        
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.NewPassword))
                return BadRequest(new { message = "Email 與新密碼不能為空" });

            var success = await _userService.ResetPasswordAsync(dto.Email, dto.NewPassword);

            if (!success)
                return NotFound(new { message = "找不到使用者" });

            return Ok(new { message = "密碼重設成功" });
        }
        
        [HttpPost("SendVerificationEmail")]
        public async Task<IActionResult> SendVerificationEmail([FromBody] EmailRequestDto request)
        {
            bool success = await _userService.SendVerificationCodeAsync(request.Email);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "驗證碼已發送"
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "無法發送驗證碼"
            });
        }
        
        [HttpPost("VerifyEmailCode")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyCodeDto request)
        {
            bool success = await _userService.VerifyEmailCodeAsync(request.Email, request.Code);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "驗證成功"
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "驗證碼錯誤或已失效"
            });
        }
    }
}
