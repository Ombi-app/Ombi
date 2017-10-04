using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class SonarrEpisodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SonarrEpisodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TvDbId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarrEpisodeCache", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SonarrEpisodeCache");
        }
    }
}
