using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class LidarrSyncJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LidarrAlbumCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArtistId = table.Column<int>(nullable: false),
                    ForeignAlbumId = table.Column<string>(nullable: true),
                    TrackCount = table.Column<int>(nullable: false),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    Monitored = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    PercentOfTracks = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrAlbumCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LidarrArtistCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArtistId = table.Column<int>(nullable: false),
                    ArtistName = table.Column<string>(nullable: true),
                    ForeignArtistId = table.Column<string>(nullable: true),
                    Monitored = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrArtistCache", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LidarrAlbumCache");

            migrationBuilder.DropTable(
                name: "LidarrArtistCache");
        }
    }
}
