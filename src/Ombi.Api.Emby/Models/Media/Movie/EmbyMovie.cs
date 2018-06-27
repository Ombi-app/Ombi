using System;

namespace Ombi.Api.Emby.Models.Movie
{
    public class EmbyMovie
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public string Container { get; set; }
        public DateTime PremiereDate { get; set; }
        public object[] ProductionLocations { get; set; }
        public string OfficialRating { get; set; }
        public float CommunityRating { get; set; }
        public long RunTimeTicks { get; set; }
        public string PlayAccess { get; set; }
        public int ProductionYear { get; set; }
        public bool IsPlaceHolder { get; set; }
        public bool IsHD { get; set; }
        public bool IsFolder { get; set; }
        public string Type { get; set; }
        public int LocalTrailerCount { get; set; }
        public EmbyUserdata UserData { get; set; }
        public string VideoType { get; set; }
        public EmbyImagetags ImageTags { get; set; }
        public string[] BackdropImageTags { get; set; }
        public string LocationType { get; set; }
        public string MediaType { get; set; }
        public bool HasSubtitles { get; set; }
        public int CriticRating { get; set; }
        public string Overview { get; set; }
        public EmbyProviderids ProviderIds { get; set; }
    }
}