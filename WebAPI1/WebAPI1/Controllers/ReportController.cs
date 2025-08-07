using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Context;
using WebAPI1.Entities;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class ReportController: ControllerBase
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<ReportController> _logger;
    private readonly IReportService _reportService;
    
    public ReportController(ILogger<ReportController> logger,ISHAuditDbcontext db,IReportService reportService)
    {
        _db = db;
        _logger = logger;
        _reportService = reportService;
    }
    
    [HttpGet("GetCompletionRates")]
    [Authorize]
    public async Task<IActionResult> GetCompletionRates([FromQuery] int? organizationId)
    {
        // 取得目前登入使用者的 Id
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        // 撈出使用者與組織資訊
        var user = await _db.Users.Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        // 🔐 權限驗證邏輯
        // TypeId == 1 代表園管局（擁有跨組織查詢權限）
        if (user.Organization.TypeId != 1)
        {
            // 非園管局 → 不可查詢其他組織資料
            if (organizationId.HasValue && organizationId.Value != user.OrganizationId)
                return Forbid();

            // 如果未指定 organizationId，預設查自己的
            organizationId = user.OrganizationId;
        }

        var result = await _reportService.GetKpiCompletionRatesAsync(organizationId);
        return Ok(new { success = true, data = result });
    }
    
    [HttpGet("GetOrganizationsWithSuggestData")]
    public async Task<IActionResult> GetOrganizationsWithSuggestData()
    {
        var result = await _reportService.GetOrganizationIdsWithSuggestDataAsync();
        return Ok(new { success = true, data = result });
    }
    
    [HttpGet("GetKpiFieldSuggestionCount")]
    public async Task<IActionResult> GetKpiFieldSuggestionCount([FromQuery] int? organizationId)
    {
        var result = await _reportService.GetSuggestionKpiFieldCountsAsync(organizationId);
        return Ok(new { success = true, data = result });
    }
    
    [HttpGet("GetTop10CompanySuggestionStats")]
    public async Task<IActionResult> GetTop10CompanySuggestionStats([FromQuery] int? year, [FromQuery] int? suggestionTypeId)
    {
        var result = await _reportService.GetTop10CompanySuggestionStatsAsync(year, suggestionTypeId);
        return Ok(new { success = true, data = result });
    }
    
    /// <summary>
    /// 取得改善建議完成率排名（依公司分類，完成率低者排前）
    /// </summary>
    /// <param name="topN">可選參數，指定只取前 N 名</param>
    /// <returns></returns>
    [HttpGet("completion-ranking")]
    [HasPermission("view-ranking")]
    public async Task<ActionResult<List<CompanyCompletionRankingDto>>> GetCompletionRanking([FromQuery] int? topN = null)
    {
        var result = await _reportService.GetCompletionRankingAsync(topN);
        return Ok(result);
    }
    
    [HttpGet("uncompleted-suggestions")]
    [HasPermission("view-ranking")]
    public async Task<ActionResult<List<SuggestUncompletedDto>>> GetUncompletedSuggestions([FromQuery] int organizationId)
    {
        var result = await _reportService.GetUncompletedSuggestionsAsync(organizationId);
        return Ok(result);
    }
    
    /// <summary>
    /// 取得公司 KPI 達成率排行榜
    /// </summary>
    [Authorize]
    [HttpGet("kpi-ranking")]
    [HasPermission("view-ranking")]
    public async Task<IActionResult> GetKpiRanking(
        [FromQuery] int? startYear,
        [FromQuery] int? endYear,
        [FromQuery] string? startQuarter,
        [FromQuery] string? endQuarter,
        [FromQuery] string? fieldName
    )
    {
        var result = await _reportService.GetKpiRankingAsync(
            startYear: startYear,
            endYear: endYear,
            startQuarter: startQuarter,
            endQuarter: endQuarter,
            fieldName: fieldName
        );

        return Ok(result);
    }
    
    [HttpGet("unmet-kpi")]
    [HasPermission("view-ranking")]
    public async Task<IActionResult> GetUnmetKpis(
        int organizationId,
        int? startYear = null,
        int? endYear = null,
        string? startQuarter = null,
        string? endQuarter = null,
        string? fieldName = null)
    {
        var result = await _reportService.GetUnmetKpisAsync(
            organizationId, startYear, endYear, startQuarter, endQuarter, fieldName);

        return Ok(result);
    }
    
    // [HttpGet("kpi-trend")]
    // public async Task<ActionResult<List<KpiTrendDto>>> GetKpiTrend([FromQuery] int? organizationId, [FromQuery] int startYear = 111, [FromQuery] int endYear = 113)
    // {
    //     var data = await _reportService.GetTrendDataAsync(organizationId, startYear, endYear);
    //     return Ok(data);
    // }
    
    [HttpGet("kpi-trend")]
    public async Task<IActionResult> GetTestKpiDisplayAsync(
        [FromQuery] int? organizationId,
        [FromQuery] int? startYear,
        [FromQuery] int? endYear,
        [FromQuery] string? startQuarter,
        [FromQuery] string? endQuarter,
        [FromQuery] string? fieldName)
    {
        var result = await _reportService.GetTrendDataAsync(
            organizationId,
            startYear,
            endYear,
            startQuarter,
            endQuarter,
            fieldName
        );

        return Ok(result);
    }
}