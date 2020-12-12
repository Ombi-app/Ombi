using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.ExternalMySql
{
    public partial class Jellyfin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JellyfinContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    JellyfinId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    ImdbId = table.Column<string>(nullable: true),
                    TheMovieDbId = table.Column<string>(nullable: true),
                    TvDbId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JellyfinContent", x => x.Id);
                    table.UniqueConstraint("AK_JellyfinContent_JellyfinId", x => x.JellyfinId);
                });

            migrationBuilder.CreateTable(
                name: "JellyfinEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    JellyfinId = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_JellyfinEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JellyfinEpisode_JellyfinContent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "JellyfinContent",
                        principalColumn: "JellyfinId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinEpisode_ParentId",
                table: "JellyfinEpisode",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JellyfinContent");

            migrationBuilder.DropTable(
                name: "JellyfinEpisode");
        }
    }
}
