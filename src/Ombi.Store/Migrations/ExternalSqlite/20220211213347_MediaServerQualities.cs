using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.ExternalSqlite
{
    public partial class MediaServerQualities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Has4K",
                table: "RadarrCache",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRegular",
                table: "RadarrCache",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Quality",
                table: "JellyfinContent",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Quality",
                table: "EmbyContent",
                type: "TEXT",
                nullable: true);
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
