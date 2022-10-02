using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table(nameof(PlexWatchlistUserError))]
    public class PlexWatchlistUserError : Entity
    {
        public string UserId { get; set; }
        public string MediaServerToken { get; set; }
    }
}
