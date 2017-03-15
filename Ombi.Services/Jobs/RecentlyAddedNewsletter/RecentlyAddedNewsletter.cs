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

namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public class RecentlyAddedNewsletter : HtmlTemplateGenerator, IJob, IRecentlyAdded, IMassEmail
    {
        public RecentlyAddedNewsletter(IPlexApi api, ISettingsService<PlexSettings> plexSettings,
            ISettingsService<EmailNotificationSettings> email, IJobRecord rec,
               ISettingsService<NewletterSettings> newsletter,
             IUserHelper userHelper, IEmbyAddedNewsletter embyNews,
            ISettingsService<EmbySettings> embyS,
            IPlexNewsletter plex)
        {
            JobRecord = rec;
            Api = api;
            PlexSettings = plexSettings;
            EmailSettings = email;
            NewsletterSettings = newsletter;
            UserHelper = userHelper;
            EmbyNewsletter = embyNews;
            EmbySettings = embyS;
            PlexNewsletter = plex;
        }

        private IPlexApi Api { get; }
        private TvMazeApi TvApi = new TvMazeApi();
        private readonly TheMovieDbApi _movieApi = new TheMovieDbApi();
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<NewletterSettings> NewsletterSettings { get; }
        private IJobRecord JobRecord { get; }
        private IUserHelper UserHelper { get; }
        private IEmbyAddedNewsletter EmbyNewsletter { get; }
        private IPlexNewsletter PlexNewsletter { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void StartNewsLetter()
        {
            try
            {
                var settings = NewsletterSettings.GetSettings();
                if (!settings.SendRecentlyAddedEmail)
                {
                    return;
                }
                JobRecord.SetRunning(true, JobNames.RecentlyAddedEmail);
                StartNewsLetter(settings);
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
            StartNewsLetter();
        }

        public void RecentlyAddedAdminTest()
        {
            Log.Debug("Starting Recently Added Newsletter Test");
            var settings = NewsletterSettings.GetSettings();
            StartNewsLetter(settings, true);
        }

        public void MassEmailAdminTest(string html, string subject)
        {
            Log.Debug("Starting Mass Email Test");
            var template = new MassEmailTemplate();
            var body = template.LoadTemplate(html);
            SendMassEmail(body, subject, true);
        }

        public void SendMassEmail(string html, string subject)
        {
            Log.Debug("Starting Mass Email Test");
            var template = new MassEmailTemplate();
            var body = template.LoadTemplate(html);
            SendMassEmail(body, subject, false);
        }

        private void StartNewsLetter(NewletterSettings newletterSettings, bool testEmail = false)
        {
            var embySettings = EmbySettings.GetSettings();
            if (embySettings.Enable)
            {
                var html = EmbyNewsletter.GetNewsletterHtml(testEmail);

                var escapedHtml = new string(html.Where(c => !char.IsControl(c)).ToArray());
                Log.Debug(escapedHtml);
                SendNewsletter(newletterSettings, escapedHtml, testEmail, "New Content On Emby!");
            }
            else
            {
                var plexSettings = PlexSettings.GetSettings();
                if (plexSettings.Enable)
                {
                    var html = PlexNewsletter.GetNewsletterHtml(testEmail);

                    var escapedHtml = new string(html.Where(c => !char.IsControl(c)).ToArray());
                    Log.Debug(escapedHtml);
                    SendNewsletter(newletterSettings, html, testEmail);
                }
            }
        }

        private void SendMassEmail(string html, string subject, bool testEmail)
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
                Subject = subject
            };
            Log.Debug("Created Plain/HTML MIME body");

            if (!testEmail)
            {
                var users = UserHelper.GetUsers(); // Get all users
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        if (!string.IsNullOrEmpty(user.EmailAddress))
                        {
                            message.Bcc.Add(new MailboxAddress(user.Username, user.EmailAddress)); // BCC everyone
                        }
                    }
                }
            }
            message.Bcc.Add(new MailboxAddress(settings.EmailUsername, settings.RecipientEmail)); // Include the admin

            message.From.Add(new MailboxAddress(settings.EmailUsername, settings.EmailSender));
            SendMail(settings, message);
        }

       
        private void SendNewsletter(NewletterSettings newletterSettings, string html, bool testEmail = false, string subject = "New Content on Plex!")
        {
            Log.Debug("Entering SendNewsletter");
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
            SendMail(settings, message);
        }

        private void SendMail(EmailNotificationSettings settings, MimeMessage message)
        {
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
    }
}