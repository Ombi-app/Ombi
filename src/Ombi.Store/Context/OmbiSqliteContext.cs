using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public sealed class OmbiSqliteContext : OmbiContext
    {
        private static bool _created;
        public OmbiSqliteContext(DbContextOptions<OmbiSqliteContext> options) : base(options)
        {
            if (_created) return;


            _created = true;
            Database.SetCommandTimeout(60);
            Database.Migrate();
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
    }
}