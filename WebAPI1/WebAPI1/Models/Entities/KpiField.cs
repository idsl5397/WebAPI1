﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

public class KpiField
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string field { get; set; }
    
    [StringLength(50)]
    public string enfield { get; set; }
    
    public virtual ICollection<KpiItem> KpiItems { get; set; } = new List<KpiItem>();
    public virtual ICollection<SuggestData> SuggestDatas { get; set; } = new List<SuggestData>();
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }
}