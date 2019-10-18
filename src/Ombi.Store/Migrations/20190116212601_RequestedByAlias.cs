using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class RequestedByAlias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestedByAlias",
                table: "MovieRequests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedByAlias",
                table: "ChildRequests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedByAlias",
                table: "AlbumRequests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedByAlias",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "RequestedByAlias",
                table: "ChildRequests");

            migrationBuilder.DropColumn(
                name: "RequestedByAlias",
                table: "AlbumRequests");
        }
    }
}
