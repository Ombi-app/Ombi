using Ombi.Api.External.Models;

namespace Ombi.Api.External.MediaServers.Emby.Models.Movie
{
    public class EmbyProviderids: BaseProviderids
    {
        public string TmdbCollection { get; set; }
        public string Zap2It { get; set; }
        public string TvRage { get; set; }
    }
}