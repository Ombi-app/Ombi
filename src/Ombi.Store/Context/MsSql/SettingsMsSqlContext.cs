using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.MsSql;

public sealed class SettingsMsSqlContext : SettingsContext
{
    private static bool _created;
    
    public SettingsMsSqlContext(DbContextOptions<SettingsMsSqlContext> options) : base(options)
    {
        if (_created) return;
        
        _created = true;
        Database.Migrate();
    }
}