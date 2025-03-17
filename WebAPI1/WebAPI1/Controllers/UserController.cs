using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI1.Migrations;
using WebAPI1.Models;

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
        public UserController(isha_sys_devContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            // 驗證用戶名和密碼
            var user = _db.UserInfoNames
                .FirstOrDefault(u => u.Email == loginDto.Usermail && u.Password == loginDto.Password);
        
            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Email or password is incorrect"
                });
            }

            
            // 創建 JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            };

            // 讀取 JwtSettings 配置
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // 返回 JWT 和用戶信息
            return Ok(new 
            { 
                success = true,
                message = "",
                token = jwt,
                username = user.Username,
                email = user.Email
            });
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
    }
}
