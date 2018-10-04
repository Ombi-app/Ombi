using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public interface IExternalContext : IDbContext
    {
        DbSet<CouchPotatoCache> CouchPotatoCache { get; set; }
        DbSet<EmbyContent> EmbyContent { get; set; }
        DbSet<EmbyEpisode> EmbyEpisode { get; set; }
        DbSet<LidarrAlbumCache> LidarrAlbumCache { get; set; }
        DbSet<LidarrArtistCache> LidarrArtistCache { get; set; }
        DbSet<PlexEpisode> PlexEpisode { get; set; }
        DbSet<PlexServerContent> PlexServerContent { get; set; }
        DbSet<RadarrCache> RadarrCache { get; set; }
        DbSet<SickRageCache> SickRageCache { get; set; }
        DbSet<SickRageEpisodeCache> SickRageEpisodeCache { get; set; }
        DbSet<SonarrCache> SonarrCache { get; set; }
        DbSet<SonarrEpisodeCache> SonarrEpisodeCache { get; set; }
    }
}