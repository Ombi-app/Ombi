using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ombi.Api.Plex.Models
{
    public class PlexWatchlistContainer
    {
        public PlexWatchlist MediaContainer { get; set; }
        [JsonIgnore]
        public bool AuthError { get; set; }
    }
}