using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI1.Entities;

public enum Operation : byte
{
    新增 = 0,
    刪除 = 1,
    修改 = 2
}

public class DataChangeLog
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]  // 限制最大長度為 100
    public string TableName { get; set; }

    [Required]
    public int RecordId { get; set; }

    [Column(TypeName = "tinyint")]
    public Operation OperationType { get; set; } //'新增', '修改', '刪除'

    public string? NewData { get; set; } //修改後資料

    [Required]
    [MaxLength(50)]
    public string ModifiedBy { get; set; }

    [Required]
    public DateTime ModifyDate { get; set; }
}