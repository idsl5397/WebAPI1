using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class addAllKpi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldItems");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.CreateTable(
                name: "KpiCategorys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiCategorys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    field = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    KpiReportValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiUnitDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitData = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiUnitDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiItem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KpiItemDetail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsApplied = table.Column<bool>(type: "bit", nullable: false),
                    BaselineYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BaselineValue = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KpiFieldId = table.Column<int>(type: "int", nullable: true),
                    KpiCategoryId = table.Column<int>(type: "int", nullable: true),
                    KpiUnitDataId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiDatas_CompanyNames_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyNames",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KpiDatas_KpiCategorys_KpiCategoryId",
                        column: x => x.KpiCategoryId,
                        principalTable: "KpiCategorys",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KpiDatas_KpiFields_KpiFieldId",
                        column: x => x.KpiFieldId,
                        principalTable: "KpiFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KpiDatas_KpiUnitDatas_KpiUnitDataId",
                        column: x => x.KpiUnitDataId,
                        principalTable: "KpiUnitDatas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiDatas_CompanyId",
                table: "KpiDatas",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiDatas_KpiCategoryId",
                table: "KpiDatas",
                column: "KpiCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiDatas_KpiFieldId",
                table: "KpiDatas",
                column: "KpiFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiDatas_KpiUnitDataId",
                table: "KpiDatas",
                column: "KpiUnitDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KpiDatas");

            migrationBuilder.DropTable(
                name: "KpiReports");

            migrationBuilder.DropTable(
                name: "KpiCategorys");

            migrationBuilder.DropTable(
                name: "KpiFields");

            migrationBuilder.DropTable(
                name: "KpiUnitDatas");

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    field = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fieldItem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldItems_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldItems_FieldId",
                table: "FieldItems",
                column: "FieldId");
        }
    }
}
