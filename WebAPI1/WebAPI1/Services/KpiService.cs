using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;
using WebAPI1.Models;

namespace WebAPI1.Services;

public class CreateKpiFieldDto
{
    public string Field { get; set; }
}

public class KpiExcelRow
{
    public int CompanyId { get; set; }
    public string FactoryName { get; set; } = "";
    
    public string? ProductionSite { get; set; } = null;
    public string KpiCategoryName { get; set; } = ""; // 基礎/客製
    public string FieldName { get; set; } = "";
    public int IndicatorNumber { get; set; }
    public string IndicatorName { get; set; } = "";
    public string DetailItemName { get; set; } = "";
    public string Unit { get; set; } = "";
    public bool IsApplied { get; set; }
    public string BaselineYear { get; set; } = "";
    public int BaselineValue { get; set; }
    public int TargetValue { get; set; }
    public string? Remarks { get; set; } = "";
    public Dictionary<int, int> ExecutionByYear { get; set; } = new();
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

public class KpiReportDto
{
    public int Year { get; set; }
    public string Period { get; set; }
    public decimal? KpiReportValue { get; set; }
}
public class KpiDisplayDto
{
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
    Task<string> InsertKpiRecordAsync(KpiExcelRow row);
    Task<string> InsertKpiData(KpisingleRow row);
    Task<List<KpiDisplayDto>> GetKpiDisplayAsync(int? organizationId = null, int? startYear = null, int? endYear = null, string? categoryName = null, string? fieldName = null);
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
    
    //匯入績效指標資料+每年的執行情況
    public async Task<string> InsertKpiRecordAsync(KpiExcelRow row)
    {
        var now = DateTime.UtcNow;
        var userEmail = "idsl5397@mail.isha.org.tw";
        var currentYear = 113;

        // 1. 公司
        var company = await _db.Organizations.FindAsync(row.CompanyId);

        // 2. 工廠（若 FactoryName 為 "-" 或空字串，就沿用公司）
        Organization factory;

        if (string.IsNullOrWhiteSpace(row.FactoryName) || row.FactoryName.Trim() == "-")
        {
            factory = company; // 工廠同公司
        }
        else
        {
            factory = await _db.Organizations.FirstOrDefaultAsync(o =>
                o.Name == row.FactoryName.Trim() && o.ParentId == company.Id);
        }

        // 2. 領域
        var field = await _db.KpiFields.FirstOrDefaultAsync(f => f.enfield == row.FieldName);
        // if (field == null)
        // {
        //     field = new KpiField { enfield = row.FieldName, CreatedAt = now };
        //     _db.KpiFields.Add(field);
        //     await _db.SaveChangesAsync();
        // }

        // 3. 指標類型
        int categoryId = row.KpiCategoryName == "客製型" ? 1 : 0;

        // 4. KpiItem
        var item = await _db.KpiItems.FirstOrDefaultAsync(i => i.IndicatorNumber == row.IndicatorNumber &&
                                                               i.KpiFieldId == field.Id &&
                                                               i.KpiCategoryId == categoryId &&
                                                               i.OrganizationId == (categoryId == 1 ? company.Id : null));
        if (item == null)
        {
            item = new KpiItem
            {
                IndicatorNumber = row.IndicatorNumber,
                KpiFieldId = field.Id,
                KpiCategoryId = categoryId,
                OrganizationId = (categoryId == 1 ? company.Id : null),
                CreateTime = now,
                UploadTime = now
            };
            _db.KpiItems.Add(item);
            await _db.SaveChangesAsync();
        }

        // 5. KpiItemName
        bool hasItemName = await _db.KpiItemNames.AnyAsync(n => n.KpiItemId == item.Id && n.StartYear == currentYear);
        if (!hasItemName)
        {
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
                d.Unit == row.Unit &&
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
        var kpiData = new KpiData
        {
            OrganizationId = factory.Id,
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

        // 9. KpiReport for 111-113
        foreach (var (year, value) in row.ExecutionByYear)
        {
            _db.KpiReports.Add(new KpiReport
            {
                Year = year,
                Period = "Y", // 全年
                KpiReportValue = value,
                KpiDataId = kpiData.Id,
                CreatedAt = now
            });
        }
        await _db.SaveChangesAsync();

        return $"✅ 匯入成功：{row.CompanyId} / {row.IndicatorName} / {row.DetailItemName}";
    }

    //匯入績效指標資料
    public async Task<string> InsertKpiData(KpisingleRow row)
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
                d.Unit == row.Unit &&
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
            d.ProductionSite == row.ProductionSite &&
            d.BaselineYear == row.BaselineYear);

        if (existsData)
        {
            return $"⚠️ 已存在相同指標資料，未重複匯入：{row.OrganizationId} / {row.IndicatorName} / {row.DetailItemName}";
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
        
        return $"匯入成功：{row.OrganizationId} / {row.IndicatorName} / {row.DetailItemName}";
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
}