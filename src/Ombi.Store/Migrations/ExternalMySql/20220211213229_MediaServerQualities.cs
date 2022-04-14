using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.ExternalMySql
{
    public partial class MediaServerQualities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Has4K",
                table: "RadarrCache",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRegular",
                table: "RadarrCache",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Quality",
                table: "JellyfinContent",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Quality",
                table: "EmbyContent",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Has4K",
                table: "RadarrCache");

            migrationBuilder.DropColumn(
                name: "HasRegular",
                table: "RadarrCache");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "JellyfinContent");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "EmbyContent");
        }
    }
}
