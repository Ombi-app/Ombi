using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ombi.Store.Context.MySql;
using Ombi.Store;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    [DbContext(typeof(OmbiMySqlContext))]
    [Migration("20260723000000_UserSelectableQualityProfiles")]
    public partial class UserSelectableQualityProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUserSelectableQualityProfilesMySql();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RemoveUserSelectableQualityProfilesMySql();
        }
    }
}
