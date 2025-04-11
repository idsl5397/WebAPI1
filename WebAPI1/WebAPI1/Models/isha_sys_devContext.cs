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
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<OrganizationType> OrganizationTypes { get; set; }  
    public virtual DbSet<OrganizationDomain> OrganizationDomains { get; set; }  
    public virtual DbSet<OrganizationHierarchy> OrganizationHierarchies { get; set; }  
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<UserPasswordHistory> UserRPasswordHistories { get; set; }
    
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
        
//--------------------    Organization    --------------------
        // 設定 Organization 與 User 關聯
        // 設定 Organization 與 User 關聯
         modelBuilder.Entity<OrganizationType>()
            .HasKey(ot => ot.Id);

        modelBuilder.Entity<OrganizationType>()
            .HasIndex(ot => ot.TypeCode)
            .IsUnique();

        // 設定 Organization 表
        modelBuilder.Entity<Organization>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<Organization>()
            .HasIndex(o => o.Code)
            .IsUnique();

        // 設定 Organization 與 OrganizationType 關聯
        modelBuilder.Entity<Organization>()
            .HasOne(o => o.OrganizationType)
            .WithMany(ot => ot.Organizations)
            .HasForeignKey(o => o.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 設定 Organization 自關聯 (parent-child)
        modelBuilder.Entity<Organization>()
            .HasOne(o => o.ParentOrganization)
            .WithMany(o => o.ChildOrganizations)
            .HasForeignKey(o => o.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        // 與 OrganizationDomain 的一對多關聯
        modelBuilder.Entity<Organization>()
            .HasMany(o => o.Domains)
            .WithOne(d => d.Organization)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // 設定 OrganizationHierarchy 表
        modelBuilder.Entity<OrganizationHierarchy>()
            .HasKey(oh => oh.Id);


        // 設定 OrganizationHierarchy 與 OrganizationType 關聯 (父類型)
        modelBuilder.Entity<OrganizationHierarchy>()
            .HasOne(oh => oh.ParentType)
            .WithMany(ot => ot.ParentHierarchies)
            .HasForeignKey(oh => oh.ParentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 設定 OrganizationHierarchy 與 OrganizationType 關聯 (子類型)
        modelBuilder.Entity<OrganizationHierarchy>()
            .HasOne(oh => oh.ChildType)
            .WithMany(ot => ot.ChildHierarchies)
            .HasForeignKey(oh => oh.ChildTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 設定 OrganizationHierarchy 唯一索引
        modelBuilder.Entity<OrganizationHierarchy>()
            .HasIndex(oh => new { oh.ParentTypeId, oh.ChildTypeId })
            .IsUnique();
        
//--------------------     User    --------------------
        
        // 設定 User 與 UserRole 關聯
        modelBuilder.Entity<User>(entity =>
        {
            // User與Organization的關聯
            entity.HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // 用戶名唯一索引
            entity.HasIndex(u => u.Username).IsUnique();
            
            // 電子郵件索引
            entity.HasIndex(u => u.Email);
        });
        
        // UserPasswordHistory表關聯配置
        modelBuilder.Entity<UserPasswordHistory>(entity =>
        {
            entity.HasOne(uph => uph.User)
                .WithMany(u => u.PasswordHistory)
                .HasForeignKey(uph => uph.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 創建時間索引（用於按時間排序）
            entity.HasIndex(uph => uph.CreatedAt);
            
            // 複合索引（用於快速查找特定用戶的密碼歷史）
            entity.HasIndex(uph => new { uph.UserId, uph.CreatedAt });
        });
        
        // 設定 UserRole 表 (多對多關聯表)
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => ur.Id);

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        // 設定 UserRole 與 User 關聯
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
