using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("SonarrCache")]
    public class SonarrCache : Entity
    {
        public int TvDbId { get; set; }
        public int TheMovieDbId { get; set; }
    }
}