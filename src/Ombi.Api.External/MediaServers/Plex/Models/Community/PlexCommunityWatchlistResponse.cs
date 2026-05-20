using System.Collections.Generic;

namespace Ombi.Api.External.MediaServers.Plex.Models.Community
{
    public class PlexCommunityWatchlistResponse
    {
        public PlexCommunityWatchlistData data { get; set; }
        public List<PlexCommunityError> errors { get; set; }
    }

    public class PlexCommunityWatchlistData
    {
        public PlexCommunityUserV2 userV2 { get; set; }
    }

    public class PlexCommunityUserV2
    {
        public PlexCommunityWatchlist watchlist { get; set; }
    }

    public class PlexCommunityWatchlist
    {
        public List<PlexCommunityWatchlistNode> nodes { get; set; }
        public PlexCommunityPageInfo pageInfo { get; set; }
    }

    public class PlexCommunityWatchlistNode
    {
        public string id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class PlexCommunityPageInfo
    {
        public bool hasNextPage { get; set; }
        public string endCursor { get; set; }
    }
}
