using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI1.Entities;
using WebAPI1.Models;
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
        private readonly isha_sys_devContext _db;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public UserController(isha_sys_devContext db, IConfiguration configuration, IUserService userService)
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

            return Ok(new
            {
                success = true,
                message = result.Message,
                token = result.Token,
                nickname = result.Nickname,
                email = result.Email
            });
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
            //     CreatedAt = DateTime.UtcNow,
            //     UpdatedAt = DateTime.UtcNow,
            //     IsActive = true,
            //     LastLoginAt = null,
            //     PasswordChangedAt = DateTime.UtcNow,
            //     PasswordExpiresAt = DateTime.UtcNow.AddMonths(3),
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
    }
}
