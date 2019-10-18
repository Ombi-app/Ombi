using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Helpers;

namespace Ombi.Store.Migrations
{
    public partial class Roles : Migration
    {
        protected override void Up(MigrationBuilder mb)
        {
            // Make sure we have the roles
            InsertRole(mb, OmbiRoles.ReceivesNewsletter);
            InsertRole(mb, OmbiRoles.RequestMusic);
            InsertRole(mb, OmbiRoles.AutoApproveMusic);
            InsertRole(mb, OmbiRoles.ManageOwnRequests);
            InsertRole(mb, OmbiRoles.EditCustomPage);
        }

        private void InsertRole(MigrationBuilder mb, string role)
        {
            mb.Sql($@"
INSERT INTO AspnetRoles(Id, ConcurrencyStamp, Name, NormalizedName) 
SELECT '{Guid.NewGuid().ToString()}','{Guid.NewGuid().ToString()}','{role}', '{role.ToUpper()}' 
WHERE NOT EXISTS(SELECT 1 FROM AspnetRoles WHERE Name = '{role}');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
