using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.Settings
{
    public partial class ResetSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM GlobalSettings
                    WHERE SettingsName = 'JobSettings'
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
