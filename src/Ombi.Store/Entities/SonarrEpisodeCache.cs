using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("SonarrEpisodeCache")]
    public class SonarrEpisodeCache : Entity, IBaseMediaServerEpisode
    {
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int TvDbId { get; set; }
        public int MovieDbId { get; set; }
        public bool HasFile { get; set; }
    }
}