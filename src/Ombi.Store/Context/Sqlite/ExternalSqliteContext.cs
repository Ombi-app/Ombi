using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.Sqlite
{
    public sealed class ExternalSqliteContext : ExternalContext
    {
        private static bool _created;
        public ExternalSqliteContext(DbContextOptions<ExternalSqliteContext> options) : base(options)
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
                VALUES('20191103205133_Inital', '2.2.6-servicing-10079'); ");
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                // ignored
            }
        }
    }
}