using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI1.Migrations
{
    /// <inheritdoc />
    public partial class userinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserInfoNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(25)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nickname = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    EnterpriseId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    FactoryId = table.Column<int>(type: "int", nullable: true),
                    authority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    anyllmworkspace = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    avatar = table.Column<string>(type: "varchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "varchar(40)", nullable: true),
                    email = table.Column<string>(type: "varchar(max)", nullable: true),
                    Salt = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfoNames", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInfoNames");
        }
    }
}
