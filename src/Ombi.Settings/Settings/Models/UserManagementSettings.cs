using Ombi.Store.Entities;
using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models
{
    public class UserManagementSettings : Settings
    {
        public bool ImportPlexAdmin { get; set; }
        public bool ImportPlexUsers { get; set; }
        public bool CleanupPlexUsers { get; set; }
        public bool ImportEmbyUsers { get; set; }
        public bool ImportJellyfinUsers { get; set; }
        public int MovieRequestLimit { get; set; }
        public RequestLimitType MovieRequestLimitType { get; set; } = RequestLimitType.Week;
        public int EpisodeRequestLimit { get; set; }
        public RequestLimitType EpisodeRequestLimitType { get; set; } = RequestLimitType.Week;
        public int MusicRequestLimit { get; set; }
        public RequestLimitType MusicRequestLimitType { get; set; } = RequestLimitType.Week;
        public string DefaultStreamingCountry { get; set; } = "US";
        public List<string> DefaultRoles { get; set; } = new List<string>();
        public List<string> BannedPlexUserIds { get; set; } = new List<string>();
        public List<string> BannedEmbyUserIds { get; set; } = new List<string>();
        public List<string> BannedJellyfinUserIds { get; set; } = new List<string>();
    }
}
