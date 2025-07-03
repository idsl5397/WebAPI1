using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Authorization;
using WebAPI1.Entities;
using WebAPI1.Models;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[ApiController]
[Route("[controller]")]
public class ImprovementController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<ImprovementController> _logger;
    private readonly IImprovementService _improvementService;
    
    public ImprovementController(isha_sys_devContext db, ILogger<ImprovementController> logger, IImprovementService improvementService)
    {
        _db = db;
        _logger = logger;
        _improvementService = improvementService;
    }
    
    [HttpGet("list-files")]
    public async Task<IActionResult> ListFiles([FromQuery] int orgId)
    {
        var files = await _improvementService.GetUploadedFilesAsync(orgId);
        return Ok(new { files });
    }
    
    
    [HttpPost("submit-report")]

    public async Task<IActionResult> SubmitReport([FromForm] int orgId, [FromForm] int year, [FromForm] int quarter, [FromForm] IFormFile file)
    {
        try
        {
            // // 🔐 從 JWT Token 中解析 UserId
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            // {
            //     return Unauthorized(new { success = false, message = "無效的使用者憑證" });
            // }
            int userId = 1; // ← 可換成 JWT 或登入資訊取得

            // 🧾 執行服務層提交邏輯
            var success = await _improvementService.SubmitReportAsync(orgId, year, quarter, file, userId);
            return Ok(new { success });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}