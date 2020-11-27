using System;
using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.Sqlite
{
    public sealed class OmbiSqliteContext : OmbiContext
    {
        private static bool _created;
        public OmbiSqliteContext(DbContextOptions<OmbiSqliteContext> options) : base(options)
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
                Database.ExecuteSqlRaw(@"INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId,ProductVersion)
                VALUES('20191102235658_Inital', '2.2.6-servicing-10079'); ");
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}