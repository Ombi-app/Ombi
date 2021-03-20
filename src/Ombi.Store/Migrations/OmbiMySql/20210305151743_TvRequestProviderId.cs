using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class TvRequestProviderId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalProviderId",
                table: "TvRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "SeasonRequests",
                type: "longtext",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StreamingCountry",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalProviderId",
                table: "TvRequests");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "SeasonRequests");

            migrationBuilder.AlterColumn<string>(
                name: "StreamingCountry",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext");
        }
    }
}
