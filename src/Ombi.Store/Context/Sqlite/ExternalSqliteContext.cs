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
            Database.SetCommandTimeout(60);
            Database.Migrate();
        }
    }
}