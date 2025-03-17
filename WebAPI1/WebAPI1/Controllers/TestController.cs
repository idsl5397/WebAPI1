using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Controllers;

public class TestController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    
    public TestController(isha_sys_devContext db)
    {
        _db = db;
    }

    [HttpGet("GetDomain")]
    public IActionResult GetDomain()
    {
        var query = from domain in _db.DomainNames.AsNoTracking()
            select new
            {
                name = domain.domain,
                Company = domain.Company,
            };

        return Ok(query);
    }
}