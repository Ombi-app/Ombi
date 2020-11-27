using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class MobileDevices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmbyEpisode");

            migrationBuilder.DropTable(
                name: "PlexEpisode");

            migrationBuilder.DropTable(
                name: "PlexSeasonsContent");

            migrationBuilder.DropTable(
                name: "EmbyContent");

            migrationBuilder.DropTable(
                name: "PlexServerContent");

           

            migrationBuilder.CreateTable(
                name: "MobileDevices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    AddedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileDevices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileDevices_UserId",
                table: "MobileDevices",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileDevices");

       

            migrationBuilder.CreateTable(
                name: "EmbyContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    EmbyId = table.Column<string>(nullable: false),
                    ImdbId = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    TheMovieDbId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TvDbId = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbyContent", x => x.Id);
                    table.UniqueConstraint("AK_EmbyContent_EmbyId", x => x.EmbyId);
                });

            migrationBuilder.CreateTable(
                name: "PlexServerContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    ImdbId = table.Column<string>(nullable: true),
                    Key = table.Column<int>(nullable: false),
                    Quality = table.Column<string>(nullable: true),
                    ReleaseYear = table.Column<string>(nullable: true),
                    RequestId = table.Column<int>(nullable: true),
                    TheMovieDbId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TvDbId = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerContent", x => x.Id);
                    table.UniqueConstraint("AK_PlexServerContent_Key", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "EmbyEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    EmbyId = table.Column<string>(nullable: true),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    ImdbId = table.Column<string>(nullable: true),
                    ParentId = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    SeasonNumber = table.Column<int>(nullable: false),
                    TheMovieDbId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TvDbId = table.Column<string>(nullable: true)
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
                    GrandparentKey = table.Column<int>(nullable: false),
                    Key = table.Column<int>(nullable: false),
                    ParentKey = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
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
                    ParentKey = table.Column<int>(nullable: false),
                    PlexContentId = table.Column<int>(nullable: false),
                    PlexServerContentId = table.Column<int>(nullable: true),
                    SeasonKey = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false)
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
    }
}
