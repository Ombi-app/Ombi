using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ombi.Store.Migrations.ExternalPostgres
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CouchPotatoCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TheMovieDbId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouchPotatoCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmbyContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderId = table.Column<string>(type: "text", nullable: true),
                    EmbyId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ImdbId = table.Column<string>(type: "text", nullable: true),
                    TvDbId = table.Column<string>(type: "text", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "text", nullable: true),
                    Has4K = table.Column<bool>(type: "boolean", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderId = table.Column<string>(type: "text", nullable: true),
                    JellyfinId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ImdbId = table.Column<string>(type: "text", nullable: true),
                    TvDbId = table.Column<string>(type: "text", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "text", nullable: true),
                    Has4K = table.Column<bool>(type: "boolean", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    ForeignAlbumId = table.Column<string>(type: "text", nullable: true),
                    TrackCount = table.Column<int>(type: "integer", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Monitored = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    PercentOfTracks = table.Column<decimal>(type: "numeric", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrAlbumCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LidarrArtistCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    ArtistName = table.Column<string>(type: "text", nullable: true),
                    ForeignArtistId = table.Column<string>(type: "text", nullable: true),
                    Monitored = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LidarrArtistCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexServerContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReleaseYear = table.Column<string>(type: "text", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: false),
                    RequestId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ImdbId = table.Column<string>(type: "text", nullable: true),
                    TvDbId = table.Column<string>(type: "text", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "text", nullable: true),
                    Has4K = table.Column<bool>(type: "boolean", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TmdbId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexWatchlistHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RadarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TheMovieDbId = table.Column<int>(type: "integer", nullable: false),
                    HasFile = table.Column<bool>(type: "boolean", nullable: false),
                    Has4K = table.Column<bool>(type: "boolean", nullable: false),
                    HasRegular = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickRageCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TvDbId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickRageCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickRageEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    TvDbId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickRageEpisodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TvDbId = table.Column<int>(type: "integer", nullable: false),
                    TheMovieDbId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarrEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    TvDbId = table.Column<int>(type: "integer", nullable: false),
                    MovieDbId = table.Column<int>(type: "integer", nullable: false),
                    HasFile = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrEpisodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlayedEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TheMovieDbId = table.Column<int>(type: "integer", nullable: false),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayedEpisode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlayedMovie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TheMovieDbId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayedMovie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmbyEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmbyId = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<string>(type: "text", nullable: true),
                    ProviderId = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TvDbId = table.Column<string>(type: "text", nullable: true),
                    ImdbId = table.Column<string>(type: "text", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "text", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JellyfinId = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<string>(type: "text", nullable: true),
                    ProviderId = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TvDbId = table.Column<string>(type: "text", nullable: true),
                    ImdbId = table.Column<string>(type: "text", nullable: true),
                    TheMovieDbId = table.Column<string>(type: "text", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: true),
                    ParentKey = table.Column<string>(type: "text", nullable: true),
                    GrandparentKey = table.Column<string>(type: "text", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlexContentId = table.Column<string>(type: "text", nullable: true),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    SeasonKey = table.Column<string>(type: "text", nullable: true),
                    ParentKey = table.Column<string>(type: "text", nullable: true),
                    PlexServerContentId = table.Column<int>(type: "integer", nullable: true)
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
