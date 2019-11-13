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
            try
            {
                Database.Migrate();
            }
            catch (SqliteException e) when (e.Message.Equals("duplicate column name: RequestId"))
            {
            }
        }


        private void Upgrade()
        {
            try
            {
                Database.ExecuteSqlCommand(@"INSERT INTO __EFMigrationsHistory (MigrationId,ProductVersion)
                VALUES('20191103205133_Inital', '2.2.6-servicing-10079'); ");
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}