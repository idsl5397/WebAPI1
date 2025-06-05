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

public class DateOnlyJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return DateTime.ParseExact(str!, Format, null);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
public class SuggestmanyRow
{
    public int OrganizationId { get; set; }
    public string Organization { get; set; }
    [Column(TypeName = "date")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime Date { get; set; }
    public string FieldName { get; set; }
    public string UserName { get; set; }
    public string SuggestionContent { get; set; }
    public string? ImproveDetails { get; set; }
    public string SuggestionType { get; set; }
    public string SuggestEventType { get; set; }
    public string? RespDept { get; set; }
    public int IsAdopted { get; set; }
    
    public string IsAdoptedName { get; set; }
    public int? Manpower { get; set; }
    public decimal? Budget { get; set; }
    
    public int Completed { get; set; }
    public string? CompletedName { get; set; }
    public int? DoneYear { get; set; }
    public int? DoneMonth { get; set; }
    public int ParallelExec { get; set; }
    public string? ParallelExecName { get; set; }
    public string? ExecPlan { get; set; }
    public string? Remark { get; set; }
}

public class AddSuggestDto
{
    public int OrganizationId { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string Committee { get; set; } = null!;
    public string Suggestion { get; set; } = null!;
    public string SuggestionType { get; set; } = null!;
    public string SuggestEventType { get; set; }
    public string Department { get; set; } = null!;
    public IsAdopted IsAdopted { get; set; }
    public string? AdoptedOther { get; set; }
    public string ImproveDetail { get; set; } = null!;
    public int? Manpower { get; set; }
    public decimal? Budget { get; set; }
    public IsAdopted IsCompleted { get; set; }
    public string? CompletedOther { get; set; }
    public string DoneYear { get; set; } = null!;
    public string DoneMonth { get; set; } = null!;
    public IsAdopted IsParallel { get; set; }
    public string? ParallelOther { get; set; }
    public string? ExecPlan { get; set; }
    public string? Remark { get; set; }
}

public class SuggestDto
{
    public int Id { get; set; }
    [Column(TypeName = "date")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
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
    public Task<List<SuggestDto>> GetAllSuggestsAsync(int? organizationId = null, int? startYear = null, int? endYear = null);
    public Task<(bool Success, string Message)> ImportSingleSuggestAsync(AddSuggestDto dto);
    public Task<List<SuggestmanyRow>> ParseExcelAsync(Stream fileStream);

    public Task<(bool Success, string Message, int SuccessCount, int FailCount)> BatchInsertSuggestAsync(
        List<SuggestmanyRow> rows);

}

public class SuggestService:ISuggestService
{
    private readonly isha_sys_devContext _context;
    private readonly ILogger<SuggestService> _logger;
    private readonly IOrganizationService _organizationService;

    public SuggestService(isha_sys_devContext context,ILogger<SuggestService> logger,IOrganizationService organizationService)
    {
        _context = context;
        _logger = logger;
        _organizationService = organizationService;
    }
    private static DateTime? GetCellDate(ICell cell)
    {
        if (cell == null) return null;
        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
        {
            return (DateTime)cell.DateCellValue;
        }

        // 若是用文字輸入日期，也試圖轉換
        var raw = cell.ToString()?.Trim();
        if (DateTime.TryParse(raw, out var parsed))
            return parsed;

        return null;
    }
    private static string GetCellString(ICell cell)
    {
        if (cell == null) return string.Empty;

        var text = cell.ToString()?.Trim() ?? string.Empty;
        return text == "-" ? string.Empty : text;
    }
    private static decimal? GetCellDecimal(ICell cell)
    {
        if (cell == null) return null;

        if (cell.CellType == CellType.Numeric)
        {
            return (decimal)cell.NumericCellValue;
        }
        else
        {
            var text = cell.ToString()?.Trim();

            // ✅ 如果是空字串或者 "-"，就當成 null
            if (string.IsNullOrEmpty(text) || text == "-")
            {
                return null;
            }

            return decimal.TryParse(text, out var result) ? result : null;
        }
    }
    public static IsAdopted ParseIsAdopted(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || input == "-" || input.ToUpper() == "NA")
            return IsAdopted.否;

