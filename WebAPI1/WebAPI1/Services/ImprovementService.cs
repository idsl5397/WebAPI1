﻿using System.Text.RegularExpressions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WebAPI1.Context;
using WebAPI1.Entities;
using File = System.IO.File;

namespace WebAPI1.Services;

public interface IImprovementService
{
    Task<List<ImprovementService.UploadedFileDto>> GetUploadedFilesAsync(int orgId);
    Task<bool> SubmitReportAsync(int orgId, int year, int quarter, string filePath, Guid userId);
    Task<bool> DeleteFileAsync(string filePath, int? orgId = null, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string orgId, string fileName, CancellationToken ct = default);
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
    public class UploadedFileDto
    {
        public string Year { get; set; }
        public string Quarter { get; set; }
        
        public string FilePath { get; set; } = default!;
        public string OriName { get; set; } = default!;
    }
    public async Task<List<UploadedFileDto>> GetUploadedFilesAsync(int orgId)
    {
        return await _db.SuggestFiles
            .Where(sf => sf.OrganizationId == orgId)
            .OrderByDescending(sf => sf.file.CreatedAt)
            .Select(sf => new UploadedFileDto
            {
                Year = sf.Year.ToString(),
                Quarter = sf.Quarter.ToString(),
                FilePath = sf.file.FilePath,
                OriName = sf.file.FileName, // 原始檔名
            })
            .ToListAsync();
    }


    public async Task<bool> SubmitReportAsync(int orgId, int year, int quarter, string filePath, Guid userId)
    {
        // 驗證組織是否存在
        var org = await _db.Organizations.FindAsync(orgId);
        if (org == null) throw new Exception("無效的組織 ID");

        // 驗證用戶是否存在
        var user = await _db.Users.FindAsync(userId);
        if (user == null) throw new Exception("無效的用戶 ID");

        // 檢查是否已存在相同季度的報告
        bool exists = await _db.SuggestFiles.AnyAsync(f =>
            f.OrganizationId == orgId &&
            f.Year == year &&
            f.Quarter == quarter
        );

        if (exists) throw new Exception("此季度的報告已上傳，請勿重複上傳");

        // 驗證檔案路徑
        if (string.IsNullOrWhiteSpace(filePath))
            throw new Exception("檔案路徑不能為空");

        // 根據檔案路徑查找 File 記錄
        var existingFile = await _db.Files.FirstOrDefaultAsync(f => f.FilePath == filePath && f.IsActive == true);
        if (existingFile == null)
            throw new Exception("找不到對應的檔案記錄，請確認檔案路徑是否正確");

        // 生成顯示用的檔案名稱
        string displayFileName = $"績效指標推動執行總成果及後續規劃報告-{org.Name}-{year}年-Q{quarter}";

        try
        {
            // 創建 SuggestFile 記錄，關聯到現有的 File
            var newSuggestFile = new SuggestFile
            {
                Year = year,
                Quarter = quarter,
                ReportName = displayFileName,
                CreatedAt = tool.GetTaiwanNow(),
                UpdateAt = tool.GetTaiwanNow(),
                OrganizationId = orgId,
                FileId = existingFile.Id  // 關聯到現有的 File 記錄
            };

            _db.SuggestFiles.Add(newSuggestFile);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            throw;
        }
    }
    
    public async Task<bool> DeleteFileAsync(string filePath, int? orgId = null, CancellationToken ct = default)
    {
        // filePath 是資料庫 File.FilePath（例如：/Files/Reports/xxx.xlsx）
        // orgId 可選：若有傳，僅刪該公司關聯；沒傳則刪此 file 的所有關聯

        using var trx = await _db.Database.BeginTransactionAsync(ct);

        // 1) 找出檔案主檔
        var file = await _db.Files
            .FirstOrDefaultAsync(f => f.FilePath == filePath, ct);
        if (file == null) return false;

        // 2) 刪除關聯（可選公司）
        var linksQuery = _db.SuggestFiles.Where(sf => sf.FileId == file.Id);
        if (orgId.HasValue)
            linksQuery = linksQuery.Where(sf => sf.OrganizationId == orgId.Value);

        var links = await linksQuery.ToListAsync(ct);
        if (links.Count == 0) return false;

        _db.SuggestFiles.RemoveRange(links);
        await _db.SaveChangesAsync(ct);

        // 3) 檢查是否仍被其他關聯使用
        var stillLinked = await _db.SuggestFiles.AnyAsync(sf => sf.FileId == file.Id, ct);

        // 4) 若無其他關聯，刪除 File 主檔與實體檔案
        if (!stillLinked)
        {
            _db.Files.Remove(file);
            await _db.SaveChangesAsync(ct);

            // 實體路徑：以 WebRoot 為主，無則用 ContentRoot
            var root = _env.WebRootPath ?? _env.ContentRootPath;
            // FilePath 通常是以 / 開頭的相對路徑
            var absolutePath = Path.Combine(root, file.FilePath.TrimStart('/', '\\'));

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        await trx.CommitAsync(ct);
        return true;
    }
    
    private readonly FileExtensionContentTypeProvider _ctp = new();
    private string Root => Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
    private static string SanitizeFileName(string fileName)
    {
        // 去除危險字元，避免路徑穿越
        fileName = Path.GetFileName(fileName);
        fileName = Regex.Replace(fileName, @"[^\w\u4e00-\u9fa5\-\.\(\)（）【】\[\]\s]", "_");
        return fileName;
    }
    public Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string orgId, string fileName, CancellationToken ct = default)
    {
        var safe = SanitizeFileName(fileName);
        var abs = Path.GetFullPath(Path.Combine(Root, safe));
        if (!abs.StartsWith(Root, StringComparison.OrdinalIgnoreCase)) return Task.FromResult<(Stream, string, string)?>(null);
        if (!File.Exists(abs)) return Task.FromResult<(Stream, string, string)?>(null);

        if (!_ctp.TryGetContentType(abs, out var contentType))
            contentType = "application/octet-stream";

        var stream = new FileStream(abs, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<(Stream, string, string)?>( (stream, contentType, safe) );
    }
    
    
    private string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "text/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".webp" => "image/webp",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            _ => "application/octet-stream"
        };
    }
}