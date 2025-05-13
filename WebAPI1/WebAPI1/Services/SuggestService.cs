using Microsoft.EntityFrameworkCore;
using WebAPI1.Context;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Services;

public class SuggestDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string SuggestionContent { get; set; }
    public string SuggestEventTypeName { get; set; }
    public string SuggestionTypeName { get; set; }

    public string IsAdopted { get; set; }
    public string? RespDept { get; set; }
    public string? ImproveDetails { get; set; }
    public int? Manpower { get; set; }
    public decimal? Budget { get; set; }

    public string Completed { get; set; }
    public int? DoneYear { get; set; }
    public int? DoneMonth { get; set; }

    public string ParallelExec { get; set; }
    public string? ExecPlan { get; set; }
    public string? Remark { get; set; }
    public string? OrganizationName { get; set; }
    public string? KpiFieldName { get; set; }
    public string? UserName { get; set; }
}

public interface ISuggestService
{
    public Task<List<SuggestDto>> GetAllSuggestsAsync();
}

public class SuggestService:ISuggestService
{
    private readonly isha_sys_devContext _context;
    private readonly ILogger<SuggestService> _logger;

    public SuggestService(isha_sys_devContext context,ILogger<SuggestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SuggestDto>> GetAllSuggestsAsync()
    {
        var data = await _context.SuggestDatas
            .Include(s => s.SuggestEventType)
            .Include(s => s.SuggestionType)
            .Include(s => s.Organization)
            .Include(s => s.KpiField)
            .Include(s => s.User)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return data.Select(s => new SuggestDto
        {
            Id = s.Id,
            Date = s.Date,
            SuggestionContent = s.SuggestionContent,
            SuggestEventTypeName = s.SuggestEventType?.Name,
            SuggestionTypeName = s.SuggestionType?.Name,
            IsAdopted = s.IsAdopted.ToString(),
            RespDept = s.RespDept,
            ImproveDetails = s.ImproveDetails,
            Manpower = s.Manpower,
            Budget = s.Budget,
            Completed = s.Completed.ToString(),
            DoneYear = s.DoneYear,
            DoneMonth = s.DoneMonth,
            ParallelExec = s.ParallelExec.ToString(),
            ExecPlan = s.ExecPlan,
            Remark = s.Remark,
            OrganizationName = s.Organization?.Name,
            KpiFieldName = s.KpiField?.field,
            UserName = s.User?.Nickname,
        }).ToList();
    }
}