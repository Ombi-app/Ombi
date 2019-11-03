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
            Database.SetCommandTimeout(60);
            Database.Migrate();
        }
    }
}