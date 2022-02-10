using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class Radarr4kRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertRole(OmbiRoles.Request4KMovie);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
