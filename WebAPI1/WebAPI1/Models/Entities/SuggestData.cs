using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;


public enum IsAdopted : byte
{
    否 = 0,
    是 = 1,
    不參採 = 2,
    詳備註 = 3
}

public class SuggestData
{
    [Key]
    public int Id { get; set; }
    
    [Range(0, 10)]
    public int Year { get; set; } // 年份
    
    public int Quarter { get; set; } // 季度 (1~4)
    
    public string MonthAndDay { get; set; }
    
    public string SuggestionContent { get; set; } // 建議內容

    // 會議/活動類別
    public int SuggestEventTypeId { get; set; }

    [ForeignKey("SuggestEventTypeId")]
    public SuggestEventType SuggestEventType { get; set; }

    // 建議類別
    public int SuggestionTypeId { get; set; }

    [ForeignKey("SuggestionTypeId")]
    public SuggestionType SuggestionType { get; set; }
    
    
    [Column(TypeName = "tinyint")] // 是否參採
    public IsAdopted IsAdopted { get; set; }
    
    public string? RespDept { get; set; } //負責部門
    
    [MaxLength(500)]
    public string? ImproveDetails { get; set; } // 改善對策/辦理情形
    
    public int? Manpower { get; set; } // 投入人力
    
    public decimal? Budget { get; set; } // 投入改善經費
    
    [Column(TypeName = "tinyint")] // 是否完成改善
    public IsAdopted Completed { get; set; }
    
    public int? DoneYear { get; set; } // 完成/預計完成日期 年份
    public int? DoneMonth { get; set; } // 完成/預計完成日期 月份
    
    [Column(TypeName = "tinyint")] // 是否平行展開
    public IsAdopted ParallelExec { get; set; }
    
    [MaxLength(500)]
    public string? ExecPlan { get; set; } // 平行展開執行規劃
    
    [MaxLength(500)]
    public string? Remark { get; set; } // 平行展開執行規劃
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
    
    public int? OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; }
    
    public int? KpiFieldId { get; set; }
    
    [ForeignKey("KpiFieldId")]
    [InverseProperty("SuggestDatas")]
    public virtual KpiField? KpiField { get; set; }
    
    public Guid UserId { get; set; } //委員使用者

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}