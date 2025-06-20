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
public class CompanyCompletionRankingDto
{
    public int OrganizationId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public int CompletedYes { get; set; }   // Completed == 是

    public int CompletedNo { get; set; }    // Completed == 否

    public int Total => CompletedYes + CompletedNo;

    /// <summary>完成率 (0–1)，若沒有可計算的資料則為 0</summary>
    public decimal CompletionRate { get; set; }
}

public class SuggestUncompletedDto
{
    public int Id { get; set; }
    [Column(TypeName = "date")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime Date { get; set; }
    public string SuggestionContent { get; set; }
    public string KpiField { get; set; }
    public string EventType { get; set; }
    public string RespDept { get; set; }
    public string Remark { get; set; }
    public string IsAdopted { get; set; }
}
public interface IReportService
{
    Task<List<KpiCompletionRateDto>> GetKpiCompletionRatesAsync(int? organizationId);
    Task<List<int>> GetOrganizationIdsWithSuggestDataAsync();
    Task<List<KpiFieldSuggestionCountDto>> GetSuggestionKpiFieldCountsAsync(int? organizationId);

    Task<List<CompanySuggestionStatsDto>> GetTop10CompanySuggestionStatsAsync(int? year = null,
        int? suggestionTypeId = null);
    
    /// <summary>
    /// 取得各公司改善建議完成率排名  
    /// 預設全部列出，可透過 <paramref name="topN"/> 只取前 N 名
    /// </summary>
    Task<IReadOnlyList<CompanyCompletionRankingDto>> GetCompletionRankingAsync(int? topN = null);
    Task<List<SuggestUncompletedDto>> GetUncompletedSuggestionsAsync(int organizationId);
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
    
    public async Task<IReadOnlyList<CompanyCompletionRankingDto>> GetCompletionRankingAsync(int? topN = null)
    {
        // 1. 先把 Adopted=是 的資料取出，並且 Completed 必須是 是/否
        var groupedQuery =
            from s in _context.SuggestDatas.AsNoTracking()
            join o in _context.Organizations.AsNoTracking()
                on s.OrganizationId equals o.Id
            where s.IsAdopted == IsAdopted.是               // ✅ 只統計「已參採」的建議
                  && (s.Completed == IsAdopted.是              // ✅ Completed 只能是 是 或 否
                      || s.Completed == IsAdopted.否)
            group new { s, o } by new { o.Id, o.Name } into g
            select new
            {
                g.Key.Id,
                g.Key.Name,
                CompletedYes = g.Count(x => x.s.Completed == IsAdopted.是),
                CompletedNo  = g.Count(x => x.s.Completed == IsAdopted.否)
            };

        // 2. 轉成 DTO、計算完成率、排序
        var ordered = groupedQuery
            .AsEnumerable()
            .Select(x => new CompanyCompletionRankingDto
            {
                OrganizationId   = x.Id,
                OrganizationName = x.Name,
                CompletedYes     = x.CompletedYes,
                CompletedNo      = x.CompletedNo,
                CompletionRate   = (x.CompletedYes + x.CompletedNo) == 0
                    ? 0
                    : (decimal)x.CompletedYes / (x.CompletedYes + x.CompletedNo)
            })
            .OrderBy(r => r.CompletionRate)        // 完成率低 → 高
            .ThenByDescending(r => r.CompletedNo); // 同率時未完成多者優先

        // 3. 需求若有限制前 N 名
        var final = topN.HasValue
            ? ordered.Take(topN.Value)
            : ordered;

        return final.ToList();
    }
    
    public async Task<List<SuggestUncompletedDto>> GetUncompletedSuggestionsAsync(int organizationId)
    {
        return await _context.SuggestDatas
            .AsNoTracking()
            .Where(s =>
                s.OrganizationId == organizationId &&
                s.Completed == IsAdopted.否)
            .Select(s => new SuggestUncompletedDto
            {
                Id = s.Id,
                Date = s.Date,
                SuggestionContent = s.SuggestionContent,
                KpiField = s.KpiField.field,
                EventType = s.SuggestEventType.Name,
                RespDept = s.RespDept,
                Remark = s.Remark,
                IsAdopted = s.IsAdopted.ToString()
            })
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }
}