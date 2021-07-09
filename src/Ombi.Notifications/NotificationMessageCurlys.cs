using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using Ombi.Helpers;
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
            LoadCommon(req, s, pref);
            LoadTitle(opts, req);
            ProviderId = req?.TheMovieDbId.ToString() ?? string.Empty;
            Year = req?.ReleaseDate.Year.ToString();
            Overview = req?.Overview;
            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;
            PosterImage = $"https://image.tmdb.org/t/p/w300/{req?.PosterPath?.TrimStart('/') ?? string.Empty}";
            CalculateRequestStatus(req);
        }

        public void Setup(NotificationOptions opts, ChildRequests req, CustomizationSettings s,
            UserNotificationPreferences pref)
        {
            LoadIssues(opts);
            LoadCommon(req, s, pref);
            LoadTitle(opts, req);
            ProviderId = req?.ParentRequest?.ExternalProviderId.ToString() ?? string.Empty;
            Year = req?.ParentRequest?.ReleaseDate.Year.ToString();
            Overview = req?.ParentRequest?.Overview;
            AdditionalInformation = opts.AdditionalInformation;
            PosterImage =
                $"https://image.tmdb.org/t/p/w300/{req?.ParentRequest?.PosterPath?.TrimStart('/') ?? string.Empty}";

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
            LoadCommon(req, s, pref);
            LoadTitle(opts, req);
            ProviderId = req?.ForeignArtistId ?? string.Empty;
            Year = req?.ReleaseDate.Year.ToString();
            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;
            PosterImage = req?.Cover.HasValue() ?? false ? req.Cover : req?.Disk ?? string.Empty;
            CalculateRequestStatus(req);
        }

        private void LoadIssues(NotificationOptions opts)
        {
            IssueDescription = opts.Substitutes.TryGetValue("IssueDescription", out string val) ? val : string.Empty;
            IssueCategory = opts.Substitutes.TryGetValue("IssueCategory", out val) ? val : string.Empty;
            IssueStatus = opts.Substitutes.TryGetValue("IssueStatus", out val) ? val : string.Empty;
            IssueSubject = opts.Substitutes.TryGetValue("IssueSubject", out val) ? val : string.Empty;
            NewIssueComment = opts.Substitutes.TryGetValue("NewIssueComment", out val) ? val : string.Empty;
            UserName = opts.Substitutes.TryGetValue("IssueUser", out val) ? val : string.Empty;
            Alias = opts.Substitutes.TryGetValue("IssueUserAlias", out val) ? val : string.Empty;
            Type = opts.Substitutes.TryGetValue("RequestType", out val) && Enum.TryParse(val, out RequestType type)
                ? HumanizeReturnType(type)
                : string.Empty;
        }

        private void LoadCommon(BaseRequest req, CustomizationSettings s, UserNotificationPreferences pref)
        {
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s.ApplicationName;
            ApplicationUrl = s?.ApplicationUrl.HasValue() ?? false ? s.ApplicationUrl : string.Empty;
            AvailableDate = req?.MarkedAsAvailable?.ToString("D") ?? string.Empty;
            DenyReason = req?.DeniedReason;
            RequestId = req?.Id.ToString();
            RequestedUser = req?.RequestedUser?.UserName;
            RequestedDate = req?.RequestedDate.ToString("D");

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
        }

        private static string HumanizeReturnType(RequestType? requestType)
        {
            return requestType switch
            {
                null => string.Empty,
                RequestType.TvShow => "TV Show",
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
            { nameof(UserPreference), UserPreference },
            { nameof(DenyReason), DenyReason },
            { nameof(AvailableDate), AvailableDate },
            { nameof(RequestStatus), RequestStatus },
            { nameof(ProviderId), ProviderId },
        };
    }
}