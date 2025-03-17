using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

public enum ReviewType : byte // 使用 byte 節省空間
{
    書面審查會議 = 0,
    實地進場查驗 = 1,
    領先指標輔導 = 2
}
public enum SuggestType : byte
{
    改善建議 = 0,
    精進建議 = 1,
    可資借鏡 = 2
}

public enum IsAdopted : byte
{
    是 = 0,
    否 = 1,
    不參採 = 2,
    詳備註 = 3
}

[Index("KpiFieldId", Name = "IX_SuggestDatas_KpiFieldId")]
[Index("CompanyId", Name = "IX_SuggestDatas_CompanyId")]
[Index("UserInfoNameId", Name = "IX_SuggestDatas_UserInfoNameId")]
public class SuggestData
{
    [Key]
    public int Id { get; set; }
    
    [Range(0, 10)]
    public int Year { get; set; } // 年份
    
    public int Quarter { get; set; } // 季度 (1~4)
    
    public string MonthAndDay { get; set; }
    
    public string SuggestionContent { get; set; } // 建議內容
    
    [Column(TypeName = "tinyint")] // 會議/活動類別
    public ReviewType ReviewMethod { get; set; }
    
    [Column(TypeName = "tinyint")] // 建議類別
    public SuggestType SuggestType { get; set; }
    
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
    
    public int? CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("SuggestDatas")]
    public virtual CompanyName? Company { get; set; }
    
    public int? KpiFieldId { get; set; }
    
    [ForeignKey("KpiFieldId")]
    [InverseProperty("SuggestDatas")]
    public virtual KpiField? KpiField { get; set; }
    
    public int? UserInfoNameId { get; set; } //委員使用者

    [ForeignKey("UserInfoNameId")]
    public virtual UserInfoName? UserInfoName { get; set; }
}