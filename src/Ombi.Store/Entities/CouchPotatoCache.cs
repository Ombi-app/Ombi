using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("CouchPotatoCache")]
    public class CouchPotatoCache : Entity
    {
        public int TheMovieDbId { get; set; }
    }
}