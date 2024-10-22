using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.MsSql;

public sealed class ExternalMsSqlContext : ExternalContext
{
    private static bool _created;
    
    public ExternalMsSqlContext(DbContextOptions<ExternalMsSqlContext> options) : base(options)
    {
        if (_created) return;
        
        _created = true;
        Database.Migrate();
    }
}