using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.ExternalSqlite
{
    public partial class RequestIdPatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            //migrationBuilder.AddColumn<int>(
            //    name: "RequestId",
            //    table: "PlexServerContent",
            //    nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
