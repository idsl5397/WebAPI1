using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;

namespace WebAPI1.Context;

public partial class ISHAuditDbcontext : DbContext
{
    public ISHAuditDbcontext()
    {
    }

    public ISHAuditDbcontext(DbContextOptions<ISHAuditDbcontext> options)
        : base(options)
    {
    }

    public virtual DbSet<CompanyName> CompanyNames { get; set; }
    public virtual DbSet<EnterpriseName> EnterpriseNames { get; set; }
    public virtual DbSet<FactoryName> FactoryNames { get; set; }
    public virtual DbSet<UserInfoName> UserInfoNames { get; set; }
    public virtual DbSet<DomainName> DomainNames { get; set; }
    public virtual DbSet<KpiField> KpiFields { get; set; }
    public virtual DbSet<KpiCategory> KpiCategorys { get; set; }
    public virtual DbSet<KpiUnitData> KpiUnitDatas { get; set; }
    public virtual DbSet<KpiData> KpiDatas { get; set; }
    public virtual DbSet<KpiReport> KpiReports { get; set; }
    public virtual DbSet<SuggestData> SuggestDatas { get; set; }
    public virtual DbSet<DataChangeLog> DataChangeLogs { get; set; }
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<MenuRole> MenusRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=127.0.0.1;Database=isha_kpi;User ID=sa;Password=04861064;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
