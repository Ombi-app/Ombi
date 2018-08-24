using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class LidarrSyncJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrAlbumCache", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LidarrAlbumCache_LidarrArtistCache_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "LidarrArtistCache",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LidarrAlbumCache_ArtistId",
                table: "LidarrAlbumCache",
                column: "ArtistId");
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
