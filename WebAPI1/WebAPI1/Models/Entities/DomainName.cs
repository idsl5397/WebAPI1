using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

[Index("CompanyId", Name = "IX_FactoryNames_CompanyId")]
public class DomainName
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string domain { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
    
    public int? CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("DomainNames")]
    public virtual CompanyName? Company { get; set; }
}