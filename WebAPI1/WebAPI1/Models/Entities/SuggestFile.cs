using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

public class SuggestFile
{
    [Key]
    public int Id { get; set; } // 主鍵
    
    public int Year { get; set; } // 年份
    
    public int Quarter { get; set; } // 所屬季度
    
    [MaxLength(255)]
    public string ReportName { get; set; } // 報告書名稱

    [MaxLength(100)]
    public string? ReportType { get; set; } // 報告書類型

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
    
    public int? OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; }

    public Guid? UserId { get; set; } //上傳人員

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}