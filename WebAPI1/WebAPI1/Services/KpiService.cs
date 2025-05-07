using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Services;

public class KpiReportInsertDto
{
    public int KpiDataId { get; set; }
    public int Year { get; set; }
    public string Quarter { get; set; } // Q1, Q2, Q3, Q4, Y
    public decimal? Value { get; set; }
    public bool IsSkipped { get; set; }
    public string Remark { get; set; }
}
public class CreateKpiFieldDto
{
    public string Field { get; set; }
}

public class KpiDataDto
{
    public int KpiDataId { get; set; }
    public string Company { get; set; }
    public string Field { get; set; }
    public string EnField { get; set; }
    public string ProductionSite { get; set; }
    public string KpiCategoryName { get; set; }
    public string IndicatorName { get; set; }
    public string DetailItemName { get; set; }
    public string Unit { get; set; }
    public string BaselineYear { get; set; }
    public decimal? BaselineValue { get; set; }
    public decimal? TargetValue { get; set; }
}

public class KpisingleRow
{
    public int OrganizationId { get; set; }
    public string ProductionSite { get; set; } // ✅ 這個名字要對
    public string KpiCategoryName { get; set; }
    public string FieldName { get; set; }
    public string IndicatorName { get; set; }
    public string DetailItemName { get; set; }
    public string Unit { get; set; }
    public bool IsApplied { get; set; }
    public string BaselineYear { get; set; }
    public decimal BaselineValue { get; set; }
    public decimal TargetValue { get; set; }
    public string Remarks { get; set; }
}

public class KpimanyRow
{
    public int OrganizationId { get; set; }
    public string Organization { get; set; }
    public string ProductionSite { get; set; } // ✅ 這個名字要對
    public string KpiCategoryName { get; set; }
    public string FieldName { get; set; }
    public string IndicatorName { get; set; }
    public string DetailItemName { get; set; }
    public string Unit { get; set; }
    public bool IsApplied { get; set; }
    public string BaselineYear { get; set; }
    public decimal? BaselineValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string Remarks { get; set; }
}

public class KpiReportDto
{
    public int Year { get; set; }
    public string Period { get; set; }
    public decimal? KpiReportValue { get; set; }
}
public class KpiDisplayDto
{
    public int Id { get; set; }
    public string Company { get; set; }
    public string? ProductionSite { get; set; }
    public string Category { get; set; }
    public string Field { get; set; }
    public int IndicatorNumber { get; set; }
    public string IndicatorName { get; set; }
    public string DetailItemName { get; set; }
    public string Unit { get; set; }
    public bool IsApplied { get; set; }
    public string BaselineYear { get; set; }
    public decimal? BaselineValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Remarks { get; set; }
    public List<KpiReportDto> Reports { get; set; } = new();
}

public interface IKpiService
{
    Task<KpiField> CreateKpiFieldAsync(CreateKpiFieldDto dto);
   
    Task<(bool Success, string Message)> InsertKpiData(KpisingleRow row);
    Task<List<KpiDisplayDto>> GetKpiDisplayAsync(int? organizationId = null, int? startYear = null, int? endYear = null, string? categoryName = null, string? fieldName = null);
    Task<List<KpiDataDto>> GetKpiDataDtoByOrganizationIdAsync(int organizationId);
    Task<(bool Success, string Message)> SubmitKpiReportsAsync(List<KpiReportInsertDto> reports);
    Task<List<KpimanyRow>> ParseExcelAsync(Stream fileStream);
    Task<(bool Success, string Message)> BatchInsertKpiDataAsync(List<KpimanyRow> rows);
}