        return Enum.TryParse<IsAdopted>(input.Trim(), out var result)
            ? result
            : IsAdopted.否;
    }
    public async Task<List<SuggestDto>> GetAllSuggestsAsync(
        int? organizationId = null,
        int? startYear = null,
        int? endYear = null)
    {
        var query = _context.SuggestDatas
            .Include(s => s.SuggestEventType)
            .Include(s => s.SuggestionType)
            .Include(s => s.Organization)
            .Include(s => s.KpiField)
            .Include(s => s.User)
            .AsQueryable();

        // 篩選：組織（含下層）
        if (organizationId.HasValue)
        {
            var orgIds = _organizationService.GetDescendantOrganizationIds(organizationId.Value);
            query = query.Where(d => orgIds.Contains(d.Organization.Id));
        }

        // 篩選：年份（根據日期中的年份）
        if (startYear.HasValue)
        {
            query = query.Where(d => d.Date.Year >= startYear.Value);
        }

        if (endYear.HasValue)
        {
            query = query.Where(d => d.Date.Year <= endYear.Value);
        }

        var data = await query
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
    
    public async Task<(bool Success, string Message)> ImportSingleSuggestAsync(AddSuggestDto dto)
    {
        try
        {
            var now = DateTime.UtcNow;
            
            // 取得 KpiFieldId（支援中英文判斷）
            var fieldInput = dto.Category?.Trim();
            if (string.Equals(fieldInput, "製程安全", StringComparison.OrdinalIgnoreCase))
            {
                fieldInput = "製程安全管理";
            }
            var kpiField = await _context.KpiFields.FirstOrDefaultAsync(f =>
                f.field == fieldInput || f.enfield == fieldInput);

            if (kpiField == null)
            {
                return (false, $"找不到對應的指標領域（{fieldInput}）");
            }
            
            // 根據 nickname 找出對應的使用者
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Nickname == dto.Committee);

            if (user == null)
            {
                return (false, $"找不到對應的委員帳戶（{dto.Committee}）");
            }
    
            // 檢查建議類別是否存在
            var suggestType = await _context.SuggestionTypes
                .FirstOrDefaultAsync(x => x.Name == dto.SuggestionType);
            if (suggestType == null)
            {
                suggestType = new SuggestionType
                {
                    Name = dto.SuggestionType,
                    CreatedAt = now
                };
                _context.SuggestionTypes.Add(suggestType);
                await _context.SaveChangesAsync();
            }
    
            // 檢查會議/活動類別是否存在
            var suggestEventType = await _context.SuggestEventTypes
                .FirstOrDefaultAsync(x => x.Name == dto.SuggestEventType);
            if (suggestEventType == null)
            {
                suggestEventType = new SuggestEventType
                {
                    Name = dto.SuggestEventType,
                    CreatedAt = now
                };
                _context.SuggestEventTypes.Add(suggestEventType);
                await _context.SaveChangesAsync();
            }

    
            // 建立建議資料
            var suggest = new SuggestData
            {
                OrganizationId= dto.OrganizationId,
                Date = dto.Date,
                KpiFieldId = kpiField.Id,
                SuggestionContent = dto.Suggestion,
                SuggestionTypeId = suggestType.Id,
                SuggestEventTypeId = suggestEventType.Id,
                RespDept = dto.Department,
                IsAdopted = dto.IsAdopted,
                IsAdoptedOther = dto.AdoptedOther,
                ImproveDetails = dto.ImproveDetail,
                Manpower = dto.Manpower,
                Budget = dto.Budget,
                Completed = dto.IsCompleted,
                CompletedOther = dto.CompletedOther,
                DoneYear = int.TryParse(dto.DoneYear, out var y) ? y : null,
                DoneMonth = int.TryParse(dto.DoneMonth, out var m) ? m : null,
                ParallelExec = dto.IsParallel,
                ParallelExecOther = dto.ParallelOther,
                ExecPlan = dto.ExecPlan,
                Remark = dto.Remark,
                CreatedAt = now,
                UpdateAt = now,
                UserId = user.Id
            };
    
            _context.SuggestDatas.Add(suggest);
            await _context.SaveChangesAsync();
    
            return (true, "✅ 匯入成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入建議資料失敗");
            return (false, "❌ 匯入失敗：" + ex.Message);
        }
    }
    
