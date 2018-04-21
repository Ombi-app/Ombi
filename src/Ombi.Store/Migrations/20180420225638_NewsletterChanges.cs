using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class NewsletterChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpisodeNumber",
                table: "RecentlyAddedLog",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonNumber",
                table: "RecentlyAddedLog",
                nullable: true);

            migrationBuilder.Sql("DELETE FROM RecentlyAddedLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeNumber",
                table: "RecentlyAddedLog");

            migrationBuilder.DropColumn(
                name: "SeasonNumber",
                table: "RecentlyAddedLog");
        }
    }
}
