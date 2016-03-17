#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: EmailMessageNotification.cs
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
using System.Net;
using System.Net.Mail;

using NLog;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.Services.Notification
{
    public class EmailMessageNotification : INotification
    {
        public EmailMessageNotification(ISettingsService<EmailNotificationSettings> settings)
        {
            EmailNotificationSettings = settings;
        }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private ISettingsService<EmailNotificationSettings> EmailNotificationSettings { get; }
        public string NotificationName => "EmailMessageNotification";
        public bool Notify(string title, string requester)
        {
            var configuration = GetConfiguration();
            if (!ValidateConfiguration(configuration))
            {
                return false;
            }

            var message = new MailMessage
            {
                IsBodyHtml = true,
                To = { new MailAddress(configuration.RecipientEmail) },
                Body = $"User {requester} has requested {title}!",
                From = new MailAddress(configuration.EmailUsername),
                Subject = $"New Request for {title}!"
            };

            try
            {
                using (var smtp = new SmtpClient(configuration.EmailHost, configuration.EmailPort))
                {
                    smtp.Credentials = new NetworkCredential(configuration.EmailUsername, configuration.EmailPassword);
                    smtp.EnableSsl = configuration.Ssl;
                    smtp.Send(message);
                    return true;
                }
            }
            catch (SmtpException smtp)
            {
                Log.Fatal(smtp);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            return false;
        }

        private EmailNotificationSettings GetConfiguration()
        {
            var settings = EmailNotificationSettings.GetSettings();
            return settings;
        }

        private bool ValidateConfiguration(EmailNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.EmailHost) || string.IsNullOrEmpty(settings.EmailUsername)
                || string.IsNullOrEmpty(settings.EmailPassword) || string.IsNullOrEmpty(settings.RecipientEmail)
                || string.IsNullOrEmpty(settings.EmailPort.ToString()))
            {
                return false;
            }

            return true;
        }
    }
}