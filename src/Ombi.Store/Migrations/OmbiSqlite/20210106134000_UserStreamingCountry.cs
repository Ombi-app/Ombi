using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class UserStreamingCountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StreamingCountry",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "US");

            migrationBuilder.Sql("UPDATE AspNetUsers SET StreamingCountry = 'US'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreamingCountry",
                table: "AspNetUsers");
        }
    }
}
