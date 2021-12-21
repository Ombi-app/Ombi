using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class RenameAlbumRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("AlbumRequests", null, "MusicRequests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
