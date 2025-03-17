using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class change_user_auth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "authority",
                table: "UserInfoNames");

            migrationBuilder.AddColumn<byte>(
                name: "Auth",
                table: "UserInfoNames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Auth",
                table: "UserInfoNames");

            migrationBuilder.AddColumn<string>(
                name: "authority",
                table: "UserInfoNames",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
