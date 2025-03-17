using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

[Index("KpiFieldId", Name = "IX_KpiDatas_KpiFieldId")]
[Index("KpiCategoryId", Name = "IX_KpiDatas_KpiCategoryId")]
[Index("KpiUnitDataId", Name = "IX_KpiDatas_KpiUnitDataId")]
[Index("CompanyId", Name = "IX_KpiDatas_CompanyId")]
public class KpiData
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(50)]
    public string KpiItem { get; set; }
    
    [StringLength(50)]
    public string KpiItemDetail { get; set; }
    
    public bool IsApplied { get; set; }
    
    [StringLength(20)]
    public string BaselineYear { get; set; }
    
    [Range(0, 10)]
    public int BaselineValue { get; set; }

    [Range(0, 10)]
    public int TargetValue { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Remarks { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
    
    public virtual ICollection<KpiReport> KpiReports { get; set; } = new List<KpiReport>();
    
    //外鍵
    public int? KpiFieldId { get; set; }

    [ForeignKey("KpiFieldId")]
    [InverseProperty("KpiDatas")]
    public virtual KpiField? KpiField { get; set; }
    
    public int? KpiCategoryId { get; set; }

    [ForeignKey("KpiCategoryId")]
    [InverseProperty("KpiDatas")]
    public virtual KpiCategory? KpiCategory { get; set; }
    
    public int? KpiUnitDataId { get; set; }

    [ForeignKey("KpiUnitDataId")]
    [InverseProperty("KpiDatas")]
    public virtual KpiUnitData? KpiUnitData { get; set; }
    
    public int? CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("KpiDatas")]
    public virtual CompanyName? Company { get; set; }
}