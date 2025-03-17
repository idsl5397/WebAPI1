using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Controllers;

public class RegisterController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<RegisterController> _logger;
    
    public RegisterController(isha_sys_devContext db)
    {
        _db = db;
        
    }
    

    [HttpPost("verify-email")]
    public IActionResult VerifyEmail(string email)
    {
        if (email == null)
        {
            return BadRequest(new { message = "Email 不能為空" });
        }

        var domain = email.Split('@').LastOrDefault();
        if (string.IsNullOrEmpty(domain))
        {
            return BadRequest(new { message = "Email 格式錯誤" });
        }

        var company = _db.DomainNames
            .Include(d => d.Company)
            .Where(d => d.domain == domain)
            .Select(d => d.Company.Id)
            .FirstOrDefaultAsync();

        if (company == null)
        {
            return NotFound(new { message = "該 Email 網域未註冊" });
        }

        return Ok(new { companyId = company });
    }
    
}