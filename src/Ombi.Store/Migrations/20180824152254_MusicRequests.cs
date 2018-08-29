using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class MusicRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MusicRequestLimit",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlbumRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    MarkedAsApproved = table.Column<DateTime>(nullable: false),
                    RequestedDate = table.Column<DateTime>(nullable: false),
                    Available = table.Column<bool>(nullable: false),
                    MarkedAsAvailable = table.Column<DateTime>(nullable: true),
                    RequestedUserId = table.Column<string>(nullable: true),
                    Denied = table.Column<bool>(nullable: true),
                    MarkedAsDenied = table.Column<DateTime>(nullable: false),
                    DeniedReason = table.Column<string>(nullable: true),
                    RequestType = table.Column<int>(nullable: false),
                    ForeignAlbumId = table.Column<string>(nullable: true),
                    ForeignArtistId = table.Column<string>(nullable: true),
                    Disk = table.Column<string>(nullable: true),
                    Cover = table.Column<string>(nullable: true),
                    Rating = table.Column<decimal>(nullable: false),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    ArtistName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumRequests_AspNetUsers_RequestedUserId",
                        column: x => x.RequestedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRequests_RequestedUserId",
                table: "AlbumRequests",
                column: "RequestedUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumRequests");

            migrationBuilder.DropColumn(
                name: "MusicRequestLimit",
                table: "AspNetUsers");
        }
    }
}
