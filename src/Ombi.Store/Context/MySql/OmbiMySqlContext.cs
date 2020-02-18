using Microsoft.EntityFrameworkCore;

namespace Ombi.Store.Context.MySql
{
    public sealed class OmbiMySqlContext : OmbiContext
    {
        private static bool _created;
        
        public OmbiMySqlContext(DbContextOptions<OmbiMySqlContext> options) : base(options)
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