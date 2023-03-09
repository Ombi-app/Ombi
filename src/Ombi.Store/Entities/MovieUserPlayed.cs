using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    public class UserPlayedMovie : Entity
    {
        public string TheMovieDbId { get; set; }
        public string UserId { get; set; }
    }
}