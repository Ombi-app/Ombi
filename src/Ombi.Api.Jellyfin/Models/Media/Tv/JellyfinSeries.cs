using Ombi.Api.Jellyfin.Models.Movie;
using System;

namespace Ombi.Api.Jellyfin.Models.Media.Tv
{
    public class JellyfinSeries
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public DateTime PremiereDate { get; set; }
        public string OfficialRating { get; set; }
        public float CommunityRating { get; set; }
        public long RunTimeTicks { get; set; }
        public string PlayAccess { get; set; }
        public int ProductionYear { get; set; }
        public bool IsFolder { get; set; }
        public string Type { get; set; }
        public int LocalTrailerCount { get; set; }
        public JellyfinUserdata UserData { get; set; }
        public int ChildCount { get; set; }
        public string Status { get; set; }
        public string AirTime { get; set; }
        public string[] AirDays { get; set; }
        public JellyfinImagetags ImageTags { get; set; }
        public string[] BackdropImageTags { get; set; }
        public string LocationType { get; set; }
        public DateTime EndDate { get; set; }
        
        public JellyfinProviderids ProviderIds { get; set; }
    }
}