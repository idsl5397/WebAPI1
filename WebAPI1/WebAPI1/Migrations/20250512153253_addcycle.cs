using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class addcycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KpiCycleId",
                table: "KpiDatas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KpiCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CycleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartYear = table.Column<int>(type: "int", nullable: false),
                    EndYear = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiCycles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiDatas_KpiCycleId",
                table: "KpiDatas",
                column: "KpiCycleId");

            migrationBuilder.AddForeignKey(
                name: "FK_KpiDatas_KpiCycles_KpiCycleId",
                table: "KpiDatas",
                column: "KpiCycleId",
                principalTable: "KpiCycles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KpiDatas_KpiCycles_KpiCycleId",
                table: "KpiDatas");

            migrationBuilder.DropTable(
                name: "KpiCycles");

            migrationBuilder.DropIndex(
                name: "IX_KpiDatas_KpiCycleId",
                table: "KpiDatas");

            migrationBuilder.DropColumn(
                name: "KpiCycleId",
                table: "KpiDatas");
        }
    }
}
