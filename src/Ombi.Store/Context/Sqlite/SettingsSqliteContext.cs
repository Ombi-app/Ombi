using System;
using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.Sqlite
{
    public sealed class SettingsSqliteContext : SettingsContext
    {
        private static bool _created;
        public SettingsSqliteContext(DbContextOptions<SettingsSqliteContext> options) : base(options)
        {
            if (_created) return;

            _created = true;
            Upgrade();
            Database.SetCommandTimeout(60);
            Database.Migrate();
        }

        private void Upgrade()
        {
            try
            {
                Database.ExecuteSqlCommand(@"INSERT INTO __EFMigrationsHistory (MigrationId,ProductVersion)
                VALUES('20191103205204_Inital', '2.2.6-servicing-10079'); ");
            }
            catch (Exception) 
            {
                // ignored
            }
        }
    }
}