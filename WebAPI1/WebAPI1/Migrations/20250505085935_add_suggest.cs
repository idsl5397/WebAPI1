using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class add_suggest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuggestDatas_UserInfoNames_UserInfoNameId",
                table: "SuggestDatas");

            migrationBuilder.DropIndex(
                name: "IX_SuggestDatas_UserInfoNameId",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "ReviewMethod",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "SuggestType",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "UserInfoNameId",
                table: "SuggestDatas");

            migrationBuilder.AddColumn<int>(
                name: "SuggestEventTypeId",
                table: "SuggestDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuggestionTypeId",
                table: "SuggestDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SuggestDatas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SuggestEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuggestionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_SuggestEventTypeId",
                table: "SuggestDatas",
                column: "SuggestEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_SuggestionTypeId",
                table: "SuggestDatas",
                column: "SuggestionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_UserId",
                table: "SuggestDatas",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestDatas_SuggestEventTypes_SuggestEventTypeId",
                table: "SuggestDatas",
                column: "SuggestEventTypeId",
                principalTable: "SuggestEventTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestDatas_SuggestionTypes_SuggestionTypeId",
                table: "SuggestDatas",
                column: "SuggestionTypeId",
                principalTable: "SuggestionTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestDatas_Users_UserId",
                table: "SuggestDatas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuggestDatas_SuggestEventTypes_SuggestEventTypeId",
                table: "SuggestDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_SuggestDatas_SuggestionTypes_SuggestionTypeId",
                table: "SuggestDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_SuggestDatas_Users_UserId",
                table: "SuggestDatas");

            migrationBuilder.DropTable(
                name: "SuggestEventTypes");

            migrationBuilder.DropTable(
                name: "SuggestionTypes");

            migrationBuilder.DropIndex(
                name: "IX_SuggestDatas_SuggestEventTypeId",
                table: "SuggestDatas");

            migrationBuilder.DropIndex(
                name: "IX_SuggestDatas_SuggestionTypeId",
                table: "SuggestDatas");

            migrationBuilder.DropIndex(
                name: "IX_SuggestDatas_UserId",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "SuggestEventTypeId",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "SuggestionTypeId",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SuggestDatas");

            migrationBuilder.AddColumn<byte>(
                name: "ReviewMethod",
                table: "SuggestDatas",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SuggestType",
                table: "SuggestDatas",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "UserInfoNameId",
                table: "SuggestDatas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuggestDatas_UserInfoNameId",
                table: "SuggestDatas",
                column: "UserInfoNameId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuggestDatas_UserInfoNames_UserInfoNameId",
                table: "SuggestDatas",
                column: "UserInfoNameId",
                principalTable: "UserInfoNames",
                principalColumn: "Id");
        }
    }
}
