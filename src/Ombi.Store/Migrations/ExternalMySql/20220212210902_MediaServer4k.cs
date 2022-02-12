using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.ExternalMySql
{
    public partial class MediaServer4k : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Has4K",
                table: "PlexServerContent",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Has4K",
                table: "JellyfinContent",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Has4K",
                table: "EmbyContent",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Has4K",
                table: "PlexServerContent");

            migrationBuilder.DropColumn(
                name: "Has4K",
                table: "JellyfinContent");

            migrationBuilder.DropColumn(
                name: "Has4K",
                table: "EmbyContent");
        }
    }
}
