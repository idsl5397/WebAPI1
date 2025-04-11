using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class RegisterController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<RegisterController> _logger;
    private readonly IOrganizationService _organizationService;
    private readonly IUserService _userService;
    
    public RegisterController(ILogger<RegisterController> logger, IOrganizationService organizationService,isha_sys_devContext db, IUserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _organizationService = organizationService;
        _db = db;
        _userService = userService;
    }
    public class EmailDto
    {
        public string Email { get; set; }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] EmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email 不能為空" });
        }
        // 檢查 Email格式是否正確
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        if (!emailRegex.IsMatch(dto.Email))
        {
            return BadRequest(new { message = "Email 格式錯誤" });
        }
        
        // 檢查是否已註冊過此 Email
        var isEmailRegistered = _db.Users.Any(u => u.Email.ToLower() == dto.Email.ToLower());
        if (isEmailRegistered)
        {
            return Conflict(new { message = "此 Email 已註冊過" });
        }
        
        var domain = dto.Email.Split('@').Last().ToLower();
        
        //調用服務 從domain搜尋組織id
        try
        {
            var orgInfo = await _organizationService.GetOrganizationTreeByDomainAsync(domain);
            return Ok(orgInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all organizations");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("insert-user")]
    public async Task<IActionResult> InsertUser([FromBody] RegisterUserDto dto)
    {
        var success = await _userService.CreateUserAsync(dto);
        if (!success)
        {
            return StatusCode(500, "註冊失敗，請稍後再試");
        }

        return Ok("註冊成功");
    }
    
}