using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class Approve4KMovie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRoleMySql(OmbiRoles.AutoApprove4KMovie);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
