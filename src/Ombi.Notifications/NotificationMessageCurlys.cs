using System;
using System.Collections.Generic;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Notifications
{
    public class NotificationMessageCurlys
    {

        public void Setup(NotificationOptions opts, FullBaseRequest req, CustomizationSettings s)
        {
            LoadIssues(opts);
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
            RequestedUser = string.IsNullOrEmpty(req?.RequestedUser?.Alias)
                ? req?.RequestedUser?.UserName
                : req?.RequestedUser?.Alias;
            Title = title;
            RequestedDate = req?.RequestedDate.ToString("D");
            Type = req?.RequestType.ToString();
            Overview = req?.Overview;
            Year = req?.ReleaseDate.Year.ToString();
            PosterImage = req?.RequestType == RequestType.Movie ?
                string.Format("https://image.tmdb.org/t/p/w300{0}", req?.PosterPath) : req?.PosterPath;
            AdditionalInformation = opts?.AdditionalInformation ?? string.Empty;
        }

        public void Setup(NotificationOptions opts, ChildRequests req, CustomizationSettings s)
        {
            LoadIssues(opts);
            string title;
            if (req == null)
            {
                opts.Substitutes.TryGetValue("Title", out title);
            }
            else
            {
                title = req?.ParentRequest.Title;
            }
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
            RequestedUser = string.IsNullOrEmpty(req?.RequestedUser.Alias)
                ? req?.RequestedUser.UserName
                : req?.RequestedUser.Alias;
            Title = title;
            RequestedDate = req?.RequestedDate.ToString("D");
            Type = req?.RequestType.ToString();
            Overview = req?.ParentRequest.Overview;
            Year = req?.ParentRequest.ReleaseDate.Year.ToString();
            PosterImage = req?.RequestType == RequestType.Movie ?
                $"https://image.tmdb.org/t/p/w300{req?.ParentRequest.PosterPath}" : req?.ParentRequest.PosterPath;
            AdditionalInformation = opts.AdditionalInformation;
            // DO Episode and Season Lists
        }

        public void Setup(OmbiUser user, CustomizationSettings s)
        {
            ApplicationUrl = (s?.ApplicationUrl.HasValue() ?? false) ? s.ApplicationUrl : string.Empty;
            ApplicationName = string.IsNullOrEmpty(s?.ApplicationName) ? "Ombi" : s?.ApplicationName;
            RequestedUser = user.UserName;
        }

        private void LoadIssues(NotificationOptions opts)
        {
            var val = string.Empty;
            IssueDescription = opts.Substitutes.TryGetValue("IssueDescription", out val) ? val : string.Empty;
            IssueCategory = opts.Substitutes.TryGetValue("IssueCategory", out val) ? val : string.Empty;
            IssueStatus = opts.Substitutes.TryGetValue("IssueStatus", out val) ? val : string.Empty;
            IssueSubject = opts.Substitutes.TryGetValue("IssueSubject", out val) ? val : string.Empty;
            NewIssueComment = opts.Substitutes.TryGetValue("NewIssueComment", out val) ? val : string.Empty;
            IssueUser = opts.Substitutes.TryGetValue("IssueUser", out val) ? val : string.Empty;
        }

        // User Defined
        public string RequestedUser { get; set; }
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
        public string IssueUser { get; set; }

        // System Defined
        private string LongDate => DateTime.Now.ToString("D");
        private string ShortDate => DateTime.Now.ToString("d");
        private string LongTime => DateTime.Now.ToString("T");
        private string ShortTime => DateTime.Now.ToString("t");

        public Dictionary<string, string> Curlys => new Dictionary<string, string>
        {
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
        };
    }
}