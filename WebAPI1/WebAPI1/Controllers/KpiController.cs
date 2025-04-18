using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class KpiController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<KpiController> _logger;
    private readonly IKpiService _kpiService;

    public KpiController(isha_sys_devContext db, ILogger<KpiController> logger, IKpiService kpiService)
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
    
    [HttpPost("import-test-single")]
    public async Task<IActionResult> ImportTestSingle()
    {
        try
        {
            var result = await _kpiService.InsertKpiRecordAsync(new KpiExcelRow
            {
                CompanyId = 1,
                KpiCategoryName = "基礎型",
                FieldName = "PSM",
                IndicatorNumber = 1,
                IndicatorName = "製程安全資訊之完整性",
                DetailItemName = "需具備製程安全資訊文件數",
                Unit = "件",
                IsApplied = true,
                BaselineYear = "110年",
                BaselineValue = 25,
                TargetValue = 25,
                Remarks = "",
                ExecutionByYear = new Dictionary<int, int>
                {
                    { 111, 25 },
                    { 112, 25 },
                    { 113, 25 }
                }
            });

            return Ok(new { success = true, message = result });
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
            var result = await _kpiService.InsertKpiData(row);
            return Ok(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    
    [HttpGet("display")]
    public async Task<IActionResult> GetDisplayKpis([FromQuery] int? organizationId, [FromQuery] int? startYear, [FromQuery] int? endYear)
    {
        var result = await _kpiService.GetKpiDisplayAsync(organizationId, startYear, endYear);
        return Ok(new { success = true, data = result });
    }
}