using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class UserRequestLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpisodeRequestLimitAmount",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EpisodeRequestLimitType",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovieRequestLimitAmount",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovieRequestLimitType",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MusicRequestLimitAmount",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MusicRequestLimitType",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeRequestLimitAmount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EpisodeRequestLimitType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MovieRequestLimitAmount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MovieRequestLimitType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MusicRequestLimitAmount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MusicRequestLimitType",
                table: "AspNetUsers");
        }
    }
}
