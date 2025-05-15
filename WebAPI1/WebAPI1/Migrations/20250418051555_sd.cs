using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class sd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KpiItems_IndicatorNumber_KpiCategoryId_OrganizationId",
                table: "KpiItems");

            migrationBuilder.RenameColumn(
                name: "productionSite",
                table: "KpiDatas",
                newName: "ProductionSite");

            migrationBuilder.AlterColumn<decimal>(
                name: "KpiReportValue",
                table: "KpiReports",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetValue",
                table: "KpiDatas",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaselineValue",
                table: "KpiDatas",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.CreateIndex(
                name: "IX_KpiItems_IndicatorNumber_KpiCategoryId_OrganizationId_KpiFieldId",
                table: "KpiItems",
                columns: new[] { "IndicatorNumber", "KpiCategoryId", "OrganizationId", "KpiFieldId" },
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KpiItems_IndicatorNumber_KpiCategoryId_OrganizationId_KpiFieldId",
                table: "KpiItems");

            migrationBuilder.RenameColumn(
                name: "ProductionSite",
                table: "KpiDatas",
                newName: "productionSite");

            migrationBuilder.AlterColumn<decimal>(
                name: "KpiReportValue",
                table: "KpiReports",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetValue",
                table: "KpiDatas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaselineValue",
                table: "KpiDatas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiItems_IndicatorNumber_KpiCategoryId_OrganizationId",
                table: "KpiItems",
                columns: new[] { "IndicatorNumber", "KpiCategoryId", "OrganizationId" },
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");
        }
    }
}
