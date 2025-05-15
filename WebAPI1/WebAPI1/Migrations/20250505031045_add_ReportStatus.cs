using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class add_ReportStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "KpiReports",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "KpiReports");
        }
    }
}
