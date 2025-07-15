using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Authorization;
using WebAPI1.Context;
using WebAPI1.Entities;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[ApiController]
[Route("[controller]")]
public class ImprovementController: ControllerBase
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<ImprovementController> _logger;
    private readonly IImprovementService _improvementService;
    
    public ImprovementController(ISHAuditDbcontext db, ILogger<ImprovementController> logger, IImprovementService improvementService)
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
    
    [Authorize]
    [HttpPost("submit-report")]

    public async Task<IActionResult> SubmitReport([FromForm] int orgId, [FromForm] int year, [FromForm] int quarter, [FromForm] IFormFile file)
    {
        try
        {
            // 🔐 從 JWT Token 中解析 UserId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized(new { success = false, message = "無效的使用者憑證" });
            }
            // int userId = 1; // ← 可換成 JWT 或登入資訊取得

            // 🧾 執行服務層提交邏輯
            var success = await _improvementService.SubmitReportAsync(orgId, year, quarter, file, userId);
            return Ok(new { success });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpDelete("delete-file")]
    public async Task<IActionResult> DeleteFile([FromQuery] string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest(new { success = false, message = "檔案名稱不可為空" });

        try
        {
            var result = await _improvementService.DeleteFileAsync(fileName);
            if (result)
                return Ok(new { success = true });
            else
                return NotFound(new { success = false, message = "找不到檔案" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"刪除失敗：{ex.Message}" });
        }
    }
}