using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
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
    }
}