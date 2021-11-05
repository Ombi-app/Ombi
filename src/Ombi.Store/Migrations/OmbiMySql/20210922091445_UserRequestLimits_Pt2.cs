using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class UserRequestLimits_Pt2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeRequestLimitAmount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MovieRequestLimitAmount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MusicRequestLimitAmount",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpisodeRequestLimitAmount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovieRequestLimitAmount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MusicRequestLimitAmount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }
    }
}
