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
using System.Threading.Tasks;
using MimeKit;
using NLog;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.Notification.Templates;
using Ombi.Core.SettingModels;
using Ombi.Services.Interfaces;
using Ombi.Store;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Ombi.Services.Notification
{
    public class EmailMessageNotification : INotification
    {
        public EmailMessageNotification(ISettingsService<EmailNotificationSettings> settings)
        {
            EmailNotificationSettings = settings;
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ISettingsService<EmailNotificationSettings> EmailNotificationSettings { get; }
        public string NotificationName => "EmailMessageNotification";

        public async Task NotifyAsync(NotificationModel model)
        {
            var configuration = GetConfiguration();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationModel model, Settings settings)
        {
            if (settings == null) await NotifyAsync(model);

            var emailSettings = (EmailNotificationSettings)settings;

            if (!ValidateConfiguration(emailSettings))
            {
                return;
            }

            switch (model.NotificationType)
            {
                case NotificationType.NewRequest:
                    await EmailNewRequest(model, emailSettings);
                    break;
                case NotificationType.Issue:
                    await EmailIssue(model, emailSettings);
                    break;
                case NotificationType.RequestAvailable:
                    await EmailAvailableRequest(model, emailSettings);
                    break;
                case NotificationType.RequestApproved:
                    await EmailRequestApproved(model, emailSettings);
                    break;
                case NotificationType.AdminNote:
                    throw new NotImplementedException();

                case NotificationType.Test:
                    await EmailTest(model, emailSettings);
                    break;
                case NotificationType.RequestDeclined:
                    await EmailRequestDeclined(model, emailSettings);
                    break;
                case NotificationType.ItemAddedToFaultQueue:
                    await EmailAddedToRequestQueue(model, emailSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private EmailNotificationSettings GetConfiguration()
        {
            var settings = EmailNotificationSettings.GetSettings();
            return settings;
        }

        private bool ValidateConfiguration(EmailNotificationSettings settings)
        {
            if (settings.Authentication)
            {
                if (string.IsNullOrEmpty(settings.EmailUsername) || string.IsNullOrEmpty(settings.EmailPassword))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(settings.EmailHost) || string.IsNullOrEmpty(settings.RecipientEmail) || string.IsNullOrEmpty(settings.EmailPort.ToString()))
            {
                return false;
            }

            if (!settings.EnableUserEmailNotifications)
            {
                if (!settings.Enabled)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task EmailNewRequest(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                $"Plex Requests: New {model.RequestType.GetString()?.ToLower()} request for {model.Title}!",
                $"Hello! The user '{model.User}' has requested the {model.RequestType.GetString()?.ToLower()} '{model.Title}'! Please log in to approve this request. Request Date: {model.DateTime.ToString("f")}",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, TextBody = $"Hello! The user '{model.User}' has requested the {model.RequestType.GetString()?.ToLower()} '{model.Title}'! Please log in to approve this request. Request Date: {model.DateTime.ToString("f")}" };

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = $"Plex Requests: New {model.RequestType.GetString()?.ToLower()} request for {model.Title}!"
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(settings.RecipientEmail, settings.RecipientEmail));


            await Send(message, settings);
        }

        private async Task EmailIssue(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                $"Plex Requests: New issue for {model.Title}!",
                $"Hello! The user '{model.User}' has reported a new issue {model.Body} for the title {model.Title}!",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, TextBody = $"Hello! The user '{model.User}' has reported a new issue {model.Body} for the title {model.Title}!"};

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = $"Plex Requests: New issue for {model.Title}!"
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(settings.RecipientEmail, settings.RecipientEmail));


            await Send(message, settings);
        }

        private async Task EmailAddedToRequestQueue(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Plex Requests: A request could not be added.",
                $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, TextBody = $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying" };

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = $"Plex Requests: A request could not be added"
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(settings.RecipientEmail, settings.RecipientEmail));


            await Send(message, settings);
        }

        private async Task EmailRequestDeclined(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Plex Requests: Your request has been declined",
                $"Hello! Your request for {model.Title} has been declined, Sorry!",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, TextBody = $"Hello! Your request for {model.Title} has been declined, Sorry!", };

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = $"Plex Requests: Your request has been declined"
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(model.UserEmail, model.UserEmail));


            await Send(message, settings);
        }

        private async Task EmailRequestApproved(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Plex Requests: Your request has been approved!",
                $"Hello! Your request for {model.Title} has been approved!",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, TextBody = $"Hello! Your request for {model.Title} has been approved!", };

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = $"Plex Requests: Your request has been approved!"
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(model.UserEmail, model.UserEmail));


            await Send(message, settings);
        }

        private async Task EmailAvailableRequest(NotificationModel model, EmailNotificationSettings settings)
        {
            if (!settings.EnableUserEmailNotifications)
            {
                await Task.FromResult(false);
            }
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                $"Plex Requests: {model.Title} is now available!",
                $"Hello! You requested {model.Title} on PlexRequests! This is now available on Plex! :)",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, TextBody = $"Hello! You requested {model.Title} on PlexRequests! This is now available on Plex! :)" };

            var message = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = $"Plex Requests: {model.Title} is now available!"
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(model.UserEmail, model.UserEmail));

            await Send(message, settings);
        }

        private async Task Send(MimeMessage message, EmailNotificationSettings settings)
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
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async Task EmailTest(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Test Message",
                "This is just a test! Success!",
                model.ImgSrc);
            var body = new BodyBuilder { HtmlBody = html, };
            var message = new MimeMessage
            {
                Body = body.ToMessageBody()
            };
            message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
            message.To.Add(new MailboxAddress(settings.RecipientEmail, settings.RecipientEmail));

            await Send(message, settings);
        }
    }
}