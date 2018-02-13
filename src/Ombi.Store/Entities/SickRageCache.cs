using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("SickRageCache")]
    public class SickRageCache : Entity
    {
        public int TvDbId { get; set; }
    }
}