using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;
using Ombi.Store.Context.MySql;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    [DbContext(typeof(OmbiMySqlContext))]
    [Migration("20260722000000_SelectRadarrQualityProfileRole")]
    public class SelectRadarrQualityProfileRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRoleMySql(OmbiRoles.SelectRadarrQualityProfile);
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
