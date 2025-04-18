using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI1.Entities;

public class KpiData
{
    [Key]
    public int Id { get; set; }
    
    public bool IsApplied { get; set; }
    
    //基線值數據年份
    [StringLength(20)]
    public string BaselineYear { get; set; }
    
    //基線值
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? BaselineValue { get; set; }

    //目標值
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? TargetValue { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Remarks { get; set; }

    
    //外鍵
    public int DetailItemId { get; set; }

    [ForeignKey("DetailItemId")]
    public virtual KpiDetailItem DetailItem { get; set; }
        
    public int? OrganizationId { get; set; }
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; }
    
    public virtual ICollection<KpiReport> KpiReports { get; set; } = new List<KpiReport>();
    
    //工廠/製程廠
    public string? ProductionSite { get; set; }
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

}