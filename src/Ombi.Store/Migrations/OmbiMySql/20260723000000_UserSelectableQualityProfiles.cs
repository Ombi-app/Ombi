using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ombi.Store.Context.MySql;
using Ombi.Helpers;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    [DbContext(typeof(OmbiMySqlContext))]
    [Migration("20260723000000_UserSelectableQualityProfiles")]
    public partial class UserSelectableQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRoleMySql(OmbiRoles.SelectRadarrQualityProfile);
            migrationBuilder.InsertRoleMySql(OmbiRoles.SelectSonarrQualityProfile);
            migrationBuilder.CreateTable(
                name: "UserSelectableQualityProfile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("MySql:ValueGenerationStrategy", Microsoft.EntityFrameworkCore.Metadata.MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                    Application = table.Column<int>(type: "int", nullable: false),
                    QualityProfileId = table.Column<int>(type: "int", nullable: false),
                    Is4K = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSelectableQualityProfile", x => x.Id);
                    table.ForeignKey("FK_UserSelectableQualityProfile_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                }).Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateIndex("IX_UserSelectableQualityProfile_UserId_Application_QualityProfileId_Is4K", "UserSelectableQualityProfile", new[] { "UserId", "Application", "QualityProfileId", "Is4K" }, unique: true);
            migrationBuilder.AddColumn<int>(
                name: "QualityOverride4K",
                table: "MovieRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("UserSelectableQualityProfile");
            migrationBuilder.DropColumn("QualityOverride4K", "MovieRequests");
        }
    }
}
