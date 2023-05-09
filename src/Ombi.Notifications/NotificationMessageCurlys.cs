using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using Ombi.Helpers;
using Ombi.I18n.Resources;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications
{
    public class NotificationMessageCurlys
    {
        public void SetupNewsletter(CustomizationSettings s)
        {
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s.ApplicationName;
            ApplicationUrl = s?.ApplicationUrl.HasValue() ?? false ? s.ApplicationUrl : string.Empty;
        }

        public void Setup(OmbiUser user, CustomizationSettings s)
        {
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s.ApplicationName;
            ApplicationUrl = s?.ApplicationUrl.HasValue() ?? false ? s.ApplicationUrl : string.Empty;
            RequestedUser = user.UserName;
            Alias = user.UserAlias;
            UserName = user.UserName;
        }

        public void Setup(NotificationOptions opts, MovieRequests req, CustomizationSettings s,
            UserNotificationPreferences pref)
        {
            LoadIssues(opts);
            LoadCommon(req, s, pref, opts);
            LoadTitle(opts, req);
            ProviderId = req?.TheMovieDbId.ToString() ?? string.Empty;
            Year = req?.ReleaseDate.Year.ToString();
            Overview = req?.Overview;
            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;

            var img = req?.PosterPath ?? string.Empty;
            if (img.HasValue())
            {
                if (img.StartsWith("http"))
                {
                    // This means it's a legacy request from when we used TvMaze as a provider.
                    // The poster url is the fully qualified address, so just use it
                    PosterImage = img;
                }
                else
                {
                    PosterImage =
                        $"https://image.tmdb.org/t/p/w300/{img?.TrimStart('/') ?? string.Empty}";
                }
            }
            CalculateRequestStatus(req);
        }

        public void Setup(NotificationOptions opts, ChildRequests req, CustomizationSettings s,
            UserNotificationPreferences pref)
        {
            LoadIssues(opts);
            LoadCommon(req, s, pref, opts);
            LoadTitle(opts, req);
            ProviderId = req?.ParentRequest?.ExternalProviderId.ToString() ?? string.Empty;
            Year = req?.ParentRequest?.ReleaseDate.Year.ToString();
            Overview = req?.ParentRequest?.Overview;
            AdditionalInformation = opts.AdditionalInformation;
            var img = req?.ParentRequest?.PosterPath ?? string.Empty;
            if (img.HasValue())
            {
                if (img.StartsWith("http"))
                {
                    // This means it's a legacy request from when we used TvMaze as a provider.
                    // The poster url is the fully qualified address, so just use it
                    PosterImage = img;
                }
                else
                {
                    PosterImage =
                        $"https://image.tmdb.org/t/p/w300/{img?.TrimStart('/') ?? string.Empty}";
                }
            }

            // Generate episode list.
            StringBuilder epSb = new StringBuilder();
            IEnumerable<EpisodeRequests> episodes = req?.SeasonRequests?
                .SelectMany(x => x.Episodes) ?? new List<EpisodeRequests>();
            episodes
                .OrderBy(x => x.EpisodeNumber)
                .ToList()
                .ForEach(ep => epSb.Append($"{ep.EpisodeNumber},"));
            if (epSb.Length > 0) epSb.Remove(epSb.Length - 1, 1);
            EpisodesList = epSb.ToString();

            // Generate season list.
            StringBuilder seasonSb = new StringBuilder();
            List<SeasonRequests> seasons = req?.SeasonRequests ?? new List<SeasonRequests>();
            seasons
                .OrderBy(x => x.SeasonNumber)
                .ToList()
                .ForEach(ep => seasonSb.Append($"{ep.SeasonNumber},"));
            if (seasonSb.Length > 0) seasonSb.Remove(seasonSb.Length - 1, 1);
            SeasonsList = seasonSb.ToString();
            CalculateRequestStatus(req);
        }

        public void Setup(NotificationOptions opts, AlbumRequest req, CustomizationSettings s,
            UserNotificationPreferences pref)
        {
            LoadIssues(opts);
            LoadCommon(req, s, pref, opts);
            LoadTitle(opts, req);
            ProviderId = req?.ForeignArtistId ?? string.Empty;
            Year = req?.ReleaseDate.Year.ToString();
            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;
            PosterImage = req?.Cover.HasValue() ?? false ? req.Cover : req?.Disk ?? string.Empty;
            CalculateRequestStatus(req);
        }

        private void LoadIssues(NotificationOptions opts)
        {
            IssueDescription = opts.Substitutes.TryGetValue(NotificationSubstitues.IssueDescription, out string val) ? val : string.Empty;
            IssueCategory = opts.Substitutes.TryGetValue(NotificationSubstitues.IssueCategory, out val) ? val : string.Empty;
            IssueStatus = opts.Substitutes.TryGetValue(NotificationSubstitues.IssueStatus, out val) ? val : string.Empty;
            IssueSubject = opts.Substitutes.TryGetValue(NotificationSubstitues.IssueSubject, out val) ? val : string.Empty;
            NewIssueComment = opts.Substitutes.TryGetValue(NotificationSubstitues.NewIssueComment, out val) ? val : string.Empty;
            UserName = opts.Substitutes.TryGetValue(NotificationSubstitues.IssueUser, out val) ? val : string.Empty;
            Alias = opts.Substitutes.TryGetValue(NotificationSubstitues.IssueUserAlias, out val) ? val : string.Empty;
            Type = opts.Substitutes.TryGetValue(NotificationSubstitues.RequestType, out val) && Enum.TryParse(val, out RequestType type)
                ? HumanizeReturnType(type)
                : string.Empty;
            if (opts.Substitutes.TryGetValue(NotificationSubstitues.PosterPath, out val) && val.HasValue())
            {
                PosterImage = $"https://image.tmdb.org/t/p/w300/{val.TrimStart('/')}";
            }
            else
            {
                PosterImage = string.Empty;
            }
        }

        private void LoadCommon(BaseRequest req, CustomizationSettings s, UserNotificationPreferences pref, NotificationOptions opts)
        {
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s.ApplicationName;
            ApplicationUrl = s?.ApplicationUrl.HasValue() ?? false ? s.ApplicationUrl : string.Empty;
            AvailableDate = req?.MarkedAsAvailable?.ToString("D") ?? string.Empty;
            DenyReason = req?.DeniedReason;
            RequestId = req?.Id.ToString();
            RequestedUser = req?.RequestedUser?.UserName;
            RequestedDate = req?.RequestedDate.ToString("D");
            DetailsUrl = GetDetailsUrl(s, req);
            RequestedByAlias = req?.RequestedByAlias;

            if (Type.IsNullOrEmpty())
            {
                Type = HumanizeReturnType(req?.RequestType);
            }

            if (UserName.IsNullOrEmpty())
            {
                UserName = req?.RequestedUser?.UserName;
            }

            if (Alias.IsNullOrEmpty())
            {
                Alias = req?.RequestedUser?.Alias.HasValue() ?? false
                    ? req.RequestedUser?.Alias
                    : req?.RequestedUser?.UserName;
            }

            if (pref != null)
            {
                UserPreference = pref.Value.HasValue() ? pref.Value : Alias;
            }

            if (opts.NotificationType == NotificationType.PartiallyAvailable)
            {
                if (opts.Substitutes.TryGetValue("Season", out var sNumber))
                {
                    PartiallyAvailableSeasonNumber = sNumber;
                }
                if (opts.Substitutes.TryGetValue("Episodes", out var epNumber))
                {
                    PartiallyAvailableEpisodeNumbers = epNumber;
                }
                if (opts.Substitutes.TryGetValue("EpisodesCount", out var epCount))
                {
                    PartiallyAvailableEpisodeCount = epCount;
                }
                if (opts.Substitutes.TryGetValue("SeasonEpisodes", out var sEpisodes))
                {
                    PartiallyAvailableEpisodesList = sEpisodes;
                }
            }
        }

        private static string HumanizeReturnType(RequestType? requestType)
        {
            return requestType switch
            {
                null => string.Empty,
                RequestType.TvShow => Texts.TvShow,
                RequestType.Album => Texts.Album,
                RequestType.Movie => Texts.Movie,
                _ => requestType.Humanize()
            };
        }

        private void LoadTitle(NotificationOptions opts, BaseRequest req)
        {
            switch (req)
            {
                case null:
                    opts.Substitutes.TryGetValue("Title", out string title);
                    Title = title;
                    break;
                case ChildRequests tvShowRequest:
                    Title = tvShowRequest.ParentRequest?.Title;
                    break;
                default:
                    Title = req.Title;
                    break;
            }
        }

        private string GetDetailsUrl(CustomizationSettings s, BaseRequest req)
        {
            if (string.IsNullOrEmpty(s.ApplicationUrl))
            {
                return string.Empty;
            }

            switch (req)
            {
                case MovieRequests movieRequest:
                    return $"{s.ApplicationUrl}/details/movie/{movieRequest.TheMovieDbId}";
                case ChildRequests tvRequest:
                    return $"{s.ApplicationUrl}/details/tv/{tvRequest.ParentRequest.ExternalProviderId}";
                case AlbumRequest albumRequest:
                    return $"{s.ApplicationUrl}/details/artist/{albumRequest.ForeignArtistId}";
                default:
                    return string.Empty;
            }
        }

        private void CalculateRequestStatus(BaseRequest req)
        {
            RequestStatus = string.Empty;
            if (req != null)
            {
                if (req.Available)
                {
                    RequestStatus = "Available";
                    return;
                }

                if (req.Denied ?? false)
                {
                    RequestStatus = "Denied";
                    return;
                }

                if (!req.Available && req.Approved)
                {
                    RequestStatus = "Processing Request";
                    return;
                }

                RequestStatus = "Pending Approval";
            }
        }

        // User Defined
        public string RequestId { get; set; }
        public string RequestedUser { get; set; }
        public string RequestedByAlias { get; set; }
        public string UserName { get; set; }
        public string IssueUser => UserName;
        public string Alias { get; set; }
        public string Title { get; set; }
        public string RequestedDate { get; set; }
        public string Type { get; set; }
        public string AdditionalInformation { get; set; }
        public string Overview { get; set; }
        public string Year { get; set; }
        public string EpisodesList { get; set; }
        public string SeasonsList { get; set; }
        public string DetailsUrl { get; set; }
        public string PosterImage { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationUrl { get; set; }
        public string IssueDescription { get; set; }
        public string IssueCategory { get; set; }
        public string IssueStatus { get; set; }
        public string IssueSubject { get; set; }
        public string NewIssueComment { get; set; }
        public string UserPreference { get; set; }
        public string DenyReason { get; set; }
        public string AvailableDate { get; set; }
        public string RequestStatus { get; set; }
        public string ProviderId { get; set; }
        public string PartiallyAvailableEpisodeNumbers { get; set; }
        public string PartiallyAvailableSeasonNumber { get; set; }
        public string PartiallyAvailableEpisodeCount { get; set; }
        public string PartiallyAvailableEpisodesList { get; set; }

        // System Defined
        private string LongDate => DateTime.Now.ToString("D");
        private string ShortDate => DateTime.Now.ToString("d");
        private string LongTime => DateTime.Now.ToString("T");
        private string ShortTime => DateTime.Now.ToString("t");

        public Dictionary<string, string> Curlys => new Dictionary<string, string>
        {
            { nameof(RequestId), RequestId },
            { nameof(RequestedUser), RequestedUser },
            { nameof(Title), Title },
            { nameof(RequestedDate), RequestedDate },
            { nameof(Type), Type },
            { nameof(AdditionalInformation), AdditionalInformation },
            { nameof(LongDate), LongDate },
            { nameof(ShortDate), ShortDate },
            { nameof(LongTime), LongTime },
            { nameof(ShortTime), ShortTime },
            { nameof(Overview), Overview },
            { nameof(Year), Year },
            { nameof(EpisodesList), EpisodesList },
            { nameof(SeasonsList), SeasonsList },
            { nameof(PosterImage), PosterImage },
            { nameof(ApplicationName), ApplicationName },
            { nameof(ApplicationUrl), ApplicationUrl },
            { nameof(IssueDescription), IssueDescription },
            { nameof(IssueCategory), IssueCategory },
            { nameof(IssueStatus), IssueStatus },
            { nameof(IssueSubject), IssueSubject },
            { nameof(NewIssueComment), NewIssueComment },
            { nameof(IssueUser), IssueUser },
            { nameof(UserName), UserName },
            { nameof(Alias), Alias },
            { nameof(RequestedByAlias), RequestedByAlias },
            { nameof(UserPreference), UserPreference },
            { nameof(DenyReason), DenyReason },
            { nameof(AvailableDate), AvailableDate },
            { nameof(RequestStatus), RequestStatus },
            { nameof(ProviderId), ProviderId },
            { nameof(PartiallyAvailableEpisodeNumbers), PartiallyAvailableEpisodeNumbers },
            { nameof(PartiallyAvailableSeasonNumber), PartiallyAvailableSeasonNumber },
            { nameof(PartiallyAvailableEpisodesList), PartiallyAvailableEpisodesList },
            { nameof(PartiallyAvailableEpisodeCount), PartiallyAvailableEpisodeCount },
        };
    }
}