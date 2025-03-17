using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class change_user_mail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nickname",
                table: "UserInfoNames");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "UserInfoNames",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "UserInfoNames",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "nickname",
                table: "UserInfoNames",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");
        }
    }
}
