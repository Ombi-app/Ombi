using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ombi.Store.Context.Postgres;
using Ombi.Store;

#nullable disable

namespace Ombi.Store.Migrations.OmbiPostgres
{
    [DbContext(typeof(OmbiPostgresContext))]
    [Migration("20260723000000_UserSelectableQualityProfiles")]
    public partial class UserSelectableQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUserSelectableQualityProfilesPostgres();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RemoveUserSelectableQualityProfiles();
        }
    }
}
