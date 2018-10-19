using System.IO;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public sealed class ExternalContext : DbContext, IExternalContext
    {
        private static bool _created;
        public ExternalContext()
        {
            if (_created) return;

            _created = true;
            Database.Migrate();
        }

        public DbSet<PlexServerContent> PlexServerContent { get; set; }
        public DbSet<PlexSeasonsContent> PlexSeasonsContent { get; set; }
        public DbSet<PlexEpisode> PlexEpisode { get; set; }
        public DbSet<RadarrCache> RadarrCache { get; set; }
        public DbSet<CouchPotatoCache> CouchPotatoCache { get; set; }
        public DbSet<EmbyContent> EmbyContent { get; set; }
        public DbSet<EmbyEpisode> EmbyEpisode { get; set; }
        
        public DbSet<SonarrCache> SonarrCache { get; set; }
        public DbSet<LidarrArtistCache> LidarrArtistCache { get; set; }
        public DbSet<LidarrAlbumCache> LidarrAlbumCache { get; set; }
        public DbSet<SonarrEpisodeCache> SonarrEpisodeCache { get; set; }
        public DbSet<SickRageCache> SickRageCache { get; set; }
        public DbSet<SickRageEpisodeCache> SickRageEpisodeCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(i.StoragePath, "OmbiExternal.db")}");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlexServerContent>().HasMany(x => x.Episodes)
                .WithOne(x => x.Series)
                .HasPrincipalKey(x => x.Key)
                .HasForeignKey(x => x.GrandparentKey);

            builder.Entity<EmbyEpisode>()
                .HasOne(p => p.Series)
                .WithMany(b => b.Episodes)
                .HasPrincipalKey(x => x.EmbyId)
                .HasForeignKey(p => p.ParentId);

            base.OnModelCreating(builder);
        }


        public void Seed()
        {
            // VACUUM;
            Database.ExecuteSqlCommand("VACUUM;");
            SaveChanges();
        }
    }
}