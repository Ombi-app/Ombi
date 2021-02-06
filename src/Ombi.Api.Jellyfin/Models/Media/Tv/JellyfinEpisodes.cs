using Ombi.Api.Jellyfin.Models.Movie;
using System;

namespace Ombi.Api.Jellyfin.Models.Media.Tv
{
    public class JellyfinEpisodes
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public string Container { get; set; }
        public DateTime PremiereDate { get; set; }
        public float CommunityRating { get; set; }
        public long RunTimeTicks { get; set; }
        public string PlayAccess { get; set; }
        public int ProductionYear { get; set; }
        public bool IsPlaceHolder { get; set; }
        public int IndexNumber { get; set; }
        public int? IndexNumberEnd { get; set; }
        public int ParentIndexNumber { get; set; }
        public bool IsHD { get; set; }
        public bool IsFolder { get; set; }
        public string Type { get; set; }
        public string ParentLogoItemId { get; set; }
        public string ParentBackdropItemId { get; set; }
        public string[] ParentBackdropImageTags { get; set; }
        public int LocalTrailerCount { get; set; }
        public JellyfinUserdata UserData { get; set; }
        public string SeriesName { get; set; }
        public string SeriesId { get; set; }
        public string SeasonId { get; set; }
        public string SeriesPrimaryImageTag { get; set; }
        public string SeasonName { get; set; }
        public string VideoType { get; set; }
        public JellyfinImagetags ImageTags { get; set; }
        public object[] BackdropImageTags { get; set; }
        public string ParentLogoImageTag { get; set; }
        public string ParentThumbItemId { get; set; }
        public string ParentThumbImageTag { get; set; }
        public string LocationType { get; set; }
        public string MediaType { get; set; }
        public bool HasSubtitles { get; set; }
        public JellyfinProviderids ProviderIds { get; set; }
    }
}