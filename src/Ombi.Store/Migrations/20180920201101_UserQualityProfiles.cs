using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class UserQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserQualityProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(nullable: true),
                    SonarrQualityProfileAnime = table.Column<int>(nullable: false),
                    SonarrRootPathAnime = table.Column<int>(nullable: false),
                    SonarrRootPath = table.Column<int>(nullable: false),
                    SonarrQualityProfile = table.Column<int>(nullable: false),
                    RadarrRootPath = table.Column<int>(nullable: false),
                    RadarrQualityProfile = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQualityProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQualityProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQualityProfiles_UserId",
                table: "UserQualityProfiles",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQualityProfiles");
        }
    }
}
