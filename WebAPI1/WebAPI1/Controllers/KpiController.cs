using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Context;
using WebAPI1.Entities;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class KpiController: ControllerBase
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<KpiController> _logger;
    private readonly IKpiService _kpiService;

    public KpiController(ISHAuditDbcontext db, ILogger<KpiController> logger, IKpiService kpiService)
    {
        _db = db;
        _logger = logger;
        _kpiService = kpiService;
    }
    
    [HttpPost("create-kpi-field")]
    public async Task<IActionResult> CreateKpiField([FromBody] CreateKpiFieldDto dto)
    {
        try
        {
            var result = await _kpiService.CreateKpiFieldAsync(dto);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("import-single")]
    public async Task<IActionResult> InsertKpiData([FromBody] KpisingleRow row)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "資料驗證失敗" });
        try
        {
            var (success, message) = await _kpiService.InsertKpiData(row);
            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    
    [HttpGet("display")]
    public async Task<IActionResult> GetKpiDisplayAsync([FromQuery] int? organizationId, [FromQuery] int? startYear, [FromQuery] int? endYear, [FromQuery] string? startQuarter, [FromQuery] string? endQuarter, [FromQuery] string? keyword)
    {
        var result = await _kpiService.GetKpiDisplayAsync(organizationId, startYear, endYear, startQuarter, endQuarter, keyword);
        return Ok(new { success = true, data = result });
    }
    [HttpGet("displayPage")]
    public async Task<IActionResult> GetKpiDisplayPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? organizationId = null,
        [FromQuery] string? categoryName = null,
        [FromQuery] string? fieldName = null)
    {
        try
        {
            var result = await _kpiService.GetKpiDisplayPagedAsync(
                page: page,
                pageSize: pageSize,
                organizationId: organizationId,
                categoryName: categoryName,
                fieldName: fieldName
            );

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = "查詢發生錯誤",
                detail = ex.Message
            });
        }
    }
 
    [HttpGet("kpidata-for-report")]
    public async Task<IActionResult> GetKpiDataDtoByOrganizationIdAsync(int organizationId, int year, string quarter)
    {
        var kpiDataList = await _kpiService.GetKpiDataDtoByOrganizationIdAsync(organizationId, year, quarter);
    
        if (kpiDataList.Count == 0)
            return Ok(new { success = false, message = "該單位目前無 KPI 可填報", data = new List<KpiDataDto>() });

        return Ok(new { success = true, data = kpiDataList });
    }
    
    [HttpPost("submit-kpi-report")]
    public async Task<IActionResult> SubmitKpiReportsAsync([FromBody] List<KpiReportInsertDto> reports)
    {
        var result = await _kpiService.SubmitKpiReportsAsync(reports);
        return Ok(new
        {
            success = result.Success,
            message = result.Message
        });
    }
    [HttpPost("save-kpi-report")]
    public async Task<IActionResult> SaveDraft([FromBody] List<KpiReportInsertDto> reports)
    {
        var (success, message) = await _kpiService.SaveDraftReportsAsync(reports);
        return Ok(new { success, message });
    }
    
    [HttpGet("load-kpi-draft")]
    public async Task<IActionResult> LoadKpiDraft(int organizationId, int year, string quarter)
    {
        var result = await _kpiService.LoadDraftReportsAsync(organizationId, year, quarter);
        return Ok(new { success = true, data = result });
    }
    
    // 上傳Excel並預覽前五筆
    [HttpPost("import-preview")]
    public async Task<IActionResult> ImportPreview([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請上傳檔案。");

        var previewData = await _kpiService.ParseExcelAsync(file.OpenReadStream());
        return Ok(previewData);
    }
    
    /// <summary>
    /// 批次匯入KPI資料
    /// </summary>
    [HttpPost("import-confirm")]
    public async Task<IActionResult> BatchImportConfirm([FromBody] List<KpimanyRow> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            return BadRequest("匯入資料為空！");
        }

        var (success, message) = await _kpiService.BatchInsertKpiDataAsync(rows);

        if (success)
        {
            return Ok(new
            {
                success = true,
                message
            });
        }
        else
        {
            return BadRequest(new
            {
                success = false,
                message
            });
        }
    }
    
    
    // 系統管理員上傳Excel並預覽前五筆
    [HttpPost("full-import-preview")]
    public async Task<IActionResult> ParseFullImportExcel([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請上傳檔案。");

        var previewData = _kpiService.ParseFullImportExcel(file.OpenReadStream());
        return Ok(previewData);
    }
    
    /// <summary>
    /// 系統管理員批次匯入KPI資料
    /// </summary>
    [HttpPost("full-import-confirm")]
    public async Task<IActionResult> BatchFullKpiDataAsync([FromBody] List<KpiimportexcelDto> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            return BadRequest("匯入資料為空！");
        }

        var (success, message) = await _kpiService.BatchFullKpiDataAsync(rows);

        if (success)
        {
            return Ok(new
            {
                success = true,
                message
            });
        }
        else
        {
            return BadRequest(new
            {
                success = false,
                message
            });
        }
    }
    
    [HttpDelete("delete-kpidata-by-organization/{organizationId}")]
    public async Task<IActionResult> DeleteKpiDataByOrganization(int organizationId)
    {
        try
        {
            var targetData = await _db.KpiDatas
                .Where(d => d.OrganizationId == organizationId)
                .ToListAsync();

            if (!targetData.Any())
            {
                return NotFound($"找不到 OrganizationId = {organizationId} 的資料。");
            }

            _db.KpiDatas.RemoveRange(targetData);
            await _db.SaveChangesAsync();

            return Ok($"✅ 成功刪除 {targetData.Count} 筆 KpiData 資料。");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ 刪除失敗，原因：{ex.Message}");
        }
    }
    
    [HttpGet("report-history")]
    public async Task<IActionResult> GetReportHistory([FromQuery] int kpiDataId)
    {
        var reports = await _db.KpiReports
            .Where(r => r.KpiDataId == kpiDataId)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Period)
            .ToListAsync();

        return Ok(reports);
    }
    
    [HttpGet("kpiCycle-list")]
    public async Task<IActionResult> GetCycleOptions()
    {
        var cycles = await _kpiService.GetAllCyclesAsync();
        return Ok(cycles);
    }
    
    [HttpGet("download-template")]
    public async Task<IActionResult> DownloadTemplate([FromQuery] int organizationId)
    {
        if (organizationId <= 0)
            return BadRequest("缺少有效的 organizationId");

        var (fileName, content) = await _kpiService.GenerateTemplateAsync(organizationId);

        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
    
    [HttpPost("fullpreview-for-report")]
    public async Task<IActionResult> PreviewImportAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("未上傳檔案");

        var previewList = await _kpiService.ReadPreviewAsync(file);
        return Ok(previewList);
    }
    
    [HttpPost("fullsubmit-for-report")]
    public async Task<IActionResult> FullImportAsync(IFormFile file, int organizationId, int year, string quarter)
    {
        var (inserted, updated) = await _kpiService.ImportAsync(file, organizationId, year, quarter);
        return Ok(new { inserted, updated });
    }
}