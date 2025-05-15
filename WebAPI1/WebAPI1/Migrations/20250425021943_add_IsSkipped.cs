using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class add_IsSkipped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSkipped",
                table: "KpiReports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSkipped",
                table: "KpiReports");
        }
    }
}
