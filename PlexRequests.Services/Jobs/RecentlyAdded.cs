#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RecentlyAdded.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Jobs.Templates;
using Quartz;


namespace PlexRequests.Services.Jobs
{
    public class RecentlyAdded : IJob, IRecentlyAdded
    {
        public RecentlyAdded(IPlexApi api, ISettingsService<PlexSettings> plexSettings, ISettingsService<EmailNotificationSettings> email,
            ISettingsService<ScheduledJobsSettings> scheduledService, IJobRecord rec)
        {
            JobRecord = rec;
            Api = api;
            PlexSettings = plexSettings;
            EmailSettings = email;
            ScheduledJobsSettings = scheduledService;
        }

        private IPlexApi Api { get; }
        private TvMazeApi TvApi = new TvMazeApi();
        private readonly TheMovieDbApi _movieApi = new TheMovieDbApi();
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<ScheduledJobsSettings> ScheduledJobsSettings { get; }
        private IJobRecord JobRecord { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var jobs = JobRecord.GetJobs();
                var thisJob =
                    jobs.FirstOrDefault(
                        x => x.Name.Equals(JobNames.RecentlyAddedEmail, StringComparison.CurrentCultureIgnoreCase));

                var settings = ScheduledJobsSettings.GetSettings();

                if (thisJob?.LastRun > DateTime.Now.AddHours(-settings.RecentlyAdded))
                {
                    return;
                }

                Start();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                JobRecord.Record(JobNames.RecentlyAddedEmail);
            }
        }

        public void Test()
        {
            Start(true);
        }

        private void Start(bool testEmail = false)
        {
            var sb = new StringBuilder();
            var plexSettings = PlexSettings.GetSettings();

            var recentlyAdded = Api.RecentlyAdded(plexSettings.PlexAuthToken, plexSettings.FullUri);

            var movies =
                recentlyAdded._children.Where(x => x.type.Equals("Movie", StringComparison.CurrentCultureIgnoreCase));
            var tv =
                recentlyAdded._children.Where(
                    x => x.type.Equals("season", StringComparison.CurrentCultureIgnoreCase))
                    .GroupBy(x => x.parentTitle)
                    .Select(x => x.FirstOrDefault());

            GenerateMovieHtml(movies, plexSettings, ref sb);
            GenerateTvHtml(tv, plexSettings, ref sb);

            var template = new RecentlyAddedTemplate();
            var html = template.LoadTemplate(sb.ToString());

            Send(html, plexSettings, testEmail);
        }

        private void GenerateMovieHtml(IEnumerable<RecentlyAddedChild> movies, PlexSettings plexSettings, ref StringBuilder sb)
        {
            sb.Append("<h1>New Movies:</h1><br/><br/>");
            sb.Append("<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var movie in movies)
            {
                var metaData = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                    movie.ratingKey.ToString());

                var imdbId = PlexHelper.GetProviderIdFromPlexGuid(metaData.Video.Guid);
                var info = _movieApi.GetMovieInformation(imdbId).Result;

                sb.Append("<tr>");
                sb.Append("<td align=\"center\">");
                sb.AppendFormat("<img src=\"https://image.tmdb.org/t/p/w500{0}\" width=\"400px\" text-align=\"center\" />", info.BackdropPath);
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                sb.AppendFormat("<a href=\"https://www.imdb.com/title/{0}/\"><h3 style=\"font-family: sans-serif; font-weight: normal; margin: 0; Margin-bottom: 15px;\">{1} {2}</p></a>",
                    info.ImdbId, info.Title, info.ReleaseDate?.ToString("yyyy") ?? string.Empty);

                if (info.Genres.Any())
                {
                    sb.AppendFormat(
                        "<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">Genre: {0}</p>",
                        string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray()));
                }
                sb.AppendFormat("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">{0}</p>", info.Overview);

                sb.Append("<td");
                sb.Append("<hr>");
                sb.Append("<br>");
                sb.Append("<br>");
                sb.Append("</tr>");

            }
            sb.Append("</table><br/><br/>");
        }

        private void GenerateTvHtml(IEnumerable<RecentlyAddedChild> tv, PlexSettings plexSettings, ref StringBuilder sb)
        {
            // TV
            sb.Append("<h1>New Episodes:</h1><br/><br/>");
            sb.Append("<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var t in tv)
            {
                var parentMetaData = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                    t.parentRatingKey.ToString());

                var info = TvApi.ShowLookupByTheTvDbId(int.Parse(PlexHelper.GetProviderIdFromPlexGuid(parentMetaData.Directory.Guid)));
                var banner = info.image?.original;
                if (!string.IsNullOrEmpty(banner))
                {
                    banner = banner.Replace("http", "https"); // Always use the Https banners
                }
                sb.Append("<tr>");
                sb.Append("<td align=\"center\">");
                sb.AppendFormat("<img src=\"{0}\" width=\"400px\" text-align=\"center\" />", banner);
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                sb.AppendFormat("<a href=\"https://www.imdb.com/title/{0}/\"><h3 style=\"font-family: sans-serif; font-weight: normal; margin: 0; Margin-bottom: 15px;\">{1} {2}</p></a>",
                    info.externals.imdb, info.name, info.premiered.Substring(0, 4)); // Only the year

                sb.AppendFormat("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">Genre: {0}</p>", string.Join(", ", info.genres.Select(x => x.ToString()).ToArray()));
                sb.AppendFormat("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">{0}</p>",
                    string.IsNullOrEmpty(parentMetaData.Directory.Summary) ? info.summary : parentMetaData.Directory.Summary); // Episode Summary

                sb.Append("<td");
                sb.Append("<hr>");
                sb.Append("<br>");
                sb.Append("<br>");
                sb.Append("</tr>");
            }
            sb.Append("</table><br/><br/>");
        }

        private void Send(string html, PlexSettings plexSettings, bool testEmail = false)
        {
            var settings = EmailSettings.GetSettings();

            if (!settings.Enabled || string.IsNullOrEmpty(settings.EmailHost))
            {
                return;
            }

            var body = new BodyBuilder { HtmlBody = html, TextBody = "This email is only available on devices that support HTML." };
            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = "New Content on Plex!",
            };

            if (!testEmail)
            {
                var users = Api.GetUsers(plexSettings.PlexAuthToken);
                foreach (var user in users.User)
                {
                    message.Bcc.Add(new MailboxAddress(user.Username, user.Email));
                }
            }
            message.Bcc.Add(new MailboxAddress(settings.EmailUsername, settings.EmailSender)); // Include the admin

            message.From.Add(new MailboxAddress(settings.EmailUsername, settings.EmailSender));
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(settings.EmailHost, settings.EmailPort); // Let MailKit figure out the correct SecureSocketOptions.

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (settings.Authentication)
                    {
                        client.Authenticate(settings.EmailUsername, settings.EmailPassword);
                    }
                    Log.Info("sending message to {0} \r\n from: {1}\r\n Are we authenticated: {2}", message.To, message.From, client.IsAuthenticated);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}