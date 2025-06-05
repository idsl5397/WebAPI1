using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WebAPI1.Context;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Services;

public class KpiCompletionRateDto
{
    public int KpiFieldId { get; set; }
    public string KpiFieldName { get; set; } // e.g. 製程安全管理 (PSM)
    public double CompletionRate { get; set; }
}
public class CompanySuggestionStatsDto
{
    public string CompanyName { get; set; }
    public int SuggestionCount { get; set; }
}
public class KpiFieldSuggestionCountDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}
public interface IReportService
{
    Task<List<KpiCompletionRateDto>> GetKpiCompletionRatesAsync(int? organizationId);
    Task<List<int>> GetOrganizationIdsWithSuggestDataAsync();
    Task<List<KpiFieldSuggestionCountDto>> GetSuggestionKpiFieldCountsAsync(int? organizationId);

    Task<List<CompanySuggestionStatsDto>> GetTop10CompanySuggestionStatsAsync(int? year = null,
        int? suggestionTypeId = null);
}

public class ReportService:IReportService
{
    private readonly isha_sys_devContext _context;
    private readonly ILogger<ReportService> _logger;
    
    public ReportService(isha_sys_devContext context,ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<int>> GetOrganizationIdsWithSuggestDataAsync()
    {
        var orgIds = await _context.SuggestDatas
            .Where(s => s.OrganizationId != null)
            .Select(s => s.OrganizationId.Value)
            .Distinct()
            .ToListAsync();

        return orgIds;
    }
    
    public async Task<List<KpiCompletionRateDto>> GetKpiCompletionRatesAsync(int? organizationId)
    {
        var query = _context.SuggestDatas
            .Include(s => s.KpiField)
            .Where(s => s.KpiFieldId != null && s.KpiField != null);

        if (organizationId.HasValue)
        {
            query = query.Where(s => s.OrganizationId == organizationId.Value);
        }

        // 篩出只要 IsAdopted == 是 的資料，才列入分母
        var filtered = await query
            .Where(s => s.IsAdopted == IsAdopted.是)
            .ToListAsync();

        var result = filtered
            .GroupBy(s => new
            {
                Id = s.KpiField.Id,
                Field = s.KpiField.field ?? "未知類型",
                EnField = s.KpiField.enfield ?? "Unknown"
            })
            .Select(g =>
            {
                var total = g.Count(); // 分母：已採納的總數
                var adoptedAndCompleted = g.Count(s => s.Completed == IsAdopted.是); // 分子：已採納且完成

                var rate = total == 0 ? 0 : Math.Round((double)adoptedAndCompleted / total * 100, 1);

                return new KpiCompletionRateDto
                {
                    KpiFieldId = g.Key.Id,
                    KpiFieldName = $"{g.Key.Field} ({g.Key.EnField})",
                    CompletionRate = rate
                };
            })
            .ToList();

        return result;
    }
    
    public async Task<List<KpiFieldSuggestionCountDto>> GetSuggestionKpiFieldCountsAsync(int? organizationId)
    {
        var query = _context.SuggestDatas
            .Include(s => s.KpiField)
            .Where(s => s.KpiFieldId != null && s.KpiField != null);

        if (organizationId.HasValue)
        {
            query = query.Where(s => s.OrganizationId == organizationId.Value);
        }

        var result = await query
            .GroupBy(s => new {
                Field = s.KpiField.field ?? "未知類型",
                EnField = s.KpiField.enfield ?? "Unknown"
            })
            .Select(g => new KpiFieldSuggestionCountDto
            {
                Category = $"{g.Key.Field} ({g.Key.EnField})",
                Count = g.Count()
            })
            .ToListAsync();

        return result;
    }
    
    public async Task<List<CompanySuggestionStatsDto>> GetTop10CompanySuggestionStatsAsync(int? year = null, int? suggestionTypeId = null)
    {
        var query = _context.SuggestDatas
            .Include(s => s.Organization)
            .Where(s => s.Organization != null);

        if (year.HasValue)
        {
            query = query.Where(s => s.Date.Year == year.Value);
        }

        if (suggestionTypeId.HasValue)
        {
            query = query.Where(s => s.SuggestionTypeId == suggestionTypeId.Value);
        }

        var result = await query
            .GroupBy(s => new { s.Organization.Id, s.Organization.Name })
            .Select(g => new CompanySuggestionStatsDto
            {
                CompanyName = g.Key.Name,
                SuggestionCount = g.Count()
            })
            .OrderByDescending(x => x.SuggestionCount)
            .Take(10)
            .ToListAsync();

        return result;
    }
}