    public async Task<List<SuggestmanyRow>> ParseExcelAsync(Stream fileStream)
    {
        var workbook = new XSSFWorkbook(fileStream);
        var sheet = workbook.GetSheetAt(0); // 第一個工作表
        var result = new List<SuggestmanyRow>();

        // 從第2行（index = 1）開始讀，因為第1行是標題
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null) continue;

            var dto = new SuggestmanyRow
            {
                OrganizationId = int.TryParse(GetCellString(row.GetCell(0)), out var orgId) ? orgId : 0,
                Organization = GetCellString(row.GetCell(1)),
                Date = GetCellDate(row.GetCell(2)) ?? DateTime.Now,
                SuggestEventType = GetCellString(row.GetCell(3)),
                FieldName = GetCellString(row.GetCell(4)),
                UserName = GetCellString(row.GetCell(5)),
                SuggestionContent = GetCellString(row.GetCell(6)),
                SuggestionType = GetCellString(row.GetCell(7)),
                RespDept = GetCellString(row.GetCell(8)),
                IsAdoptedName = GetCellString(row.GetCell(9)),
                ImproveDetails = GetCellString(row.GetCell(10)),
                Manpower = (int?)GetCellDecimal(row.GetCell(11)),
                Budget = GetCellDecimal(row.GetCell(12)),
                CompletedName = GetCellString(row.GetCell(13)),
                DoneYear = (int?)GetCellDecimal(row.GetCell(14)),
                DoneMonth = (int?)GetCellDecimal(row.GetCell(15)),
                ParallelExecName = GetCellString(row.GetCell(16)),
                ExecPlan = GetCellString(row.GetCell(17)),
                Remark = GetCellString(row.GetCell(18)),
            };

            result.Add(dto);
        }

        return result;
    }
    
    public async Task<(bool Success, string Message, int SuccessCount, int FailCount)> BatchInsertSuggestAsync(List<SuggestmanyRow> rows)
    {
        if (rows == null || rows.Count == 0)
            return (false, "無資料可匯入", 0, 0);

        using var transaction = await _context.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        int successCount = 0, failCount = 0;
        // 預先查出所有 SuggestionContent 以加速比對（避免逐筆查 DB）
        var existingContents = await _context.SuggestDatas
            .Select(x => x.SuggestionContent)
            .ToListAsync();
        var existingSet = new HashSet<string>(existingContents);
        var errorLogs = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var fieldName = row.FieldName?.Trim();
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    failCount++;
                    errorLogs.Add($"[第{i + 1}筆] 欄位領域為空，跳過");
                    continue;
                }

                string? matchedField = fieldName switch
                {
                    var f when f.StartsWith("製程安全") => "製程安全管理",
                    var f when f.StartsWith("消防") => "消防管理",
                    var f when f.StartsWith("環保") => "環保管理",
                    var f when f.StartsWith("能源") => "能源管理",
                    _ => fieldName
                };

                var kpiField = await _context.KpiFields
                    .FirstOrDefaultAsync(f => f.field == matchedField || f.enfield == matchedField);

                if (kpiField == null)
                {
                    failCount++;
                    errorLogs.Add($"[第{i + 1}筆] 找不到 KpiField：{matchedField}");
                    continue;
                }

                var rawNickname = row.UserName?.Trim();
                if (string.IsNullOrWhiteSpace(rawNickname))
                {
                    failCount++;
                    errorLogs.Add($"[第{i + 1}筆] 委員姓名為空，跳過");
                    continue;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Nickname == rawNickname);
                if (user == null)
                {
                    string username;
                    do
                    {
                        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
                        username = $"member_{suffix}";
                    }
                    while (await _context.Users.AnyAsync(u => u.Username == username));

                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = username,
                        Nickname = rawNickname.Length > 50 ? rawNickname.Substring(0, 50) : rawNickname,
                        Email = username + "@autogen.local",
                        OrganizationId = 1218,
                        EmailVerified = false,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    try
                    {
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLogs.Add($"新增使用者失敗（{rawNickname}）：{ex.Message}");
                        continue;
                    }
                }
                
