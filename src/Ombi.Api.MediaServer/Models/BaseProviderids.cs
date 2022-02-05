namespace  Ombi.Api.MediaServer.Models
{
    public class BaseProviderids
    {
        public string Tmdb { get; set; }
        public string Imdb { get; set; }
        public string Tvdb { get; set; }
        public bool Any() => 
          !string.IsNullOrEmpty(Imdb) || !string.IsNullOrEmpty(Tmdb) || !string.IsNullOrEmpty(Tvdb);
    }
}