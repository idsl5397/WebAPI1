using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WebAPI1.Context;
using WebAPI1.Entities;

namespace WebAPI1.Services;

public interface IImprovementService
{
    Task<List<string>> GetUploadedFilesAsync(int orgId);
    Task<bool> SubmitReportAsync(int orgId, int year, int quarter, IFormFile file, Guid userId);
    Task<bool> DeleteFileAsync(string fileName);
}

public class ImprovementService:IImprovementService
{
    private readonly ISHAuditDbcontext _db;
    private readonly ILogger<ImprovementService> _logger;
    private readonly IWebHostEnvironment _env;
    
    public ImprovementService(
        ISHAuditDbcontext db,
        ILogger<ImprovementService> logger,
        IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _env = env;
    }
    public async Task<List<string>> GetUploadedFilesAsync(int orgId)
    {
        return await _db.SuggestFiles
            .Where(f => f.OrganizationId == orgId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.ReportName)
            .ToListAsync();
    }

    public async Task<bool> SubmitReportAsync(int orgId, int year, int quarter, IFormFile file, Guid userId)
    {
        var org = await _db.Organizations.FindAsync(orgId);
        if (org == null) throw new Exception("無效的組織 ID");

        string reportType = "績效指標";
        string fileName = $"績效指標推動執行總成果及後續規劃報告-{org.Name}-{year}年-Q{quarter}.pdf";

        bool exists = await _db.SuggestFiles.AnyAsync(f =>
            f.OrganizationId == orgId &&
            f.Year == year &&
            f.Quarter == quarter &&
            f.ReportType == reportType
        );

        if (exists) throw new Exception("此季度的報告已上傳，請勿重複上傳");

        // 儲存檔案到 wwwroot/uploads/
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

        string filePath = Path.Combine(uploadPath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var newFile = new SuggestFile
        {
            Year = year,
            Quarter = quarter,
            ReportName = fileName,
            ReportType = reportType,
            CreatedAt = tool.GetTaiwanNow(),
            OrganizationId = orgId,
            UserId = userId
        };

        _db.SuggestFiles.Add(newFile);
        await _db.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var fileRecord = await _db.SuggestFiles.FirstOrDefaultAsync(f => f.ReportName == fileName);
        if (fileRecord == null) return false;

        try
        {
            // Step 1: 刪除資料庫紀錄
            _db.SuggestFiles.Remove(fileRecord);
            await _db.SaveChangesAsync();

            // Step 2: 嘗試刪除檔案
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            var filePath = Path.Combine(uploadPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            // 你也可以在這裡 log 錯誤
            throw new Exception($"刪除過程出現錯誤：{ex.Message}");
        }
    }
}