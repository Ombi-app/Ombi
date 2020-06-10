using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class UserProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "AspNetUsers");
        }
    }
}
