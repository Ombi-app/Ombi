namespace Ombi.Api.FanartTv.Models
{

    public class MovieResult
    {
        public string name { get; set; }
        public string tmdb_id { get; set; }
        public string imdb_id { get; set; }
        public Content[] hdmovieclearart { get; set; }
        public Content[] hdmovielogo { get; set; }
        public Content[] moviebackground { get; set; }
        public Content[] movieposter { get; set; }
        public Content[] moviedisc { get; set; }
        public Content[] moviebanner { get; set; }
        public Content[] moviethumb { get; set; }
    }
}
