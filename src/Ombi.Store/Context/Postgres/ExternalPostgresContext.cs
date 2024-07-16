using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.Postgres
{
    public sealed class ExternalPostgresContext : ExternalContext
    {
        private static bool _created;
        public ExternalPostgresContext(DbContextOptions<ExternalPostgresContext> options) : base(options)
        {
            if (_created) return;

            _created = true;
            Database.Migrate();
        }
    }
}