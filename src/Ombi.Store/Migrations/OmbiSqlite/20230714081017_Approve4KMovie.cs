using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;

#nullable disable

namespace Ombi.Store.Migrations.OmbiSqlite
{
    public partial class Approve4KMovie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRole(OmbiRoles.AutoApprove4KMovie);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
