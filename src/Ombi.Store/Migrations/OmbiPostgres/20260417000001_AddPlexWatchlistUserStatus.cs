using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ombi.Store.Migrations.OmbiPostgres
{
    public partial class AddPlexWatchlistUserStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlexWatchlistUserStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexWatchlistUserStatus", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexWatchlistUserStatus_UserId",
                table: "PlexWatchlistUserStatus",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexWatchlistUserStatus");
        }
    }
}
