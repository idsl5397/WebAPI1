using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class IsUnique_Kpireport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Period",
                table: "KpiReports",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_KpiReports_KpiDataId_Year_Period",
                table: "KpiReports",
                columns: new[] { "KpiDataId", "Year", "Period" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KpiReports_KpiDataId_Year_Period",
                table: "KpiReports");

            migrationBuilder.AlterColumn<string>(
                name: "Period",
                table: "KpiReports",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
