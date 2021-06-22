using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.ExternalMySql
{
    public partial class SonarrSyncMovieDbData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MovieDbId",
                table: "SonarrEpisodeCache",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TheMovieDbId",
                table: "SonarrCache",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovieDbId",
                table: "SonarrEpisodeCache");

            migrationBuilder.DropColumn(
                name: "TheMovieDbId",
                table: "SonarrCache");
        }
    }
}
