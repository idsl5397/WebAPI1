using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Entities
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnterpriseNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enterprise = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
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
                    company = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnterpriseId = table.Column<int>(type: "int", nullable: true)
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
                name: "FactoryNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    factory = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_CompanyNames_EnterpriseId",
                table: "CompanyNames",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryNames_CompanyId",
                table: "FactoryNames",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactoryNames");

            migrationBuilder.DropTable(
                name: "CompanyNames");

            migrationBuilder.DropTable(
                name: "EnterpriseNames");
        }
    }
}
