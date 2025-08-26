namespace Ombi.Api.External.Models
{
    public class BaseProviderids
    {
        public string Tvdb { get; set; }
        public string Tmdb { get; set; }
        public string Imdb { get; set; }
        
        public bool Any() => !string.IsNullOrEmpty(Tvdb) || !string.IsNullOrEmpty(Tmdb) || !string.IsNullOrEmpty(Imdb);
    }
}
