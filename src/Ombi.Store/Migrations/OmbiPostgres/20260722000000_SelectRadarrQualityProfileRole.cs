using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;
using Ombi.Store.Context.Postgres;

#nullable disable

namespace Ombi.Store.Migrations.OmbiPostgres
{
    [DbContext(typeof(OmbiPostgresContext))]
    [Migration("20260722000000_SelectRadarrQualityProfileRole")]
    public class SelectRadarrQualityProfileRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRolePostgres(OmbiRoles.SelectRadarrQualityProfile);
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
