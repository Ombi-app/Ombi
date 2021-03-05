using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class TvRequestProviderId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalProviderId",
                table: "TvRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "SeasonRequests",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalProviderId",
                table: "TvRequests");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "SeasonRequests");
        }
    }
}
