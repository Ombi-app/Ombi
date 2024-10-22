using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.MsSql;

public sealed class OmbiMsSqlContext : OmbiContext
{
    private static bool _created;
    
    public OmbiMsSqlContext(DbContextOptions<OmbiMsSqlContext> options) : base(options)
    {
        if (_created) return;
        _created = true;
        
        Database.Migrate();
    }
}