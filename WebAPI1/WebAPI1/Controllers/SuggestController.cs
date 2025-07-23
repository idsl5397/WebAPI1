using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Context;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class SuggestController: ControllerBase
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<SuggestController> _logger;
    private readonly ISuggestService _suggestService;
    private readonly IUserService _userService;
    private readonly IKpiService _kpiService;
    
    public SuggestController(ILogger<SuggestController> logger,ISHAuditDbcontext db,ISuggestService suggestService,IUserService userService, IKpiService kpiService)
    {
        _db = db;
        _suggestService = suggestService;
        _logger = logger;
        _userService = userService;
        _kpiService = kpiService;
    }

    
    [HttpGet("GetAllSuggest")]
    public async Task<ActionResult<IEnumerable<SuggestDto>>> GetAll([FromQuery] int? organizationId, [FromQuery] int? startYear, [FromQuery] int? endYear, [FromQuery] string? keyword)
    {
        var suggests = await _suggestService.GetAllSuggestsAsync(organizationId, startYear, endYear, keyword);
        return Ok(suggests);
    }
    
    [HttpGet("GetAllSuggestData")]
    [Authorize]  // 確保有登入
    public async Task<ActionResult<IEnumerable<SuggestDto>>> GetAllSuggestData([FromQuery] int? organizationId, [FromQuery] string? keyword)
    {
        // 從 JWT 取得 userId
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("無效的使用者身份");

        // 查出這位使用者的所屬組織與類型
        var user = await _db.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound("找不到使用者資料");

        var orgTypeId = user.Organization.TypeId;

        // TypeId = 1 為 admin，否則強制鎖定為自己組織
        int? finalOrgId = orgTypeId == 1
            ? organizationId    // admin：有傳就查指定，沒傳就查全部
            : user.OrganizationId;

        var suggests = await _suggestService.GetAllSuggestDatesAsync(finalOrgId, keyword);
        return Ok(suggests);
    }
    
    [HttpGet("GetSuggestDetail/{id}")]
    [Authorize]
    public async Task<IActionResult> GetSuggestDetail(int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _db.Users.Include(u => u.Organization).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        var result = await _suggestService.GetSuggestDetailAsync(id);
        if (result == null)
            return NotFound();

        // 🔐 權限驗證：非 admin 就只能看自己組織
        if (user.Organization.TypeId != 1 && result.OrganizationId != user.OrganizationId)
            return Forbid();

        return Ok(result);
    }
    
    [HttpGet("GetCommitteeUsers")]
    public async Task<ActionResult<IEnumerable<SuggestDto>>> GetCommitteeUsers()
    {
        var suggests = await _userService.GetCommitteeUsers();
        return Ok(suggests);
    }
    
    [HttpGet("GetAllCategories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var fields = await _kpiService.GetAllFieldsAsync();
        return Ok(fields);
    }
    
    [HttpPost("import-singleSuggest")]
    public async Task<IActionResult> ImportSingleSuggest([FromBody] AddSuggestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "資料驗證失敗" });

        var result = await _suggestService.ImportSingleSuggestAsync(dto);
        if (result.Success)
            return Ok(new { success = true, message = result.Message });

        return BadRequest(new { success = false, message = result.Message });
    }
    
    // 上傳Excel並預覽前五筆
    [HttpPost("import-preview")]
    public async Task<IActionResult> ImportPreview([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請上傳檔案。");

        var previewData = await _suggestService.ParseExcelAsync(file.OpenReadStream());
        return Ok(previewData);
    }
    
    /// <summary>
    /// 批次匯入KPI資料
    /// </summary>
    [HttpPost("import-confirm")]
    public async Task<IActionResult> BatchImportConfirm([FromBody] List<SuggestmanyRow> rows)
    {
        if (rows == null || rows.Count == 0)
            return BadRequest(new { success = false, message = "無匯入資料" });

        var result = await _suggestService.BatchInsertSuggestAsync(rows);

        return Ok(new
        {
            success = result.Success,
            message = result.Message,
            successCount = result.SuccessCount,
            failCount = result.FailCount
        });
    }
    
    [HttpGet("download-template")]
    public async Task<IActionResult> DownloadTemplate([FromQuery] int organizationId)
    {
        var (fileName, content) = await _suggestService.GenerateTemplateAsync(organizationId);
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
    
    [HttpPost("fullpreview-for-report")]
    public async Task<IActionResult> PreviewFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請上傳檔案");

        var previewData = await _suggestService.PreviewAsync(file);
        return Ok(previewData);
    }
    
    [HttpPost("fullsubmit-for-report")]
    public async Task<IActionResult> ImportFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請上傳檔案");

        var (success, message) = await _suggestService.ImportAsync(file);

        if (success)
            return Ok(new { message });

        return StatusCode(500, new { message });
    }
    
    [HttpGet("selectOrg-for-report")]
    public async Task<IActionResult> GetReportsByOrganization(int organizationId)
    {
        var data = await _suggestService.GetReportsByOrganizationAsync(organizationId);
        return Ok(data);
    }
    
    [HttpPut("update-report")]
    public async Task<IActionResult> UpdateReport([FromBody] List<SuggestDto> reports)
    {
        _logger.LogInformation("收到更新筆數：{count}", reports.Count);
        if (reports == null || !reports.Any())
            return BadRequest(new { success = false, message = "請提供要更新的報告資料" });

        var result = await _suggestService.UpdateSuggestReportsAsync(reports);

        if (!result)
            return BadRequest(new { success = false, message = "更新失敗，可能找不到對應的報告資料" });

        return Ok(new { success = true });
    }
}