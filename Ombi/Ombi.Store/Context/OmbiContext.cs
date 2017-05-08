using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            // Run Script

            Database.ExecuteSqlCommand(Sql.SqlTables, 0);
        }

        public DbSet<RequestBlobs> Requests { get; set; }
        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PlexContent> PlexContent { get; set; }


        public EntityEntry<GlobalSettings> Entry(GlobalSettings settings)
        {
            return Entry(settings);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Ombi.db");
        }
    }
}