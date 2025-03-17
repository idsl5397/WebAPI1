using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class DataToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KpiDataId",
                table: "KpiReports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiReports_KpiDataId",
                table: "KpiReports",
                column: "KpiDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_KpiReports_KpiDatas_KpiDataId",
                table: "KpiReports",
                column: "KpiDataId",
                principalTable: "KpiDatas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KpiReports_KpiDatas_KpiDataId",
                table: "KpiReports");

            migrationBuilder.DropIndex(
                name: "IX_KpiReports_KpiDataId",
                table: "KpiReports");

            migrationBuilder.DropColumn(
                name: "KpiDataId",
                table: "KpiReports");
        }
    }
}
