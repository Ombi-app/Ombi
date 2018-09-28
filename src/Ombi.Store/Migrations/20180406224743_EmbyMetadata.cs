using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class EmbyMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImdbId",
                table: "EmbyEpisode",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TheMovieDbId",
                table: "EmbyEpisode",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TvDbId",
                table: "EmbyEpisode",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImdbId",
                table: "EmbyContent",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TheMovieDbId",
                table: "EmbyContent",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TvDbId",
                table: "EmbyContent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImdbId",
                table: "EmbyEpisode");

            migrationBuilder.DropColumn(
                name: "TheMovieDbId",
                table: "EmbyEpisode");

            migrationBuilder.DropColumn(
                name: "TvDbId",
                table: "EmbyEpisode");

            migrationBuilder.DropColumn(
                name: "ImdbId",
                table: "EmbyContent");

            migrationBuilder.DropColumn(
                name: "TheMovieDbId",
                table: "EmbyContent");

            migrationBuilder.DropColumn(
                name: "TvDbId",
                table: "EmbyContent");
        }
    }
}
