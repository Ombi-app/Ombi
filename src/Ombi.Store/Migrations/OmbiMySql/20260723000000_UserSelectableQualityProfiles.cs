using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ombi.Store.Context.MySql;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    [DbContext(typeof(OmbiMySqlContext))]
    [Migration("20260723000000_UserSelectableQualityProfiles")]
    public partial class UserSelectableQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) SELECT '1d86b5f2-5c17-4d42-b69b-2575ca769ef8', 'SelectSonarrQualityProfile', 'SELECTSONARRQUALITYPROFILE', 'b557b666-6665-476e-8f4c-e320515af27a' WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'SELECTSONARRQUALITYPROFILE');");
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
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("UserSelectableQualityProfile");
    }
}
