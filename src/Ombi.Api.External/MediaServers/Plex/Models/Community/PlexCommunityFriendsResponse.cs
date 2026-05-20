using System.Collections.Generic;

namespace Ombi.Api.External.MediaServers.Plex.Models.Community
{
    public class PlexCommunityFriendsResponse
    {
        public PlexCommunityFriendsData data { get; set; }
        public List<PlexCommunityError> errors { get; set; }
    }

    public class PlexCommunityFriendsData
    {
        public List<PlexCommunityFriend> allFriendsV2 { get; set; }
    }

    public class PlexCommunityFriend
    {
        public PlexCommunityUser user { get; set; }
        public string createdAt { get; set; }
    }

    public class PlexCommunityUser
    {
        public string id { get; set; }
        public string username { get; set; }
        public string displayName { get; set; }
        public string avatar { get; set; }
    }

    public class PlexCommunityError
    {
        public string message { get; set; }
    }
}
