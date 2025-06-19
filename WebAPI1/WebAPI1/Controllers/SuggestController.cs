using Microsoft.AspNetCore.Mvc;
using WebAPI1.Models;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class SuggestController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<SuggestController> _logger;
    private readonly ISuggestService _suggestService;
    private readonly IUserService _userService;
    private readonly IKpiService _kpiService;
    
    public SuggestController(ILogger<SuggestController> logger,isha_sys_devContext db,ISuggestService suggestService,IUserService userService, IKpiService kpiService)
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
}