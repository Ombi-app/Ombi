using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class Radarr4kUserQualityProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Radarr4KQualityProfile",
                table: "UserQualityProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Radarr4KRootPath",
                table: "UserQualityProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Radarr4KQualityProfile",
                table: "UserQualityProfiles");

            migrationBuilder.DropColumn(
                name: "Radarr4KRootPath",
                table: "UserQualityProfiles");
        }
    }
}
