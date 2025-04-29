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

    
    [HttpGet("testSentry")]
    public IActionResult GetTestSentry()
    {
        SentrySdk.CaptureMessage("Hello Sentry");
        return Ok();
    }
}