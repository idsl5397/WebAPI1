using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

[Index("KpiDataId", Name = "IX_KpiReports_KpiDataId")]
public class KpiReport
{
    [Key]
    public int Id { get; set; }
    
    [Range(0, 10)]
    public int Year { get; set; }
    
    //季度(如 Q1、Q2、Q3、Q4、H1、Y 全年)
    public string Period { get; set; }
    
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? KpiReportValue { get; set; }
    
    public int KpiDataId { get; set; }
    
    [ForeignKey("KpiDataId")]
    public virtual KpiData KpiData { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}