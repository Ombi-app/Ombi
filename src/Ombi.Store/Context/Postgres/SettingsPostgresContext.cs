using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.Postgres
{
    public sealed class SettingsPostgresContext : SettingsContext
    {
        private static bool _created;
        public SettingsPostgresContext(DbContextOptions<SettingsPostgresContext> options) : base(options)
        {
            if (_created) return;

            _created = true;
            Database.Migrate();
        }
    }
}