                var suggestionContent = row.SuggestionContent?.Trim();
                if (string.IsNullOrWhiteSpace(suggestionContent))
                {
                    failCount++;
                    errorLogs.Add($"[第{i + 1}筆] 建議內容為空");
                    continue;
                }

                if (existingSet.Contains(suggestionContent))
                {
                    Console.WriteLine($"⚠️ 重複資料已存在，跳過內容：{suggestionContent.Substring(0, Math.Min(20, suggestionContent.Length))}...");
                    failCount++;
                    errorLogs.Add($"[第{i + 1}筆] 已有資料（重複內容）：{suggestionContent.Substring(0, Math.Min(20, suggestionContent.Length))}...");
                    continue;
                }
                
                var suggestionTypeName = row.SuggestionType?.Trim();
                SuggestionType suggestType = null;
                if (!string.IsNullOrWhiteSpace(suggestionTypeName))
                {
                    suggestType = await _context.SuggestionTypes.FirstOrDefaultAsync(x => x.Name == suggestionTypeName);
                    if (suggestType == null)
                    {
                        suggestType = new SuggestionType
                        {
                            Name = suggestionTypeName,
                            CreatedAt = now
                        };
                        _context.SuggestionTypes.Add(suggestType);
                        await _context.SaveChangesAsync();
                    }
                }

                var eventTypeName = row.SuggestEventType?.Trim();
                SuggestEventType suggestEventType = null;
                if (!string.IsNullOrWhiteSpace(eventTypeName))
                {
                    suggestEventType = await _context.SuggestEventTypes.FirstOrDefaultAsync(x => x.Name == eventTypeName);
                    if (suggestEventType == null)
                    {
                        suggestEventType = new SuggestEventType
                        {
                            Name = eventTypeName,
                            CreatedAt = now
                        };
                        _context.SuggestEventTypes.Add(suggestEventType);
                        await _context.SaveChangesAsync();
                    }
                }

                var newSuggest = new SuggestData
                {
                    OrganizationId = row.OrganizationId,
                    Date = row.Date,
                    KpiFieldId = kpiField.Id,
                    UserId = user.Id,
                    SuggestionContent = row.SuggestionContent,
                    SuggestionTypeId = suggestType.Id,
                    SuggestEventTypeId = suggestEventType.Id,
                    RespDept = row.RespDept,
                    IsAdopted = ParseIsAdopted(row.IsAdoptedName),
                    ImproveDetails = row.ImproveDetails,
                    Manpower = row.Manpower,
                    Budget = row.Budget,
                    Completed = ParseIsAdopted(row.CompletedName),
                    DoneYear = row.DoneYear,
                    DoneMonth = row.DoneMonth,
                    ParallelExec = ParseIsAdopted(row.ParallelExecName),
                    ExecPlan = row.ExecPlan,
                    Remark = row.Remark,
                    CreatedAt = now,
                    UpdateAt = now
                };

                _context.SuggestDatas.Add(newSuggest);
                successCount++;
            }
            catch (Exception ex)
            {
                failCount++;
                errorLogs.Add($"[第{i + 1}筆] 匯入失敗：{ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        if (errorLogs.Count > 0)
        {
            var logFileName = $"Suggest失敗匯入紀錄_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var fullPath = Path.Combine("Logs", logFileName);
            Directory.CreateDirectory("Logs");
            await File.WriteAllLinesAsync(fullPath, errorLogs);
        }

        var summary = $"✅ 匯入完成，成功：{successCount} 筆，失敗：{failCount} 筆";
        if (errorLogs.Count > 0)
        {
            summary += $"\n詳細錯誤如下：\n{string.Join("\n", errorLogs.Take(5))}...";
            summary += "\n👉 其餘詳見 log 檔案";
        }

        return (true, summary, successCount, failCount);
    }

}