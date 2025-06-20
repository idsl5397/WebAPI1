using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class ReportController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<ReportController> _logger;
    private readonly IReportService _reportService;
    
    public ReportController(ILogger<ReportController> logger,isha_sys_devContext db,IReportService reportService)
    {
        _db = db;
        _logger = logger;
        _reportService = reportService;
    }
    
    [HttpGet("GetCompletionRates")]
    public async Task<IActionResult> GetCompletionRates([FromQuery] int? organizationId)
    {
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
    public async Task<ActionResult<List<CompanyCompletionRankingDto>>> GetCompletionRanking([FromQuery] int? topN = null)
    {
        var result = await _reportService.GetCompletionRankingAsync(topN);
        return Ok(result);
    }
    
    [HttpGet("uncompleted-suggestions")]
    public async Task<ActionResult<List<SuggestUncompletedDto>>> GetUncompletedSuggestions([FromQuery] int organizationId)
    {
        var result = await _reportService.GetUncompletedSuggestionsAsync(organizationId);
        return Ok(result);
    }
}