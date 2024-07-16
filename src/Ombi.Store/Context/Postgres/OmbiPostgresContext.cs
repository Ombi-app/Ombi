using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.Postgres
{
    public sealed class OmbiPostgresContext : OmbiContext
    {
        private static bool _created;
        
        public OmbiPostgresContext(DbContextOptions<OmbiPostgresContext> options) : base(options)
        {
            if (_created) return;
            _created = true;

            Database.Migrate();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}