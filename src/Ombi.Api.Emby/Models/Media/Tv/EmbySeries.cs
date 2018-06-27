using Ombi.Api.Emby.Models.Movie;
using System;

namespace Ombi.Api.Emby.Models.Media.Tv
{
    public class EmbySeries
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
        public EmbyUserdata UserData { get; set; }
        public int ChildCount { get; set; }
        public string Status { get; set; }
        public string AirTime { get; set; }
        public string[] AirDays { get; set; }
        public EmbyImagetags ImageTags { get; set; }
        public string[] BackdropImageTags { get; set; }
        public string LocationType { get; set; }
        public DateTime EndDate { get; set; }
        
        public EmbyProviderids ProviderIds { get; set; }
    }
}