using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Context;
using WebAPI1.Entities;

namespace WebAPI1.Controllers;

public class TestController: ControllerBase
{
    private readonly ISHAuditDbcontext _db;
    
    public TestController(ISHAuditDbcontext db)
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