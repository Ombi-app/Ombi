using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.ExternalSqlite
{
    /// <inheritdoc />
    public partial class PlexWatchlistHistoryLastSeen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "PlexWatchlistHistory",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "PlexWatchlistHistory");
        }
    }
}
