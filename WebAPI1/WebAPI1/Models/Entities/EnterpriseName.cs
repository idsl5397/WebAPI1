using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

public class EnterpriseName
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string enterprise { get; set; } = null!;
    
    [InverseProperty("Enterprise")]
    public virtual ICollection<CompanyName> CompanyNames { get; set; } = new List<CompanyName>();
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
}
