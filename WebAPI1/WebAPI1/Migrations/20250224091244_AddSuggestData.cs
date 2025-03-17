using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "KpiReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "KpiDatas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "SuggestDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: false),
                    MonthAndDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestionContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewMethod = table.Column<byte>(type: "tinyint", nullable: false),
                    SuggestType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsAdopted = table.Column<byte>(type: "tinyint", nullable: false),
                    RespDept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImproveDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Manpower = table.Column<int>(type: "int", nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Completed = table.Column<byte>(type: "tinyint", nullable: false),
                    DoneYear = table.Column<int>(type: "int", nullable: true),
                    DoneMonth = table.Column<int>(type: "int", nullable: true),
                    ParallelExec = table.Column<byte>(type: "tinyint", nullable: false),
                    ExecPlan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    KpiFieldId = table.Column<int>(type: "int", nullable: true),
                    UserInfoNameId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestDatas_CompanyNames_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyNames",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuggestDatas_KpiFields_KpiFieldId",
                        column: x => x.KpiFieldId,
                        principalTable: "KpiFields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuggestDatas_UserInfoNames_UserInfoNameId",
                        column: x => x.UserInfoNameId,
                        principalTable: "UserInfoNames",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_CompanyId",
                table: "SuggestDatas",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_KpiFieldId",
                table: "SuggestDatas",
                column: "KpiFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_UserInfoNameId",
                table: "SuggestDatas",
                column: "UserInfoNameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "KpiReports");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "KpiDatas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
