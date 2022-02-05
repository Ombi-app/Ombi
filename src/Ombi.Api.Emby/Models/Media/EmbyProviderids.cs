using Ombi.Api.MediaServer.Models;

namespace Ombi.Api.Emby.Models.Movie
{
    public class EmbyProviderids: BaseProviderids
    {
        public string TmdbCollection { get; set; }
        public string Zap2It { get; set; }
        public string TvRage { get; set; }
    }
}