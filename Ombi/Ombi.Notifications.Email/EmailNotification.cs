using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Ombi.Core.Models.Requests;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.Notifications;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;

namespace Ombi.Notifications.Email
{
    public class EmailNotification : BaseNotification<EmailNotificationSettings>
    {
        public EmailNotification(ISettingsService<EmailNotificationSettings> settings) : base(settings)
        {
        }

        public override string NotificationName => nameof(EmailNotification);

        protected override bool ValidateConfiguration(EmailNotificationSettings settings)
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

            return true;
        }

        protected override async Task NewRequest(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                $"Ombi: New {model.RequestType.GetString()?.ToLower()} request for {model.Title}!",
                $"Hello! The user '{model.User}' has requested the {model.RequestType.GetString()?.ToLower()} '{model.Title}'! Please log in to approve this request. Request Date: {model.DateTime:f}",
                model.ImgSrc);


            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: New {model.RequestType.GetString()?.ToLower()} request for {model.Title}!",
                To = settings.RecipientEmail,
            };

            message.Other.Add("PlainTextBody", $"Hello! The user '{model.User}' has requested the {model.RequestType.GetString()?.ToLower()} '{model.Title}'! Please log in to approve this request. Request Date: {model.DateTime:f}");

            await Send(message, settings);
        }

        protected override async Task Issue(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                $"Ombi: New issue for {model.Title}!",
                $"Hello! The user '{model.User}' has reported a new issue {model.Body} for the title {model.Title}!",
                model.ImgSrc);

            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: New issue for {model.Title}!",
                To = settings.RecipientEmail,
            };

            message.Other.Add("PlainTextBody", $"Hello! The user '{model.User}' has reported a new issue {model.Body} for the title {model.Title}!");

            await Send(message, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Ombi: A request could not be added.",
                $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying",
                model.ImgSrc);

            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: A request could not be added",
                To = settings.RecipientEmail,
            };

            message.Other.Add("PlainTextBody", $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying");


            await Send(message, settings);
        }

        protected override async Task RequestDeclined(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Ombi: Your request has been declined",
                $"Hello! Your request for {model.Title} has been declined, Sorry!",
                model.ImgSrc);

            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: Your request has been declined",
                To = model.UserEmail,
            };

            message.Other.Add("PlainTextBody", $"Hello! Your request for {model.Title} has been declined, Sorry!");


            await Send(message, settings);
        }

        protected override async Task RequestApproved(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Ombi: Your request has been approved!",
                $"Hello! Your request for {model.Title} has been approved!",
                model.ImgSrc);

            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: Your request has been approved!",
                To = model.UserEmail,
            };

            message.Other.Add("PlainTextBody", $"Hello! Your request for {model.Title} has been approved!");

            await Send(message, settings);
        }

        protected override async Task AvailableRequest(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                $"Ombi: {model.Title} is now available!",
                $"Hello! You requested {model.Title} on Ombi! This is now available on Plex! :)",
                model.ImgSrc);


            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: {model.Title} is now available!",
                To = model.UserEmail,
            };

            message.Other.Add("PlainTextBody", $"Hello! You requested {model.Title} on Ombi! This is now available on Plex! :)");

            await Send(message, settings);
        }

        protected override async Task Send(NotificationMessage model, EmailNotificationSettings settings)
        {
            try
            {
                var body = new BodyBuilder
                {
                    HtmlBody = model.Message,
                    TextBody = model.Other["PlainTextBody"]
                };

                var message = new MimeMessage
                {
                    Body = body.ToMessageBody(),
                    Subject = model.Subject
                };
                message.From.Add(new MailboxAddress(settings.EmailSender, settings.EmailSender));
                message.To.Add(new MailboxAddress(model.To, model.To));

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
                    //Log.Info("sending message to {0} \r\n from: {1}\r\n Are we authenticated: {2}", message.To, message.From, client.IsAuthenticated);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                //Log.Error(e);
                throw new InvalidOperationException(e.Message);
            }
        }

        protected override async Task Test(NotificationModel model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Test Message",
                "This is just a test! Success!",
                model.ImgSrc);
            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: Test",
                To = settings.RecipientEmail,
            };

            message.Other.Add("PlainTextBody", "This is just a test! Success!");

            await Send(message, settings);
        }
    }
}
