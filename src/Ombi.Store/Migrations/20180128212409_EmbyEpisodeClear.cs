using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class EmbyEpisodeClear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM EmbyEpisode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