public class KpiService:IKpiService
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<KpiService> _logger;
    private readonly IOrganizationService _organizationService;
    
    public KpiService(
        isha_sys_devContext db,
        ILogger<KpiService> logger,
        IOrganizationService organizationService)
    {
        _db = db;
        _logger = logger;
        _organizationService = organizationService;
    }
    
    public async Task<KpiField> CreateKpiFieldAsync(CreateKpiFieldDto dto)
    {
        try
        {
            // 避免重複欄位名稱
            var exists = await _db.KpiFields
                .AnyAsync(k => k.field == dto.Field);

            if (exists)
            {
                _logger.LogWarning("欄位已存在: {Field}", dto.Field);
                throw new Exception("欄位名稱重複，無法新增");
            }

            var newField = new KpiField
            {
                field = dto.Field,
                CreatedAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            _db.KpiFields.Add(newField);
            await _db.SaveChangesAsync();

            _logger.LogInformation("成功新增欄位: {Field}", dto.Field);

            return newField;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增 KPI 欄位失敗");
            throw;
        }
    }

    //匯入績效指標資料
    public async Task<(bool Success, string Message)> InsertKpiData(KpisingleRow row)
    {
        var now = DateTime.UtcNow;
        var userEmail = "idsl5397@mail.isha.org.tw";
        var currentYear = 113;

        // 1. 公司
        var organization = await _db.Organizations.FindAsync(row.OrganizationId);

        
        // 2. 領域
        var field = await _db.KpiFields.FirstOrDefaultAsync(f => f.enfield == row.FieldName);

        // 3. 指標類型
        int categoryId = row.KpiCategoryName == "客製型" ? 1 : 0;

        // 4. KpiItem（含名稱判斷）
        var item = await _db.KpiItems
            .Include(i => i.KpiItemNames)
            .FirstOrDefaultAsync(i =>
                i.KpiFieldId == field.Id &&
                i.KpiCategoryId == categoryId &&
                i.OrganizationId == (categoryId == 1 ? organization.Id : null) &&
                i.KpiItemNames.Any(n => n.Name == row.IndicatorName && n.StartYear == currentYear));

        if (item == null)
        {
            // 動態產生下一個編號
            var maxNumber = await _db.KpiItems
                .Where(i => i.KpiFieldId == field.Id &&
                            i.KpiCategoryId == categoryId &&
                            i.OrganizationId == (categoryId == 1 ? organization.Id : null))
                .MaxAsync(i => (int?)i.IndicatorNumber) ?? 0;

            item = new KpiItem
            {
                IndicatorNumber = maxNumber + 1,
                KpiFieldId = field.Id,
                KpiCategoryId = categoryId,
                OrganizationId = (categoryId == 1 ? organization.Id : null),
                CreateTime = now,
                UploadTime = now
            };

            _db.KpiItems.Add(item);
            await _db.SaveChangesAsync();

            // 直接新增對應名稱
            _db.KpiItemNames.Add(new KpiItemName
            {
                KpiItemId = item.Id,
                Name = row.IndicatorName,
                StartYear = currentYear,
                UserEmail = userEmail,
                CreatedAt = now
            });
            await _db.SaveChangesAsync();
        }

        // 6. DetailItem
        var detailItem = await _db.KpiDetailItems
            .Include(d => d.KpiDetailItemNames)
            .FirstOrDefaultAsync(d =>
                d.KpiItemId == item.Id &&
                d.KpiDetailItemNames.Any(n => n.Name == row.DetailItemName && n.StartYear == currentYear));

        if (detailItem == null)
        {
            // 新增一筆 detailItem
            detailItem = new KpiDetailItem
            {
                KpiItemId = item.Id,
                Unit = row.Unit,
                CreateTime = now,
                UploadTime = now
            };

            var detailItemName = new KpiDetailItemName
            {
                Name = row.DetailItemName,
                StartYear = currentYear,
                CreatedAt = now,
                UserEmail = userEmail
            };

            detailItem.KpiDetailItemNames.Add(detailItemName);

            _db.KpiDetailItems.Add(detailItem);
            await _db.SaveChangesAsync();
        }

        // 7. KpiDetailItemName
        bool hasDetailName = await _db.KpiDetailItemNames.AnyAsync(n =>
                n.KpiDetailItemId == detailItem.Id &&
                n.StartYear == currentYear &&
                n.Name == row.DetailItemName // ✅ 加上名稱比對
        );

        if (!hasDetailName)
        {
            _db.KpiDetailItemNames.Add(new KpiDetailItemName
            {
                KpiDetailItemId = detailItem.Id,
                Name = row.DetailItemName,
                StartYear = currentYear,
                UserEmail = userEmail,
                CreatedAt = now
            });
            await _db.SaveChangesAsync();
        }

        // 8. KpiData
        var existsData = await _db.KpiDatas.AnyAsync(d =>
            d.OrganizationId == organization.Id &&
            d.DetailItemId == detailItem.Id &&
            d.ProductionSite == row.ProductionSite);

        if (existsData)
        {
            return (false, "已存在相同指標資料");
        }
        else
        {
            var kpiData = new KpiData
            {
                OrganizationId = organization.Id,
                ProductionSite = row.ProductionSite,
                DetailItemId = detailItem.Id,
                IsApplied = row.IsApplied,
                BaselineYear = row.BaselineYear,
                BaselineValue = row.BaselineValue,
                TargetValue = row.TargetValue,
                Remarks = row.Remarks,
                CreatedAt = now,
                UpdateAt = now
            };
            _db.KpiDatas.Add(kpiData);
            await _db.SaveChangesAsync();
        }
        
        return (true, $"✅ 匯入成功：{row.OrganizationId} / {row.IndicatorName} / {row.DetailItemName}");
    }
    
    
    public async Task<List<KpiDisplayDto>> GetKpiDisplayAsync(int? organizationId = null , int? startYear = null, int? endYear = null, string? categoryName = null, string? fieldName = null)
    {
        var query = _db.KpiDatas
            .Include(d => d.DetailItem)
                .ThenInclude(di => di.KpiItem)
                    .ThenInclude(i => i.KpiItemNames)
            .Include(d => d.DetailItem.KpiDetailItemNames)
            .Include(d => d.KpiReports)
            .Include(d => d.Organization)
            .Include(d => d.DetailItem.KpiItem.KpiField)
            .AsQueryable();
    
        if (organizationId.HasValue)
        {
            var orgIds = _organizationService.GetDescendantOrganizationIds(organizationId.Value);
            query = query.Where(d => orgIds.Contains(d.Organization.Id));
        }
    
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            int categoryId = categoryName == "客製型" ? 1 : 0;
            query = query.Where(d => d.DetailItem.KpiItem.KpiCategoryId == categoryId);
        }
    
        if (!string.IsNullOrWhiteSpace(fieldName))
            query = query.Where(d => d.DetailItem.KpiItem.KpiField.field == fieldName);
    
        // ✅ 關鍵：報表內需至少有一筆符合條件
        if (startYear.HasValue || endYear.HasValue)
        {
            query = query.Where(d => d.KpiReports.Any(r =>
                (!startYear.HasValue || r.Year >= startYear) &&
                (!endYear.HasValue || r.Year <= endYear)
            ));
        }
        
        var result = await query.Select(d => new KpiDisplayDto
        {
            Id = d.Id,
            Company = d.Organization.Name,
            ProductionSite = d.ProductionSite,
            Category = d.DetailItem.KpiItem.KpiCategoryId == 1 ? "客製型" : "基礎型",
            Field = d.DetailItem.KpiItem.KpiField.field,
            IndicatorNumber = d.DetailItem.KpiItem.IndicatorNumber,
            IndicatorName = d.DetailItem.KpiItem.KpiItemNames
                .OrderByDescending(n => n.StartYear).FirstOrDefault().Name,
            DetailItemName = d.DetailItem.KpiDetailItemNames
                .OrderByDescending(n => n.StartYear).FirstOrDefault().Name,
            Unit = d.DetailItem.Unit,
            IsApplied = d.IsApplied,
            BaselineYear = d.BaselineYear,
            BaselineValue = d.BaselineValue,
            TargetValue = d.TargetValue,
            Remarks = d.Remarks,
            Reports = d.KpiReports
                .Where(r => !startYear.HasValue || r.Year >= startYear)
                .Where(r => !endYear.HasValue || r.Year <= endYear)
                .OrderBy(r => r.Year).ThenBy(r => r.Period)
                .Select(r => new KpiReportDto
                {
                    Year = r.Year,
                    Period = r.Period,
                    KpiReportValue = r.KpiReportValue
                }).ToList()
        }).ToListAsync();
    
        return result;
    }
    
    public async Task<List<KpiDataDto>> GetKpiDataDtoByOrganizationIdAsync(int organizationId)
    {
        var result = await _db.KpiDatas
            .Include(d => d.DetailItem)
            .ThenInclude(di => di.KpiItem)
            .ThenInclude(ki => ki.KpiItemNames)
            .Include(d => d.DetailItem.KpiDetailItemNames)
            .Include(d => d.Organization)
            .Include(d => d.DetailItem.KpiItem.KpiField)
            .Include(d => d.KpiReports)
            .Where(d => d.Organization.Id == organizationId)
            .Select(d => new KpiDataDto
            {
                KpiDataId = d.Id,
                Company = d.Organization.Name,
                ProductionSite = d.ProductionSite,
                Field = d.DetailItem.KpiItem.KpiField.field,
                EnField = d.DetailItem.KpiItem.KpiField.enfield,
                KpiCategoryName = d.DetailItem.KpiItem.KpiCategoryId == 1 ? "客製型" : "基礎型",
                IndicatorName = d.DetailItem.KpiItem.KpiItemNames
                    .OrderByDescending(n => n.StartYear)
                    .FirstOrDefault().Name,
                DetailItemName = d.DetailItem.KpiDetailItemNames
                    .OrderByDescending(n => n.StartYear)
                    .FirstOrDefault().Name,
                Unit = d.DetailItem.Unit,
                BaselineYear = d.BaselineYear,
                BaselineValue = d.BaselineValue,
                TargetValue = d.TargetValue
            })
            .ToListAsync();

        return result;
    }
    
    public async Task<(bool Success, string Message)> SubmitKpiReportsAsync(List<KpiReportInsertDto> reports)
    {
        if (reports == null || !reports.Any())
            return (false, "報告資料不可為空");

        var now = DateTime.UtcNow;

        var entities = reports.Select(r => new KpiReport
        {
            KpiDataId = r.KpiDataId,
            Year = r.Year,
            Period = r.Quarter,
            KpiReportValue = r.IsSkipped ? null : r.Value,
            IsSkipped = r.IsSkipped,
            Remarks = r.Remark,
            CreatedAt = now,
            UpdateAt = now
        }).ToList();

        try
        {
            await _db.KpiReports.AddRangeAsync(entities);
            await _db.SaveChangesAsync();

            return (true, "KPI 報告已成功提交");
        }
        catch (DbUpdateException dbEx)
        {
            // ⛔ 簡單檢查重複鍵的訊息
            if (dbEx.InnerException?.Message.Contains("duplicate") == true || dbEx.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return (false, "已有相同年度與季度的報告資料，請勿重複提交");
            }

            return (false, "資料庫更新失敗：" + dbEx.InnerException?.Message);
        }
        catch (Exception ex)
        {
            return (false, $"提交失敗：{ex.Message}");
        }
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
    
    public async Task<List<KpimanyRow>> ParseExcelAsync(Stream fileStream)
    {
        var workbook = new XSSFWorkbook(fileStream);
        var sheet = workbook.GetSheetAt(0); // 第一個工作表
        var result = new List<KpimanyRow>();

        // 從第2行（index = 1）開始讀，因為第1行是標題
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null) continue;

            var dto = new KpimanyRow
            {
                OrganizationId = int.TryParse(GetCellString(row.GetCell(0)), out var orgId) ? orgId : 0,
                Organization = GetCellString(row.GetCell(1)),
                ProductionSite = GetCellString(row.GetCell(2)),
                KpiCategoryName = GetCellString(row.GetCell(3)),
                FieldName = GetCellString(row.GetCell(4)),
                IndicatorName = GetCellString(row.GetCell(6)),
                DetailItemName = GetCellString(row.GetCell(7)),
                Unit = GetCellString(row.GetCell(8)),
                IsApplied = GetCellString(row.GetCell(9)) == "是",
                BaselineYear = GetCellString(row.GetCell(10)),
                BaselineValue = GetCellDecimal(row.GetCell(11)),
                TargetValue = GetCellDecimal(row.GetCell(12)),
                Remarks = GetCellString(row.GetCell(13))
            };

            result.Add(dto);
        }

        return result;
    }
    // 確認後正式存入資料庫
    public async Task<(bool Success, string Message)> BatchInsertKpiDataAsync(List<KpimanyRow> rows)
    {
        if (rows == null || rows.Count == 0)
            return (false, "沒有任何資料可匯入。");

        using var transaction = await _db.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        var userEmail = "idsl5397@mail.isha.org.tw";
        var currentYear = 113;

        int successCount = 0;
        int failCount = 0;
        List<string> failDetails = new();

        // 一次查好基礎資料，放到記憶體Dictionary加速
        var allOrganizations = await _db.Organizations.ToDictionaryAsync(o => o.Id);
        var allFields = await _db.KpiFields.ToDictionaryAsync(f => f.enfield);
        var allKpiItems = await _db.KpiItems
            .Include(i => i.KpiItemNames)
            .ToListAsync();

        var allDetailItems = await _db.KpiDetailItems
            .Include(d => d.KpiDetailItemNames)
            .ToListAsync();

        try
        {
            foreach (var row in rows)
            {
                try
                {
                    // 1. 公司
                    if (!allOrganizations.TryGetValue(row.OrganizationId, out var organization))
                        throw new Exception($"找不到公司：{row.OrganizationId}");

                    // 2. 領域
                    if (!allFields.TryGetValue(row.FieldName, out var field))
                        throw new Exception($"找不到領域：{row.FieldName}");

                    // 3. 判斷指標類型
                    int categoryId = (row.KpiCategoryName == "客製" || row.KpiCategoryName == "客製型") ? 1 : 0;

                    // 4. 找指標（KpiItem）
                    var item = allKpiItems.FirstOrDefault(i =>
                        i.KpiFieldId == field.Id &&
                        i.KpiCategoryId == categoryId &&
                        i.OrganizationId == (categoryId == 1 ? organization.Id : null) &&
                        i.KpiItemNames.Any(n => n.Name == row.IndicatorName && n.StartYear == currentYear));

                    if (item == null)
                    {
                        // 建新Item
                        var maxNumber = allKpiItems
                            .Where(i => i.KpiFieldId == field.Id &&
                                        i.KpiCategoryId == categoryId &&
                                        i.OrganizationId == (categoryId == 1 ? organization.Id : null))
                            .Max(i => (int?)i.IndicatorNumber) ?? 0;

                        item = new KpiItem
                        {
                            IndicatorNumber = maxNumber + 1,
                            KpiFieldId = field.Id,
                            KpiCategoryId = categoryId,
                            OrganizationId = (categoryId == 1 ? organization.Id : null),
                            CreateTime = now,
                            UploadTime = now,
                            KpiItemNames = new List<KpiItemName>
                            {
                                new KpiItemName
                                {
                                    Name = row.IndicatorName,
                                    StartYear = currentYear,
                                    UserEmail = userEmail,
                                    CreatedAt = now
                                }
                            }
                        };
                        _db.KpiItems.Add(item);
                        await _db.SaveChangesAsync();
                        allKpiItems.Add(item); // 更新記憶體快取
                    }

                    // 5. 找指標細項（KpiDetailItem）
                    var detailItem = allDetailItems.FirstOrDefault(d =>
                        d.KpiItemId == item.Id &&
                        d.Unit == row.Unit &&
                        d.KpiDetailItemNames.Any(n => n.Name == row.DetailItemName && n.StartYear == currentYear));

                    if (detailItem == null)
                    {
                        detailItem = new KpiDetailItem
                        {
                            KpiItemId = item.Id,
                            Unit = row.Unit,
                            CreateTime = now,
                            UploadTime = now,
                            KpiDetailItemNames = new List<KpiDetailItemName>
                            {
                                new KpiDetailItemName
                                {
                                    Name = row.DetailItemName,
                                    StartYear = currentYear,
                                    UserEmail = userEmail,
                                    CreatedAt = now
                                }
                            }
                        };
                        _db.KpiDetailItems.Add(detailItem);
                        await _db.SaveChangesAsync(); // ✅ 這裡立刻存，拿到 detailItem.Id
                        allDetailItems.Add(detailItem);
                    }

                    // 6. 確認是否已經存在相同的 KPI Data
                    bool existsData = await _db.KpiDatas.AnyAsync(d =>
                        d.OrganizationId == organization.Id &&
                        d.DetailItemId == detailItem.Id &&
                        d.ProductionSite == row.ProductionSite);

                    if (existsData)
                    {
                        failCount++;
                        failDetails.Add($"已有資料：{row.OrganizationId} / {row.IndicatorName} / {row.DetailItemName}");
                        continue;
                    }

                    // 7. 新增KpiData
                    _db.KpiDatas.Add(new KpiData
                    {
                        OrganizationId = organization.Id,
                        ProductionSite = row.ProductionSite,
                        DetailItemId = detailItem.Id,
                        IsApplied = row.IsApplied,
                        BaselineYear = row.BaselineYear,
                        BaselineValue = row.BaselineValue,
                        TargetValue = row.TargetValue,
                        Remarks = row.Remarks,
                        CreatedAt = now,
                        UpdateAt = now
                    });

                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    failDetails.Add(ex.Message);
                }

                // ⚡ 每處理50筆，先存一次
                if ((successCount + failCount) % 50 == 0)
                {
                    await _db.SaveChangesAsync();
                }
            }

            // 最後再Save一次
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"✅ 匯入完成，成功：{successCount} 筆，失敗：{failCount} 筆\n{string.Join("\n", failDetails)}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return (false, $"❌ 匯入失敗，原因：{innerMessage}");
        }
    }
    
    
}