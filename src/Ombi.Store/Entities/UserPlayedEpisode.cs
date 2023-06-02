namespace Ombi.Store.Entities
{
    public class UserPlayedEpisode : Entity
    {
        public int TheMovieDbId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string UserId { get; set; }
    }
}