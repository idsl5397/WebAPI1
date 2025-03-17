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
    [Migration("20250108060511_userinfo")]
    partial class userinfo
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
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "EnterpriseId" }, "IX_CompanyNames_EnterpriseId");

                    b.ToTable("CompanyNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.EnterpriseName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("code")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("enterprise")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

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
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CompanyId" }, "IX_FactoryNames_CompanyId");

                    b.ToTable("FactoryNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.UserInfoName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AnyLLMWorkspace")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("anyllmworkspace");

                    b.Property<string>("Authority")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("authority");

                    b.Property<string>("Avatar")
                        .HasColumnType("varchar(max)")
                        .HasColumnName("avatar");

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

            modelBuilder.Entity("WebAPI1.Entities.FactoryName", b =>
                {
                    b.HasOne("WebAPI1.Entities.CompanyName", "Company")
                        .WithMany("FactoryNames")
                        .HasForeignKey("CompanyId");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("WebAPI1.Entities.CompanyName", b =>
                {
                    b.Navigation("FactoryNames");
                });

            modelBuilder.Entity("WebAPI1.Entities.EnterpriseName", b =>
                {
                    b.Navigation("CompanyNames");
                });
#pragma warning restore 612, 618
        }
    }
}
