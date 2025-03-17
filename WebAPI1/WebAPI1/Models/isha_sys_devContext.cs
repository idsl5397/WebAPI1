using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebAPI1.Entities;

namespace WebAPI1.Models;

public partial class isha_sys_devContext : DbContext
{
    public isha_sys_devContext(DbContextOptions<isha_sys_devContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<EnterpriseName> EnterpriseNames { get; set; }
    public virtual DbSet<CompanyName> CompanyNames { get; set; }
    public virtual DbSet<FactoryName> FactoryNames { get; set; }
    public virtual DbSet<UserInfoName> UserInfoNames { get; set; }
    public virtual DbSet<DomainName> DomainNames { get; set; }
    public virtual DbSet<KpiField> KpiFields { get; set; }
    public virtual DbSet<KpiCategory> KpiCategorys { get; set; }
    public virtual DbSet<KpiUnitData> KpiUnitDatas { get; set; }
    public virtual DbSet<KpiData> KpiDatas { get; set; }
    public virtual DbSet<KpiReport> KpiReports { get; set; }
    public virtual DbSet<SuggestData> SuggestDatas { get; set; }
    public virtual DbSet<SuggestFile> SuggestFiles { get; set; }
    public virtual DbSet<DataChangeLog> DataChangeLogs { get; set; }
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<MenuRole> MenusRoles { get; set; }
        
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
        // EnterpriseName 與 CompanyName 關聯設定
        modelBuilder.Entity<EnterpriseName>()
            .HasMany(e => e.CompanyNames)
            .WithOne(c => c.Enterprise)
            .HasForeignKey(c => c.EnterpriseId)
            .OnDelete(DeleteBehavior.NoAction);

        // CompanyName 與 FactoryName 關聯設定
        modelBuilder.Entity<CompanyName>()
            .HasMany(c => c.FactoryNames)
            .WithOne(f => f.Company)
            .HasForeignKey(f => f.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // CompanyName 與 DomainName 關聯設定
        modelBuilder.Entity<CompanyName>()
            .HasMany(c => c.DomainNames)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // CompanyName 與 KpiData 關聯設定
        modelBuilder.Entity<CompanyName>()
            .HasMany(c => c.KpiDatas)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // KpiField 與 KpiData 關聯設定
        modelBuilder.Entity<KpiField>()
            .HasMany(c => c.KpiDatas)
            .WithOne(d => d.KpiField)
            .HasForeignKey(d => d.KpiFieldId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // KpiCategory 與 KpiData 關聯設定
        modelBuilder.Entity<KpiCategory>()
            .HasMany(c => c.KpiDatas)
            .WithOne(d => d.KpiCategory)
            .HasForeignKey(d => d.KpiCategoryId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // KpiUnitData 與 KpiData 關聯設定
        modelBuilder.Entity<KpiUnitData>()
            .HasMany(c => c.KpiDatas)
            .WithOne(d => d.KpiUnitData)
            .HasForeignKey(d => d.KpiUnitDataId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // KpiData 與 KpiReport 關聯設定
        modelBuilder.Entity<KpiData>()
            .HasMany(d => d.KpiReports)
            .WithOne(r => r.KpiData)
            .HasForeignKey(r => r.KpiDataId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // CompanyName 與 SuggestData 關聯設定
        modelBuilder.Entity<CompanyName>()
            .HasMany(c => c.SuggestDatas)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // KpiField 與 SuggestData 關聯設定
        modelBuilder.Entity<KpiField>()
            .HasMany(c => c.SuggestDatas)
            .WithOne(s => s.KpiField)
            .HasForeignKey(s => s.KpiFieldId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // UserInfo 與 SuggestData 關聯設定
        modelBuilder.Entity<UserInfoName>()
            .HasMany(c => c.SuggestDatas)
            .WithOne(s => s.UserInfoName)
            .HasForeignKey(s => s.UserInfoNameId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // CompanyName 與 SuggestFile 關聯設定
        modelBuilder.Entity<CompanyName>()
            .HasMany(c => c.SuggestFiles)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // UserInfo 與 SuggestFile 關聯設定
        modelBuilder.Entity<UserInfoName>()
            .HasMany(c => c.SuggestFiles)
            .WithOne(s => s.UserInfoName)
            .HasForeignKey(s => s.UserInfoNameId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Menu 與 MenuRole 關聯設定
        modelBuilder.Entity<Menu>()
            .HasMany(e => e.MenuRoles)
            .WithOne(c => c.Menu)
            .HasForeignKey(c => c.MenuId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
