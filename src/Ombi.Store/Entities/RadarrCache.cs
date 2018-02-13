using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("RadarrCache")]
    public class RadarrCache : Entity
    {
        public int TheMovieDbId { get; set; }
        public bool HasFile { get; set; }
    }
}