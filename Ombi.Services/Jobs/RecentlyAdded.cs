#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RecentlyAddedModel.cs
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
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Core.Users;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs.Templates;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class RecentlyAdded : HtmlTemplateGenerator, IJob, IRecentlyAdded, IMassEmail
    {
        public RecentlyAdded(IPlexApi api, ISettingsService<PlexSettings> plexSettings,
            ISettingsService<EmailNotificationSettings> email, IJobRecord rec,
               ISettingsService<NewletterSettings> newsletter,
            IPlexReadOnlyDatabase db, IUserHelper userHelper)
        {
            JobRecord = rec;
            Api = api;
            PlexSettings = plexSettings;
            EmailSettings = email;
            NewsletterSettings = newsletter;
            PlexDb = db;
            UserHelper = userHelper;
        }

        private IPlexApi Api { get; }
        private TvMazeApi TvApi = new TvMazeApi();
        private readonly TheMovieDbApi _movieApi = new TheMovieDbApi();
        private const int MetadataTypeTv = 4;
        private const int MetadataTypeMovie = 1;
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<NewletterSettings> NewsletterSettings { get; }
        private IJobRecord JobRecord { get; }
        private IPlexReadOnlyDatabase PlexDb { get; }
        private IUserHelper UserHelper { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Start()
        {
            try
            {
                var settings = NewsletterSettings.GetSettings();
                if (!settings.SendRecentlyAddedEmail)
                {
                    return;
                }
                JobRecord.SetRunning(true, JobNames.RecentlyAddedEmail);
                Start(settings);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                JobRecord.Record(JobNames.RecentlyAddedEmail);
                JobRecord.SetRunning(false, JobNames.RecentlyAddedEmail);
            }
        }
        public void Execute(IJobExecutionContext context)
        {
            Start();
        }

        public void RecentlyAddedAdminTest()
        {
            Log.Debug("Starting Recently Added Newsletter Test");
            var settings = NewsletterSettings.GetSettings();
            Start(settings, true);
        }
        public void MassEmailAdminTest(string html, string subject)
        {
            Log.Debug("Starting Mass Email Test");
            var settings = NewsletterSettings.GetSettings();
            var plexSettings = PlexSettings.GetSettings();
            var template = new MassEmailTemplate();
            var body = template.LoadTemplate(html);
            Send(settings, body, plexSettings, true, subject);
        }
        public void SendMassEmail(string html, string subject)
        {
            Log.Debug("Starting Mass Email Test");
            var settings = NewsletterSettings.GetSettings();
            var plexSettings = PlexSettings.GetSettings();
            var template = new MassEmailTemplate();
            var body = template.LoadTemplate(html);
            Send(settings, body, plexSettings, false, subject);
        }

        private void Start(NewletterSettings newletterSettings, bool testEmail = false)
        {
            var sb = new StringBuilder();
            var plexSettings = PlexSettings.GetSettings();
            Log.Debug("Got Plex Settings");

            var libs = Api.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri);
            Log.Debug("Getting Plex Library Sections");

            var tvSections = libs.Directories.Where(x => x.type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase)); // We could have more than 1 lib
            Log.Debug("Filtered sections for TV");
            var movieSection = libs.Directories.Where(x => x.type.Equals(PlexMediaType.Movie.ToString(), StringComparison.CurrentCultureIgnoreCase)); // We could have more than 1 lib
            Log.Debug("Filtered sections for Movies");

            var plexVersion = Api.GetStatus(plexSettings.PlexAuthToken, plexSettings.FullUri).Version;

            var html = string.Empty;
            if (plexVersion.StartsWith("1.3"))
            {
                var tvMetadata = new List<Metadata>();
                var movieMetadata = new List<Metadata>();
                foreach (var tvSection in tvSections)
                {
                    var item = Api.RecentlyAdded(plexSettings.PlexAuthToken, plexSettings.FullUri,
                        tvSection?.Key);
                    if (item?.MediaContainer?.Metadata != null)
                    {
                        tvMetadata.AddRange(item?.MediaContainer?.Metadata);
                    }
                }
                Log.Debug("Got RecentlyAdded TV Shows");
                foreach (var movie in movieSection)
                {
                    var recentlyAddedMovies = Api.RecentlyAdded(plexSettings.PlexAuthToken, plexSettings.FullUri, movie?.Key);
                    if (recentlyAddedMovies?.MediaContainer?.Metadata != null)
                    {
                        movieMetadata.AddRange(recentlyAddedMovies?.MediaContainer?.Metadata);
                    }
                }
                Log.Debug("Got RecentlyAdded Movies");

                Log.Debug("Started Generating Movie HTML");
                GenerateMovieHtml(movieMetadata, plexSettings, sb);
                Log.Debug("Finished Generating Movie HTML");
                Log.Debug("Started Generating TV HTML");
                GenerateTvHtml(tvMetadata, plexSettings, sb);
                Log.Debug("Finished Generating TV HTML");

                var template = new RecentlyAddedTemplate();
                html = template.LoadTemplate(sb.ToString());
                Log.Debug("Loaded the template");
            }
            else
            {
                // Old API
                var tvChild = new List<RecentlyAddedChild>();
                var movieChild = new List<RecentlyAddedChild>();
                foreach (var tvSection in tvSections)
                {
                    var recentlyAddedTv = Api.RecentlyAddedOld(plexSettings.PlexAuthToken, plexSettings.FullUri, tvSection?.Key);
                    if (recentlyAddedTv?._children != null)
                    {
                        tvChild.AddRange(recentlyAddedTv?._children);
                    }
                }

                Log.Debug("Got RecentlyAdded TV Shows");
                foreach (var movie in movieSection)
                {
                    var recentlyAddedMovies = Api.RecentlyAddedOld(plexSettings.PlexAuthToken, plexSettings.FullUri, movie?.Key);
                    if (recentlyAddedMovies?._children != null)
                    {
                        tvChild.AddRange(recentlyAddedMovies?._children);
                    }
                }
                Log.Debug("Got RecentlyAdded Movies");

                Log.Debug("Started Generating Movie HTML");
                GenerateMovieHtml(movieChild, plexSettings, sb);
                Log.Debug("Finished Generating Movie HTML");
                Log.Debug("Started Generating TV HTML");
                GenerateTvHtml(tvChild, plexSettings, sb);
                Log.Debug("Finished Generating TV HTML");

                var template = new RecentlyAddedTemplate();
                html = template.LoadTemplate(sb.ToString());
                Log.Debug("Loaded the template");
            }


            string escapedHtml = new string(html.Where(c => !char.IsControl(c)).ToArray());
            Log.Debug(escapedHtml);
            Send(newletterSettings, escapedHtml, plexSettings, testEmail);
        }

        private void GenerateMovieHtml(List<RecentlyAddedChild> movies, PlexSettings plexSettings, StringBuilder sb)
        {
            var orderedMovies = movies.OrderByDescending(x => x?.addedAt.UnixTimeStampToDateTime()).ToList() ?? new List<RecentlyAddedChild>();
            sb.Append("<h1>New Movies:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var movie in orderedMovies)
            {
                var plexGUID = string.Empty;
                try
                {
                    var metaData = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                        movie.ratingKey.ToString());

                    plexGUID = metaData.Video.Guid;

                    var imdbId = PlexHelper.GetProviderIdFromPlexGuid(plexGUID);
                    var info = _movieApi.GetMovieInformation(imdbId).Result;
                    if (info == null)
                    {
                        throw new Exception($"Movie with Imdb id {imdbId} returned null from the MovieApi");
                    }
                    AddImageInsideTable(sb, $"https://image.tmdb.org/t/p/w500{info.BackdropPath}");

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    Href(sb, $"https://www.imdb.com/title/{info.ImdbId}/");
                    Header(sb, 3, $"{info.Title} {info.ReleaseDate?.ToString("yyyy") ?? string.Empty}");
                    EndTag(sb, "a");

                    if (info.Genres.Any())
                    {
                        AddParagraph(sb,
                            $"Genre: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, info.Overview);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error(
                        "Exception when trying to process a Movie, either in getting the metadata from Plex OR getting the information from TheMovieDB, Plex GUID = {0}",
                        plexGUID);
                }
                finally
                {
                    EndLoopHtml(sb);
                }

            }
            sb.Append("</table><br /><br />");
        }

        private void GenerateMovieHtml(List<Metadata> movies, PlexSettings plexSettings, StringBuilder sb)
        {
            var orderedMovies = movies.OrderByDescending(x => x?.addedAt.UnixTimeStampToDateTime()).ToList() ?? new List<Metadata>();
            sb.Append("<h1>New Movies:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var movie in orderedMovies)
            {
                var plexGUID = string.Empty;
                try
                {
                    var metaData = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                        movie.ratingKey.ToString());

                    plexGUID = metaData.Video.Guid;

                    var imdbId = PlexHelper.GetProviderIdFromPlexGuid(plexGUID);
                    var info = _movieApi.GetMovieInformation(imdbId).Result;
                    if (info == null)
                    {
                        throw new Exception($"Movie with Imdb id {imdbId} returned null from the MovieApi");
                    }
                    AddImageInsideTable(sb, $"https://image.tmdb.org/t/p/w500{info.BackdropPath}");

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    Href(sb, $"https://www.imdb.com/title/{info.ImdbId}/");
                    Header(sb, 3, $"{info.Title} {info.ReleaseDate?.ToString("yyyy") ?? string.Empty}");
                    EndTag(sb, "a");

                    if (info.Genres.Any())
                    {
                        AddParagraph(sb,
                            $"Genre: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, info.Overview);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error(
                        "Exception when trying to process a Movie, either in getting the metadata from Plex OR getting the information from TheMovieDB, Plex GUID = {0}",
                        plexGUID);
                }
                finally
                {
                    EndLoopHtml(sb);
                }

            }
            sb.Append("</table><br /><br />");
        }

        private void GenerateTvHtml(List<RecentlyAddedChild> tv, PlexSettings plexSettings, StringBuilder sb)
        {
           var orderedTv = tv.OrderByDescending(x => x?.addedAt.UnixTimeStampToDateTime()).ToList();
            // TV
            sb.Append("<h1>New Episodes:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var t in orderedTv)
            {
                var plexGUID = string.Empty;
                try
                {

                    var parentMetaData = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                        t.parentRatingKey.ToString());

                    plexGUID = parentMetaData.Directory.Guid;

                    var info = TvApi.ShowLookupByTheTvDbId(int.Parse(PlexHelper.GetProviderIdFromPlexGuid(plexGUID)));

                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https"); // Always use the Https banners
                    }
                    AddImageInsideTable(sb, banner);

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    var title = $"{t.grandparentTitle} - {t.title}  {t.originallyAvailableAt?.Substring(0, 4)}";

                    Href(sb, $"https://www.imdb.com/title/{info.externals.imdb}/");
                    Header(sb, 3, title);
                    EndTag(sb, "a");

                    AddParagraph(sb, $"Season: {t.parentIndex}, Episode: {t.index}");
                    if (info.genres.Any())
                    {
                        AddParagraph(sb, $"Genre: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, string.IsNullOrEmpty(t.summary) ? info.summary : t.summary);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error(
                        "Exception when trying to process a TV Show, either in getting the metadata from Plex OR getting the information from TVMaze, Plex GUID = {0}",
                        plexGUID);
                }
                finally
                {
                    EndLoopHtml(sb);
                }
            }
            sb.Append("</table><br /><br />");
        }

        private void GenerateTvHtml(List<Metadata> tv, PlexSettings plexSettings, StringBuilder sb)
        {
            var orderedTv = tv.OrderByDescending(x => x?.addedAt.UnixTimeStampToDateTime()).ToList();
            // TV
            sb.Append("<h1>New Episodes:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var t in orderedTv)
            {
                var plexGUID = string.Empty;
                try
                {

                    var parentMetaData = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                        t.parentRatingKey.ToString());

                    plexGUID = parentMetaData.Directory.Guid;

                    var info = TvApi.ShowLookupByTheTvDbId(int.Parse(PlexHelper.GetProviderIdFromPlexGuid(plexGUID)));

                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https"); // Always use the Https banners
                    }
                    AddImageInsideTable(sb, banner);

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    var title = $"{t.grandparentTitle} - {t.title}  {t.originallyAvailableAt?.Substring(0, 4)}";

                    Href(sb, $"https://www.imdb.com/title/{info.externals.imdb}/");
                    Header(sb, 3, title);
                    EndTag(sb, "a");

                    AddParagraph(sb, $"Season: {t.parentIndex}, Episode: {t.index}");
                    if (info.genres.Any())
                    {
                        AddParagraph(sb, $"Genre: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, string.IsNullOrEmpty(t.summary) ? info.summary : t.summary);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error(
                        "Exception when trying to process a TV Show, either in getting the metadata from Plex OR getting the information from TVMaze, Plex GUID = {0}",
                        plexGUID);
                }
                finally
                {
                    EndLoopHtml(sb);
                }
            }
            sb.Append("</table><br /><br />");
        }

        private void Send(NewletterSettings newletterSettings, string html, PlexSettings plexSettings, bool testEmail = false, string subject = "New Content on Plex!")
        {
            Log.Debug("Entering Send");
            var settings = EmailSettings.GetSettings();

            if (!settings.Enabled || string.IsNullOrEmpty(settings.EmailHost))
            {
                return;
            }

            var body = new BodyBuilder { HtmlBody = html, TextBody = "This email is only available on devices that support HTML." };

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = subject
            };
            Log.Debug("Created Plain/HTML MIME body");

            if (!testEmail)
            {
                var users = UserHelper.GetUsersWithFeature(Features.Newsletter);
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        if (!string.IsNullOrEmpty(user.EmailAddress))
                        {
                            message.Bcc.Add(new MailboxAddress(user.Username, user.EmailAddress));
                        }
                    }
                }

                if (newletterSettings.CustomUsersEmailAddresses != null
                        && newletterSettings.CustomUsersEmailAddresses.Any())
                {
                    foreach (var user in newletterSettings.CustomUsersEmailAddresses)
                    {
                        if (!string.IsNullOrEmpty(user))
                        {
                            message.Bcc.Add(new MailboxAddress(user, user));
                        }
                    }
                }
            }

            message.Bcc.Add(new MailboxAddress(settings.EmailUsername, settings.RecipientEmail)); // Include the admin

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
                    Log.Debug("Sending");
                    client.Send(message);
                    Log.Debug("Sent");
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void EndLoopHtml(StringBuilder sb)
        {
            //NOTE: BR have to be in TD's as per html spec or it will be put outside of the table...
            //Source: http://stackoverflow.com/questions/6588638/phantom-br-tag-rendered-by-browsers-prior-to-table-tag
            sb.Append("<hr />");
            sb.Append("<br />");
            sb.Append("<br />");
            sb.Append("</td>");
            sb.Append("</tr>");
        }

    }
}