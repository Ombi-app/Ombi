using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class RemoveEmbyConnectionid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "EmbyConnectUserId",
            //    table: "AspNetUsers");

            migrationBuilder.CreateTable(
               name: "AspNetUsers_New",
               columns: table => new
               {
                   Id = table.Column<string>(nullable: false),
                   UserName = table.Column<string>(maxLength: 256, nullable: true),
                   NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                   Email = table.Column<string>(maxLength: 256, nullable: true),
                   NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                   EmailConfirmed = table.Column<bool>(nullable: false),
                   PhoneNumber = table.Column<string>(nullable: true),
                   PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                   TwoFactorEnabled = table.Column<bool>(nullable: false),
                   LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                   LockoutEnabled = table.Column<bool>(nullable: false),
                   AccessFailedCount = table.Column<int>(nullable: false),
                   Alias = table.Column<string>(nullable: true),
                   UserType = table.Column<int>(nullable: false),
                   ProviderUserId = table.Column<string>(nullable: true),
                   LastLoggedIn = table.Column<DateTime>(nullable: true),
                   MovieRequestLimit = table.Column<int>(nullable: true),
                   EpisodeRequestLimit = table.Column<int>(nullable: true),
                   MusicRequestLimit = table.Column<int>(nullable: true),
                   UserAccessToken = table.Column<string>(nullable: true),
                   PasswordHash = table.Column<string>(nullable: true),
                   SecurityStamp = table.Column<string>(nullable: true),
                   ConcurrencyStamp = table.Column<string>(nullable: true),
                   Language = table.Column<string>(nullable: true)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_AspNetUsers", x => x.Id);
               });

            migrationBuilder.Sql("UPDATE AspNetUsers SET UserType = 4 WHERE EmbyConnectUserId IS NOT NULL");
            migrationBuilder.Sql(@"INSERT INTO AspNetUsers_New SELECT `Id`,`UserName`,`NormalizedUserName`,`Email`,`NormalizedEmail`,`EmailConfirmed`,`PhoneNumber`,
            `PhoneNumberConfirmed`,`TwoFactorEnabled`,`LockoutEnd`,`LockoutEnabled`,`AccessFailedCount`,`Alias`,`UserType`,`ProviderUserId`,`LastLoggedIn`,`MovieRequestLimit`,
            `EpisodeRequestLimit`,`MusicRequestLimit`,`UserAccessToken`,`PasswordHash`,`SecurityStamp`,`ConcurrencyStamp`,`Language` FROM AspNetUsers;");
            migrationBuilder.Sql("PRAGMA foreign_keys=\"0\"", true);
            migrationBuilder.Sql("DROP TABLE AspNetUsers", true);
            migrationBuilder.Sql("ALTER TABLE AspNetUsers_New RENAME TO AspNetUsers", true);
            migrationBuilder.Sql("PRAGMA foreign_keys=\"1\"", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmbyConnectUserId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }
    }
}
