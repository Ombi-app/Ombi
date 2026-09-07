using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
            migrationBuilder.AddUserSelectableQualityProfiles();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RemoveUserSelectableQualityProfiles();
        }
    }
}
