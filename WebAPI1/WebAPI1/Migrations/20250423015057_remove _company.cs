using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class remove_company : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KpiDatas_CompanyNames_CompanyNameId",
                table: "KpiDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_SuggestDatas_CompanyNames_CompanyId",
                table: "SuggestDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_SuggestFile_CompanyNames_CompanyId",
                table: "SuggestFile");

            migrationBuilder.DropTable(
                name: "DomainNames");

            migrationBuilder.DropTable(
                name: "FactoryNames");

            migrationBuilder.DropTable(
                name: "CompanyNames");

            migrationBuilder.DropTable(
                name: "EnterpriseNames");

            migrationBuilder.DropIndex(
                name: "IX_KpiDatas_CompanyNameId",
                table: "KpiDatas");

            migrationBuilder.DropColumn(
                name: "CompanyNameId",
                table: "KpiDatas");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "SuggestFile",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_SuggestFiles_CompanyId",
                table: "SuggestFile",
                newName: "IX_SuggestFile_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "SuggestDatas",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_SuggestDatas_CompanyId",
                table: "SuggestDatas",
                newName: "IX_SuggestDatas_OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestDatas_Organizations_OrganizationId",
                table: "SuggestDatas",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestFile_Organizations_OrganizationId",
                table: "SuggestFile",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuggestDatas_Organizations_OrganizationId",
                table: "SuggestDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_SuggestFile_Organizations_OrganizationId",
                table: "SuggestFile");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "SuggestFile",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_SuggestFile_OrganizationId",
                table: "SuggestFile",
                newName: "IX_SuggestFiles_CompanyId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "SuggestDatas",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_SuggestDatas_OrganizationId",
                table: "SuggestDatas",
                newName: "IX_SuggestDatas_CompanyId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyNameId",
                table: "KpiDatas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnterpriseNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    enterprise = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnterpriseNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnterpriseId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    company = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyNames_EnterpriseNames_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "EnterpriseNames",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DomainNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    domain = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainNames_CompanyNames_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyNames",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FactoryNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    factory = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryNames_CompanyNames_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyNames",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiDatas_CompanyNameId",
                table: "KpiDatas",
                column: "CompanyNameId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyNames_EnterpriseId",
                table: "CompanyNames",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryNames_CompanyId",
                table: "DomainNames",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryNames_CompanyId",
                table: "FactoryNames",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_KpiDatas_CompanyNames_CompanyNameId",
                table: "KpiDatas",
                column: "CompanyNameId",
                principalTable: "CompanyNames",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestDatas_CompanyNames_CompanyId",
                table: "SuggestDatas",
                column: "CompanyId",
                principalTable: "CompanyNames",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestFile_CompanyNames_CompanyId",
                table: "SuggestFile",
                column: "CompanyId",
                principalTable: "CompanyNames",
                principalColumn: "Id");
        }
    }
}
