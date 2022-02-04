namespace Ombi.Api.Emby.Models.Movie
{
    public class EmbyProviderids
    {
        public string Tmdb { get; set; }
        public string Imdb { get; set; }
        public string TmdbCollection { get; set; }

        public string Tvdb { get; set; }
        public string Zap2It { get; set; }
        public string TvRage { get; set; }
        
        public bool Any()
        {
            if (string.IsNullOrEmpty(Imdb)
               && string.IsNullOrEmpty(Tmdb)
               && string.IsNullOrEmpty(Tvdb))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}