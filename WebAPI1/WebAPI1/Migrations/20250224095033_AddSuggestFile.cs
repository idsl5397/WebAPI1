using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuggestFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    UserInfoNameId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestFile_CompanyNames_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyNames",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuggestFile_UserInfoNames_UserInfoNameId",
                        column: x => x.UserInfoNameId,
                        principalTable: "UserInfoNames",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuggestFiles_CompanyId",
                table: "SuggestFile",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestFiles_UserInfoNameId",
                table: "SuggestFile",
                column: "UserInfoNameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuggestFile");
        }
    }
}
