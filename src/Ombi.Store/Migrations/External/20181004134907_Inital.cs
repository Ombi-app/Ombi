using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.External
{
    public partial class Inital : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CouchPotatoCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TheMovieDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouchPotatoCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmbyContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    EmbyId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    ImdbId = table.Column<string>(nullable: true),
                    TheMovieDbId = table.Column<string>(nullable: true),
                    TvDbId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbyContent", x => x.Id);
                    table.UniqueConstraint("AK_EmbyContent_EmbyId", x => x.EmbyId);
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
                    Title = table.Column<string>(nullable: true),
                    PercentOfTracks = table.Column<decimal>(nullable: false),
                    AddedAt = table.Column<DateTime>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PlexServerContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    ReleaseYear = table.Column<string>(nullable: true),
                    ImdbId = table.Column<string>(nullable: true),
                    TvDbId = table.Column<string>(nullable: true),
                    TheMovieDbId = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Key = table.Column<int>(nullable: false),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    Quality = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerContent", x => x.Id);
                    table.UniqueConstraint("AK_PlexServerContent_Key", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "RadarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TheMovieDbId = table.Column<int>(nullable: false),
                    HasFile = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickRageCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TvDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickRageCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickRageEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonNumber = table.Column<int>(nullable: false),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    TvDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickRageEpisodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TvDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarrEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonNumber = table.Column<int>(nullable: false),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    TvDbId = table.Column<int>(nullable: false),
                    HasFile = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrEpisodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmbyEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    EmbyId = table.Column<string>(nullable: true),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false),
                    ParentId = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    TvDbId = table.Column<string>(nullable: true),
                    ImdbId = table.Column<string>(nullable: true),
                    TheMovieDbId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbyEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmbyEpisode_EmbyContent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "EmbyContent",
                        principalColumn: "EmbyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false),
                    Key = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ParentKey = table.Column<int>(nullable: false),
                    GrandparentKey = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexEpisode_PlexServerContent_GrandparentKey",
                        column: x => x.GrandparentKey,
                        principalTable: "PlexServerContent",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexSeasonsContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexContentId = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false),
                    SeasonKey = table.Column<int>(nullable: false),
                    ParentKey = table.Column<int>(nullable: false),
                    PlexServerContentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexSeasonsContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexSeasonsContent_PlexServerContent_PlexServerContentId",
                        column: x => x.PlexServerContentId,
                        principalTable: "PlexServerContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmbyEpisode_ParentId",
                table: "EmbyEpisode",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexEpisode_GrandparentKey",
                table: "PlexEpisode",
                column: "GrandparentKey");

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonsContent_PlexServerContentId",
                table: "PlexSeasonsContent",
                column: "PlexServerContentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouchPotatoCache");

            migrationBuilder.DropTable(
                name: "EmbyEpisode");

            migrationBuilder.DropTable(
                name: "LidarrAlbumCache");

            migrationBuilder.DropTable(
                name: "LidarrArtistCache");

            migrationBuilder.DropTable(
                name: "PlexEpisode");

            migrationBuilder.DropTable(
                name: "PlexSeasonsContent");

            migrationBuilder.DropTable(
                name: "RadarrCache");

            migrationBuilder.DropTable(
                name: "SickRageCache");

            migrationBuilder.DropTable(
                name: "SickRageEpisodeCache");

            migrationBuilder.DropTable(
                name: "SonarrCache");

            migrationBuilder.DropTable(
                name: "SonarrEpisodeCache");

            migrationBuilder.DropTable(
                name: "EmbyContent");

            migrationBuilder.DropTable(
                name: "PlexServerContent");
        }
    }
}
