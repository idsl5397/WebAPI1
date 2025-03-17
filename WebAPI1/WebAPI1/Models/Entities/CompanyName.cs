using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

[Index("EnterpriseId", Name = "IX_CompanyNames_EnterpriseId")]
public partial class CompanyName
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string company { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? EnterpriseId { get; set; }

    [ForeignKey("EnterpriseId")]
    [InverseProperty("CompanyNames")]
    public virtual EnterpriseName? Enterprise { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<FactoryName> FactoryNames { get; set; } = new List<FactoryName>();
    public virtual ICollection<DomainName> DomainNames { get; set; } = new List<DomainName>();
    public virtual ICollection<KpiData> KpiDatas { get; set; } = new List<KpiData>();
    public virtual ICollection<SuggestData> SuggestDatas { get; set; } = new List<SuggestData>();
    public virtual ICollection<SuggestFile> SuggestFiles { get; set; } = new List<SuggestFile>();
}
