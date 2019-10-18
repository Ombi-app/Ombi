using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.External
{
    public partial class PlexContentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "PlexServerContent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "PlexServerContent");
        }
    }
}
