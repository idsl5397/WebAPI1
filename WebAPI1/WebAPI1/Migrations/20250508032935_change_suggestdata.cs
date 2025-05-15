using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class change_suggestdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthAndDay",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "SuggestDatas");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "SuggestDatas");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "SuggestDatas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "SuggestDatas");

            migrationBuilder.AddColumn<string>(
                name: "MonthAndDay",
                table: "SuggestDatas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "SuggestDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "SuggestDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
