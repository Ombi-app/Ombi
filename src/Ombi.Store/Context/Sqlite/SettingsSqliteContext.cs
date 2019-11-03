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
            Database.SetCommandTimeout(60);
            Database.Migrate();
        }
    }
}