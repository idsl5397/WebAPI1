using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

//指標類別
//ex: 基礎型/客制型
public class KpiCategory
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Category { get; set; }
    
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
    
    [InverseProperty("KpiCategory")]
    public virtual ICollection<KpiData> KpiDatas { get; set; } = new List<KpiData>();
}