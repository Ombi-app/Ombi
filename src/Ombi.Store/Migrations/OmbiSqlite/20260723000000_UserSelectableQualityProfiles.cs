using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Store.Context.Sqlite;

#nullable disable

namespace Ombi.Store.Migrations.OmbiSqlite
{
    [DbContext(typeof(OmbiSqliteContext))]
    [Migration("20260723000000_UserSelectableQualityProfiles")]
    public partial class UserSelectableQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRole(OmbiRoles.SelectSonarrQualityProfile);
            migrationBuilder.CreateTable(
                name: "UserSelectableQualityProfile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Application = table.Column<int>(type: "INTEGER", nullable: false),
                    QualityProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Is4K = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSelectableQualityProfile", x => x.Id);
                    table.ForeignKey("FK_UserSelectableQualityProfile_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex("IX_UserSelectableQualityProfile_UserId_Application_QualityProfileId_Is4K", "UserSelectableQualityProfile", new[] { "UserId", "Application", "QualityProfileId", "Is4K" }, unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("UserSelectableQualityProfile");
    }
}
