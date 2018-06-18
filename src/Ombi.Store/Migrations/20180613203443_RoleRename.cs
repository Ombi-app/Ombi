using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class RoleRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE AspNetRoles
SET Name = 'ReceivesNewsletter',
NORMALIZEDNAME = 'RECEIVESNEWSLETTER'
where Name = 'RecievesNewsletter'
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
