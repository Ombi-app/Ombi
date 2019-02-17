using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class TvRequestsTotalSeasons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSeasons",
                table: "TvRequests",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSeasons",
                table: "TvRequests");
        }
    }
}
