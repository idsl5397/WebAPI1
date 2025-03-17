using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;


[Index("CompanyId", Name = "IX_SuggestFiles_CompanyId")]
[Index("UserInfoNameId", Name = "IX_SuggestFiles_UserInfoNameId")]
public class SuggestFile
{
    [Key]
    public int Id { get; set; } // 主鍵
    
    public int Year { get; set; } // 年份
    
    public int Quarter { get; set; } // 所屬季度
    
    [MaxLength(255)]
    public string ReportName { get; set; } // 報告書名稱

    [MaxLength(100)]
    public string ReportType { get; set; } // 報告書類型

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
    
    public int? CompanyId { get; set; } // 公司 (外鍵)

    [ForeignKey("CompanyId")]
    [InverseProperty("SuggestFiles")]
    public virtual CompanyName? Company { get; set; }

    public int? UserInfoNameId { get; set; } //上傳人員

    [ForeignKey("UserInfoNameId")]
    public virtual UserInfoName? UserInfoName { get; set; }
}