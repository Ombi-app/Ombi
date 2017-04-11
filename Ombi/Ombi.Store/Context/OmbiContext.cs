using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public class OmbiContext : DbContext, IOmbiContext
    {
        private static bool _created;
        public OmbiContext()
        {
            if (_created) return;

            _created = true;
            Database.EnsureCreated();
            Database.Migrate();
        }
        public DbSet<RequestBlobs> Requests { get; set; }
        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Ombi.db");
        }
    }
}