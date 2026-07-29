using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ombi.Store.Context.Postgres;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ombi.Helpers;

#nullable disable

namespace Ombi.Store.Migrations.OmbiPostgres
{
    [DbContext(typeof(OmbiPostgresContext))]
    [Migration("20260723000000_UserSelectableQualityProfiles")]
    public partial class UserSelectableQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRolePostgres(OmbiRoles.SelectRadarrQualityProfile);
            migrationBuilder.InsertRolePostgres(OmbiRoles.SelectSonarrQualityProfile);
            migrationBuilder.CreateTable(
                name: "UserSelectableQualityProfile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Application = table.Column<int>(type: "integer", nullable: false),
                    QualityProfileId = table.Column<int>(type: "integer", nullable: false),
                    Is4K = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSelectableQualityProfile", x => x.Id);
                    table.ForeignKey("FK_UserSelectableQualityProfile_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex("IX_UserSelectableQualityProfile_UserId_Application_QualityProfileId_Is4K", "UserSelectableQualityProfile", new[] { "UserId", "Application", "QualityProfileId", "Is4K" }, unique: true);
            migrationBuilder.AddColumn<int>(
                name: "QualityOverride4K",
                table: "MovieRequests",
                type: "integer",
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
