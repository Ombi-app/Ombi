using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.ExternalMsSql
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CouchPotatoCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheMovieDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouchPotatoCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmbyContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmbyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImdbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TvDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Has4K = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbyContent", x => x.Id);
                    table.UniqueConstraint("AK_EmbyContent_EmbyId", x => x.EmbyId);
                });

            migrationBuilder.CreateTable(
                name: "JellyfinContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JellyfinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImdbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TvDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Has4K = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JellyfinContent", x => x.Id);
                    table.UniqueConstraint("AK_JellyfinContent_JellyfinId", x => x.JellyfinId);
                });

            migrationBuilder.CreateTable(
                name: "LidarrAlbumCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    ForeignAlbumId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackCount = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monitored = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PercentOfTracks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrAlbumCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LidarrArtistCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    ArtistName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForeignArtistId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monitored = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrArtistCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexServerContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReleaseYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImdbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TvDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Has4K = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerContent", x => x.Id);
                    table.UniqueConstraint("AK_PlexServerContent_Key", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "PlexWatchlistHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmdbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexWatchlistHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RadarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheMovieDbId = table.Column<int>(type: "int", nullable: false),
                    HasFile = table.Column<bool>(type: "bit", nullable: false),
                    Has4K = table.Column<bool>(type: "bit", nullable: false),
                    HasRegular = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickRageCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TvDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickRageCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickRageEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    TvDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickRageEpisodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TvDbId = table.Column<int>(type: "int", nullable: false),
                    TheMovieDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarrEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    TvDbId = table.Column<int>(type: "int", nullable: false),
                    MovieDbId = table.Column<int>(type: "int", nullable: false),
                    HasFile = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrEpisodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlayedEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheMovieDbId = table.Column<int>(type: "int", nullable: false),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayedEpisode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlayedMovie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheMovieDbId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayedMovie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmbyEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmbyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TvDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImdbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbyEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmbyEpisode_EmbyContent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "EmbyContent",
                        principalColumn: "EmbyId");
                });

            migrationBuilder.CreateTable(
                name: "JellyfinEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JellyfinId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TvDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImdbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JellyfinEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JellyfinEpisode_JellyfinContent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "JellyfinContent",
                        principalColumn: "JellyfinId");
                });

            migrationBuilder.CreateTable(
                name: "PlexEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrandparentKey = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexEpisode_PlexServerContent_GrandparentKey",
                        column: x => x.GrandparentKey,
                        principalTable: "PlexServerContent",
                        principalColumn: "Key");
                });

            migrationBuilder.CreateTable(
                name: "PlexSeasonsContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlexContentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    SeasonKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlexServerContentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexSeasonsContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexSeasonsContent_PlexServerContent_PlexServerContentId",
                        column: x => x.PlexServerContentId,
                        principalTable: "PlexServerContent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmbyEpisode_ParentId",
                table: "EmbyEpisode",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinEpisode_ParentId",
                table: "JellyfinEpisode",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouchPotatoCache");

            migrationBuilder.DropTable(
                name: "EmbyEpisode");

            migrationBuilder.DropTable(
                name: "JellyfinEpisode");

            migrationBuilder.DropTable(
                name: "LidarrAlbumCache");

            migrationBuilder.DropTable(
                name: "LidarrArtistCache");

            migrationBuilder.DropTable(
                name: "PlexEpisode");

            migrationBuilder.DropTable(
                name: "PlexSeasonsContent");

            migrationBuilder.DropTable(
                name: "PlexWatchlistHistory");

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
                name: "UserPlayedEpisode");

            migrationBuilder.DropTable(
                name: "UserPlayedMovie");

            migrationBuilder.DropTable(
                name: "EmbyContent");

            migrationBuilder.DropTable(
                name: "JellyfinContent");

            migrationBuilder.DropTable(
                name: "PlexServerContent");
        }
    }
}
