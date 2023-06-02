namespace Ombi.Store.Entities
{
    public class UserPlayedMovie : Entity
    {
        public int TheMovieDbId { get; set; }
        public string UserId { get; set; }
    }
}