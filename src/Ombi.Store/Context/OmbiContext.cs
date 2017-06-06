using System.IO;
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

#if DEBUG
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);
            var file = File.ReadAllText(Path.Combine(directory,"SqlTables.sql"));
#else

            var file = File.ReadAllText("SqlTables.sql");
#endif
            // Run Script

            Database.ExecuteSqlCommand(file, 0);
        }

        public DbSet<RequestBlobs> Requests { get; set; }
        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PlexContent> PlexContent { get; set; }
        public DbSet<RadarrCache> RadarrCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Ombi.db");
        }
    }
}