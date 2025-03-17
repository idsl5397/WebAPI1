﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebAPI1.Context;

#nullable disable

namespace WebAPI1.Migrations
{
    [DbContext(typeof(ISHAuditDbcontext))]
    [Migration("20250224024910_DataToReport")]
    partial class DataToReport
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebAPI1.Entities.CompanyName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("EnterpriseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("company")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "EnterpriseId" }, "IX_CompanyNames_EnterpriseId");

                    b.ToTable("CompanyNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.DomainName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("domain")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CompanyId" }, "IX_FactoryNames_CompanyId");

                    b.ToTable("DomainNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.EnterpriseName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("enterprise")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.HasKey("Id");

                    b.ToTable("EnterpriseNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.FactoryName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("factory")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CompanyId" }, "IX_FactoryNames_CompanyId");

                    b.ToTable("FactoryNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("KpiCategorys");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BaselineValue")
                        .HasColumnType("int");

                    b.Property<string>("BaselineYear")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsApplied")
                        .HasColumnType("bit");

                    b.Property<int?>("KpiCategoryId")
                        .HasColumnType("int");

                    b.Property<int?>("KpiFieldId")
                        .HasColumnType("int");

                    b.Property<string>("KpiItem")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("KpiItemDetail")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("KpiUnitDataId")
                        .HasColumnType("int");

                    b.Property<string>("Remarks")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TargetValue")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CompanyId" }, "IX_KpiDatas_CompanyId");

                    b.HasIndex(new[] { "KpiCategoryId" }, "IX_KpiDatas_KpiCategoryId");

                    b.HasIndex(new[] { "KpiFieldId" }, "IX_KpiDatas_KpiFieldId");

                    b.HasIndex(new[] { "KpiUnitDataId" }, "IX_KpiDatas_KpiUnitDataId");

                    b.ToTable("KpiDatas");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("field")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("KpiFields");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("KpiDataId")
                        .HasColumnType("int");

                    b.Property<int>("KpiReportValue")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "KpiDataId" }, "IX_KpiReports_KpiDataId");

                    b.ToTable("KpiReports");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiUnitData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("UnitData")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("KpiUnitDatas");
                });

            modelBuilder.Entity("WebAPI1.Entities.UserInfoName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Authority")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("authority");

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("varchar(max)")
                        .HasColumnName("email");

                    b.Property<int?>("EnterpriseId")
                        .HasColumnType("int");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("int");

                    b.Property<string>("Mobile")
                        .HasColumnType("varchar(40)")
                        .HasColumnName("mobile");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("nickname");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("password");

                    b.Property<byte[]>("Salt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(25)")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.ToTable("UserInfoNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.CompanyName", b =>
                {
                    b.HasOne("WebAPI1.Entities.EnterpriseName", "Enterprise")
                        .WithMany("CompanyNames")
                        .HasForeignKey("EnterpriseId");

                    b.Navigation("Enterprise");
                });

            modelBuilder.Entity("WebAPI1.Entities.DomainName", b =>
                {
                    b.HasOne("WebAPI1.Entities.CompanyName", "Company")
                        .WithMany("DomainNames")
                        .HasForeignKey("CompanyId");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("WebAPI1.Entities.FactoryName", b =>
                {
                    b.HasOne("WebAPI1.Entities.CompanyName", "Company")
                        .WithMany("FactoryNames")
                        .HasForeignKey("CompanyId");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiData", b =>
                {
                    b.HasOne("WebAPI1.Entities.CompanyName", "Company")
                        .WithMany("KpiDatas")
                        .HasForeignKey("CompanyId");

                    b.HasOne("WebAPI1.Entities.KpiCategory", "KpiCategory")
                        .WithMany("KpiDatas")
                        .HasForeignKey("KpiCategoryId");

                    b.HasOne("WebAPI1.Entities.KpiField", "KpiField")
                        .WithMany("KpiDatas")
                        .HasForeignKey("KpiFieldId");

                    b.HasOne("WebAPI1.Entities.KpiUnitData", "KpiUnitData")
                        .WithMany("KpiDatas")
                        .HasForeignKey("KpiUnitDataId");

                    b.Navigation("Company");

                    b.Navigation("KpiCategory");

                    b.Navigation("KpiField");

                    b.Navigation("KpiUnitData");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiReport", b =>
                {
                    b.HasOne("WebAPI1.Entities.KpiData", "KpiData")
                        .WithMany("KpiReports")
                        .HasForeignKey("KpiDataId");

                    b.Navigation("KpiData");
                });

            modelBuilder.Entity("WebAPI1.Entities.CompanyName", b =>
                {
                    b.Navigation("DomainNames");

                    b.Navigation("FactoryNames");

                    b.Navigation("KpiDatas");
                });

            modelBuilder.Entity("WebAPI1.Entities.EnterpriseName", b =>
                {
                    b.Navigation("CompanyNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiCategory", b =>
                {
                    b.Navigation("KpiDatas");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiData", b =>
                {
                    b.Navigation("KpiReports");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiField", b =>
                {
                    b.Navigation("KpiDatas");
                });

            modelBuilder.Entity("WebAPI1.Entities.KpiUnitData", b =>
                {
                    b.Navigation("KpiDatas");
                });
#pragma warning restore 612, 618
        }
    }
}
