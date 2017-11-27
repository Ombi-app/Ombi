using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("SickRageEpisodeCache")]
    public class SickRageEpisodeCache : Entity
    {
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int TvDbId { get; set; }
    }
}