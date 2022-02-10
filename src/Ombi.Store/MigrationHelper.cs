using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Ombi.Store
{
    internal static class MigrationHelper
    {
        public static void InsertRole(this MigrationBuilder mb, string role)
        {
            mb.Sql($@"
INSERT INTO AspnetRoles(Id, ConcurrencyStamp, Name, NormalizedName) 
SELECT '{Guid.NewGuid()}','{Guid.NewGuid()}','{role}', '{role.ToUpper()}' 
WHERE NOT EXISTS(SELECT 1 FROM AspnetRoles WHERE Name = '{role}');");
        }
    }
}
