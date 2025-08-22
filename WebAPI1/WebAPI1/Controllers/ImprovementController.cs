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
    [Authorize]
    public async Task<IActionResult> ListFiles([FromQuery] int orgId)
    {
        var files = await _improvementService.GetUploadedFilesAsync(orgId);
        return Ok(new { files });
    }
    
    [Authorize]
    [HttpPost("submit-report")]

    public async Task<IActionResult> SubmitReport([FromForm] int orgId, [FromForm] int year, [FromForm] int quarter, [FromForm] string filepath)
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
            var success = await _improvementService.SubmitReportAsync(orgId, year, quarter, filepath, userId);
            return Ok(new { success });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpDelete("delete-file")]
    [Authorize]
    public async Task<IActionResult> DeleteFile(
        [FromQuery] string filePath,
        [FromQuery] int? orgId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest(new { success = false, message = "filePath 不可為空" });

        try
        {
            var ok = await _improvementService.DeleteFileAsync(filePath, orgId, ct);
            if (!ok)
                return NotFound(new { success = false, message = "找不到檔案或關聯" });

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"刪除失敗：{ex.Message}" });
        }
    }
    
    [HttpGet("download-file")]
    [Authorize]
    public async Task<IActionResult> DownloadFile([FromQuery] string fileName, [FromQuery] string orgId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return BadRequest("missing fileName");
        

        var opened = await _improvementService.OpenReadAsync(orgId, fileName, ct);
        if (opened == null) return NotFound();

        var (stream, contentType, safeName) = opened.Value;

        // Content-Disposition（支援 UTF-8 檔名）
        // Response.Headers["Content-Disposition"] = $"attachment; filename=\"{safeName}\"; filename*=UTF-8''{Uri.EscapeDataString(safeName)}";
        return File(stream, contentType, safeName);
    }
    
    // [HttpGet("DownloadFile")]
    // [SwaggerOperation(Summary = "下載檔案", Description = "下載指定路徑的檔案")]
    // public async Task<IActionResult> DownloadFile([FromQuery] string path)
    // {
    //     var result = await _fileService.DownloadFile(path);
    //     if (result.Success)
    //     {
    //         var downloadResult = result.Data;
    //         return File(downloadResult.FileStream, downloadResult.MimeType, downloadResult.FileName);
    //     }
    //
    //     return BadRequest(new { success = false, message = result.Message, data = result.Data });
    // }
}