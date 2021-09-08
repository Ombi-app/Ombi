using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class MusicRequestsMonitor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Monitored",
                table: "MusicRequests",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Monitor",
                table: "MusicRequests",
                type: "longtext",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SearchForMissingAlbums",
                table: "MusicRequests",
                type: "longtext",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Monitored",
                table: "MusicRequests");

            migrationBuilder.DropColumn(
                name: "Monitor",
                table: "MusicRequests");

            migrationBuilder.DropColumn(
                name: "SearchForMissingAlbums",
                table: "MusicRequests");
        }
    }
}
