using Ombi.Api.MediaServer.Models;

namespace Ombi.Api.Jellyfin.Models.Movie
{
    public class JellyfinProviderids: BaseProviderids
    {
        public string TmdbCollection { get; set; }
        public string Zap2It { get; set; }
        public string TvRage { get; set; }
    }
}