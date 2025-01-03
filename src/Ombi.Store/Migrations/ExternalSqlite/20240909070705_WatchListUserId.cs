using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.ExternalSqlite
{
    /// <inheritdoc />
    public partial class WatchListUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "PlexWatchlistHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PlexWatchlistHistory",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "PlexWatchlistHistory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PlexWatchlistHistory");
        }
    }
}
