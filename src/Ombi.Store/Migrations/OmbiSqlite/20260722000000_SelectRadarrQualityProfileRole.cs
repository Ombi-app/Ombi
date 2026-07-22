using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;
using Ombi.Store.Context.Sqlite;

#nullable disable

namespace Ombi.Store.Migrations.OmbiSqlite
{
    [DbContext(typeof(OmbiSqliteContext))]
    [Migration("20260722000000_SelectRadarrQualityProfileRole")]
    public class SelectRadarrQualityProfileRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRole(OmbiRoles.SelectRadarrQualityProfile);
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
