using System;
using Ombi.Api.Jellyfin.Models.Movie;

namespace Ombi.Api.Jellyfin.Models.Media.Tv
{
    public class EpisodeInformation
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public string Etag { get; set; }
        public DateTime DateCreated { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }
        public bool SupportsSync { get; set; }
        public string Container { get; set; }
        public string SortName { get; set; }
        public DateTime PremiereDate { get; set; }
        public JellyfinExternalurl[] ExternalUrls { get; set; }
        public JellyfinMediasource[] MediaSources { get; set; }
        public string Path { get; set; }
        public string Overview { get; set; }
        public object[] Taglines { get; set; }
        public object[] Genres { get; set; }
        public string[] SeriesGenres { get; set; }
        public float CommunityRating { get; set; }
        public int VoteCount { get; set; }
        public long RunTimeTicks { get; set; }
        public string PlayAccess { get; set; }
        public int ProductionYear { get; set; }
        public bool IsPlaceHolder { get; set; }
        public int IndexNumber { get; set; }
        public int ParentIndexNumber { get; set; }
        public object[] RemoteTrailers { get; set; }
        public JellyfinProviderids ProviderIds { get; set; }
        public bool IsHD { get; set; }
        public bool IsFolder { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
        public object[] People { get; set; }
        public object[] Studios { get; set; }
        public string ParentLogoItemId { get; set; }
        public string ParentBackdropItemId { get; set; }
        public string[] ParentBackdropImageTags { get; set; }
        public int LocalTrailerCount { get; set; }
        public JellyfinUserdata UserData { get; set; }
        public string SeriesName { get; set; }
        public string SeriesId { get; set; }
        public string SeasonId { get; set; }
        public string DisplayPreferencesId { get; set; }
        public object[] Tags { get; set; }
        public object[] Keywords { get; set; }
        public string SeriesPrimaryImageTag { get; set; }
        public string SeasonName { get; set; }
        public JellyfinMediastream[] MediaStreams { get; set; }
        public string VideoType { get; set; }
        public JellyfinImagetags ImageTags { get; set; }
        public object[] BackdropImageTags { get; set; }
        public object[] ScreenshotImageTags { get; set; }
        public string ParentLogoImageTag { get; set; }
        public string SeriesStudio { get; set; }
        public JellyfinSeriesstudioinfo SeriesStudioInfo { get; set; }
        public string ParentThumbItemId { get; set; }
        public string ParentThumbImageTag { get; set; }
        public JellyfinChapter[] Chapters { get; set; }
        public string LocationType { get; set; }
        public string MediaType { get; set; }
        public object[] LockedFields { get; set; }
        public bool LockData { get; set; }
    }
}