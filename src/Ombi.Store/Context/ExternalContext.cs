using System.IO;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public abstract class ExternalContext : DbContext
    {
        protected ExternalContext(DbContextOptions<ExternalContext> options) : base(options)
        {

        }

        /// <summary>
        /// This allows a sub class to call the base class 'DbContext' non typed constructor
        /// This is need because instances of the subclasses will use a specific typed DbContextOptions
        /// which can not be converted into the parameter in the above constructor
        /// </summary>
        /// <param name="options"></param>
        protected ExternalContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<PlexServerContent> PlexServerContent { get; set; }
        public DbSet<PlexSeasonsContent> PlexSeasonsContent { get; set; }
        public DbSet<PlexEpisode> PlexEpisode { get; set; }
        public DbSet<RadarrCache> RadarrCache { get; set; }
        public DbSet<CouchPotatoCache> CouchPotatoCache { get; set; }
        public DbSet<EmbyContent> EmbyContent { get; set; }
        public DbSet<EmbyEpisode> EmbyEpisode { get; set; }
        public DbSet<JellyfinEpisode> JellyfinEpisode { get; set; }
        public DbSet<JellyfinContent> JellyfinContent { get; set; }
        
        public DbSet<SonarrCache> SonarrCache { get; set; }
        public DbSet<LidarrArtistCache> LidarrArtistCache { get; set; }
        public DbSet<LidarrAlbumCache> LidarrAlbumCache { get; set; }
        public DbSet<SonarrEpisodeCache> SonarrEpisodeCache { get; set; }
        public DbSet<SickRageCache> SickRageCache { get; set; }
        public DbSet<SickRageEpisodeCache> SickRageEpisodeCache { get; set; }

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

            builder.Entity<JellyfinEpisode>()
                .HasOne(p => p.Series)
                .WithMany(b => b.Episodes)
                .HasPrincipalKey(x => x.JellyfinId)
                .HasForeignKey(p => p.ParentId);

            base.OnModelCreating(builder);
        }
    }
}
