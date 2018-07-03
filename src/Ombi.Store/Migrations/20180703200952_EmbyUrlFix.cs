using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class EmbyUrlFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE EmbyContent SET Url = replace( Url, 'http://app.emby.media/itemdetails.html', 'http://app.emby.media/#!/itemdetails.html' ) WHERE Url LIKE 'http://app.emby.media/itemdetails.html%';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
