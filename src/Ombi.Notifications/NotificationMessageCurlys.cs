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
        public void Setup(NotificationOptions opts, MovieRequests req, CustomizationSettings s, UserNotificationPreferences pref)
        {
            LoadIssues(opts);

            RequestId = req?.Id.ToString();
            ProviderId = req?.TheMovieDbId.ToString() ?? string.Empty;
            string title;
            if (req == null)
            {
                opts.Substitutes.TryGetValue("Title", out title);
            }
            else
            {
                title = req?.Title;
            }
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
            RequestedUser = req?.RequestedUser?.UserName;
            if (UserName.IsNullOrEmpty())
            {
                // Can be set if it's an issue
                UserName = req?.RequestedUser?.UserName;
            }

            Alias = (req?.RequestedUser?.Alias.HasValue() ?? false) ? req?.RequestedUser?.Alias : req?.RequestedUser?.UserName;
            if (pref != null)
            {
                UserPreference = pref.Value.HasValue() ? pref.Value : Alias;
            }
            Title = title;
            RequestedDate = req?.RequestedDate.ToString("D");
            if (Type.IsNullOrEmpty())
            {
                Type = req?.RequestType.Humanize();
            }
            Overview = req?.Overview;
            Year = req?.ReleaseDate.Year.ToString();
            DenyReason = req?.DeniedReason;
            AvailableDate = req?.MarkedAsAvailable?.ToString("D") ?? string.Empty;
            if (req?.RequestType == RequestType.Movie)
            {
                PosterImage = string.Format((req?.PosterPath ?? string.Empty).StartsWith("/", StringComparison.InvariantCultureIgnoreCase)
                    ? "https://image.tmdb.org/t/p/w300{0}" : "https://image.tmdb.org/t/p/w300/{0}", req?.PosterPath);
            }
            else
            {
                PosterImage = req?.PosterPath;
            }

            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;

            CalculateRequestStatus(req);
        }

        public void Setup(NotificationOptions opts, AlbumRequest req, CustomizationSettings s, UserNotificationPreferences pref)
        {
            LoadIssues(opts);

            RequestId = req?.Id.ToString();
            ProviderId = req?.ForeignArtistId ?? string.Empty;

            string title;
            if (req == null)
            {
                opts.Substitutes.TryGetValue("Title", out title);
            }
            else
            {
                title = req?.Title;
            }
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
            RequestedUser = req?.RequestedUser?.UserName;
            if (UserName.IsNullOrEmpty())
            {
                // Can be set if it's an issue
                UserName = req?.RequestedUser?.UserName;
            }

            AvailableDate = req?.MarkedAsAvailable?.ToString("D") ?? string.Empty;
            DenyReason = req?.DeniedReason;
            Alias = (req?.RequestedUser?.Alias.HasValue() ?? false) ? req?.RequestedUser?.Alias : req?.RequestedUser?.UserName;
            if (pref != null)
            {
                UserPreference = pref.Value.HasValue() ? pref.Value : Alias;
            }
            Title = title;
            RequestedDate = req?.RequestedDate.ToString("D");
            if (Type.IsNullOrEmpty())
            {
                Type = req?.RequestType.Humanize();
            }
            Year = req?.ReleaseDate.Year.ToString();
            PosterImage = (req?.Cover.HasValue() ?? false) ? req.Cover : req?.Disk ?? string.Empty;

            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;
            CalculateRequestStatus(req);
        }

        public void SetupNewsletter(CustomizationSettings s)
        {
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
        }

        public void Setup(NotificationOptions opts, ChildRequests req, CustomizationSettings s, UserNotificationPreferences pref)
        {
            LoadIssues(opts);
            RequestId = req?.Id.ToString();
            ProviderId = req?.ParentRequest?.TvDbId.ToString() ?? string.Empty;
            string title;
            if (req == null)
            {
                opts.Substitutes.TryGetValue("Title", out title);
            }
            else
            {
                title = req?.ParentRequest.Title;
            }
            DenyReason = req?.DeniedReason;
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
            RequestedUser = req?.RequestedUser?.UserName;
            if (UserName.IsNullOrEmpty())
            {
                // Can be set if it's an issue
                UserName = req?.RequestedUser?.UserName;
            }
            AvailableDate = req?.MarkedAsAvailable?.ToString("D") ?? string.Empty;
            Alias = (req?.RequestedUser?.Alias.HasValue() ?? false) ? req?.RequestedUser?.Alias : req?.RequestedUser?.UserName;
            if (pref != null)
            {
                UserPreference = pref.Value.HasValue() ? pref.Value : Alias;
            }
            Title = title;
            RequestedDate = req?.RequestedDate.ToString("D");
            if (Type.IsNullOrEmpty())
            {
                Type = req?.RequestType.Humanize();
            }

            Overview = req?.ParentRequest.Overview;
            Year = req?.ParentRequest.ReleaseDate.Year.ToString();
            if (req?.RequestType == RequestType.Movie)
            {
                PosterImage = string.Format((req?.ParentRequest.PosterPath ?? string.Empty).StartsWith("/", StringComparison.InvariantCultureIgnoreCase)
                    ? "https://image.tmdb.org/t/p/w300{0}" : "https://image.tmdb.org/t/p/w300/{0}", req?.ParentRequest.PosterPath);
            }
            else
            {
                PosterImage = req?.ParentRequest.PosterPath;
            }
            AdditionalInformation = opts.AdditionalInformation;
            // DO Episode and Season Lists

            var episodes = req?.SeasonRequests?.SelectMany(x => x.Episodes) ?? new List<EpisodeRequests>();
            var seasons = req?.SeasonRequests?.OrderBy(x => x.SeasonNumber).ToList() ?? new List<SeasonRequests>();
            var orderedEpisodes = episodes.OrderBy(x => x.EpisodeNumber).ToList();
            var epSb = new StringBuilder();
            var seasonSb = new StringBuilder();
            for (var i = 0; i < orderedEpisodes.Count; i++)
            {
                var ep = orderedEpisodes[i];
                if (i < orderedEpisodes.Count - 1)
                {
                    epSb.Append($"{ep.EpisodeNumber},");
                }
                else
                {
                    epSb.Append($"{ep.EpisodeNumber}");
                }
            }

            for (var i = 0; i < seasons.Count; i++)
            {
                var ep = seasons[i];
                if (i < seasons.Count - 1)
                {
                    seasonSb.Append($"{ep.SeasonNumber},");
                }
                else
                {
                    seasonSb.Append($"{ep.SeasonNumber}");
                }
            }

            EpisodesList = epSb.ToString();
            SeasonsList = seasonSb.ToString();
            CalculateRequestStatus(req);
        }

        public void Setup(OmbiUser user, CustomizationSettings s)
        {
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
            RequestedUser = user.UserName;
            Alias = user.UserAlias;
            UserName = user.UserName;
        }

        private void LoadIssues(NotificationOptions opts)
        {
            var val = string.Empty;
            IssueDescription = opts.Substitutes.TryGetValue("IssueDescription", out val) ? val : string.Empty;
            IssueCategory = opts.Substitutes.TryGetValue("IssueCategory", out val) ? val : string.Empty;
            IssueStatus = opts.Substitutes.TryGetValue("IssueStatus", out val) ? val : string.Empty;
            IssueSubject = opts.Substitutes.TryGetValue("IssueSubject", out val) ? val : string.Empty;
            NewIssueComment = opts.Substitutes.TryGetValue("NewIssueComment", out val) ? val : string.Empty;
            UserName = opts.Substitutes.TryGetValue("IssueUser", out val) ? val : string.Empty;
            Type = opts.Substitutes.TryGetValue("RequestType", out val) ? val.Humanize() : string.Empty;
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
            {nameof(RequestId), RequestId },
            {nameof(RequestedUser), RequestedUser },
            {nameof(Title), Title },
            {nameof(RequestedDate), RequestedDate },
            {nameof(Type), Type },
            {nameof(AdditionalInformation), AdditionalInformation },
            {nameof(LongDate),LongDate},
            {nameof(ShortDate),ShortDate},
            {nameof(LongTime),LongTime},
            {nameof(ShortTime),ShortTime},
            {nameof(Overview),Overview},
            {nameof(Year),Year},
            {nameof(EpisodesList),EpisodesList},
            {nameof(SeasonsList),SeasonsList},
            {nameof(PosterImage),PosterImage},
            {nameof(ApplicationName),ApplicationName},
            {nameof(ApplicationUrl),ApplicationUrl},
            {nameof(IssueDescription),IssueDescription},
            {nameof(IssueCategory),IssueCategory},
            {nameof(IssueStatus),IssueStatus},
            {nameof(IssueSubject),IssueSubject},
            {nameof(NewIssueComment),NewIssueComment},
            {nameof(IssueUser),IssueUser},
            {nameof(UserName),UserName},
            {nameof(Alias),Alias},
            {nameof(UserPreference),UserPreference},
            {nameof(DenyReason),DenyReason},
            {nameof(AvailableDate),AvailableDate},
            {nameof(RequestStatus),RequestStatus},
            {nameof(ProviderId),ProviderId},
        };
    }
}