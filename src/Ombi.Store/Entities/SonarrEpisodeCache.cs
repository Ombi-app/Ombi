using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    /// <summary>
    /// Cached Sonarr episode metadata used for series availability and request workflows.
    /// Uses <see cref="LongEntity"/> because rows are re-inserted frequently during sync.
    /// </summary>
    [Table("SonarrEpisodeCache")]
    public class SonarrEpisodeCache : LongEntity, IBaseMediaServerEpisode
    {
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int TvDbId { get; set; }
        public int MovieDbId { get; set; }
        public bool HasFile { get; set; }
    }
}