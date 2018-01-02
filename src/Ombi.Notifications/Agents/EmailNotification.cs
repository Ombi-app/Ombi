using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Interfaces;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Agents
{
    public class EmailNotification : BaseNotification<EmailNotificationSettings>, IEmailNotification
    {
        public EmailNotification(ISettingsService<EmailNotificationSettings> settings, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t, IEmailProvider prov, ISettingsService<CustomizationSettings> c,
            ILogger<EmailNotification> log) : base(settings, r, m, t, c)
        {
            EmailProvider = prov;
            Logger = log;
        }
        private IEmailProvider EmailProvider { get; }
        private ILogger<EmailNotification> Logger { get; }
        public override string NotificationName => nameof(EmailNotification);

        protected override bool ValidateConfiguration(EmailNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (settings.Authentication)
            {
                if (string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(settings.Host) || string.IsNullOrEmpty(settings.AdminEmail) || string.IsNullOrEmpty(settings.Port.ToString()))
            {
                return false;
            }

            return true;
        }
        
        private async Task<NotificationMessage> LoadTemplate(NotificationType type, NotificationOptions model, EmailNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Email, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Email}");
                return null;
            }
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(parsed.Subject, parsed.Message,parsed.Image, Customization.Logo);
            

            var message = new NotificationMessage
            {
                Message = html,
                Subject = parsed.Subject,
                To = model.Recipient.HasValue() ? model.Recipient : settings.AdminEmail,
            };

            return message;
        }

        private async Task<string> LoadMessage(NotificationType type, NotificationOptions model, EmailNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Email, type, model);

            return parsed.Message;
        }

        protected override async Task NewRequest(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.NewRequest, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadMessage(NotificationType.NewRequest, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            await Send(message, settings);
        }

        protected override async Task Issue(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.Issue, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadMessage(NotificationType.Issue, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            // Issues should be sent to admin
            message.To = settings.AdminEmail;

            await Send(message, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var user = string.Empty;
            var title = string.Empty;
            var img = string.Empty;
            if (model.RequestType == RequestType.Movie)
            {
                user = MovieRequest.RequestedUser.UserAlias;
                title = MovieRequest.Title;
                img = $"https://image.tmdb.org/t/p/w300/{MovieRequest.PosterPath}";
            }
            else
            {
                user = TvRequest.RequestedUser.UserAlias;
                title = TvRequest.ParentRequest.Title;
                img = TvRequest.ParentRequest.PosterPath;
            }

            var html = email.LoadTemplate(
                $"{Customization.ApplicationName}: A request could not be added.",
                $"Hello! The user '{user}' has requested {title} but it could not be added. This has been added into the requests queue and will keep retrying", img, Customization.Logo);

            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"{Customization.ApplicationName}: A request could not be added",
                To = settings.AdminEmail,
            };

            var plaintext = $"Hello! The user '{user}' has requested {title} but it could not be added. This has been added into the requests queue and will keep retrying";
            message.Other.Add("PlainTextBody", plaintext);

            await Send(message, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestDeclined, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadMessage(NotificationType.RequestDeclined, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            message.To = model.RequestType == RequestType.Movie
                ? MovieRequest.RequestedUser.Email
                : TvRequest.RequestedUser.Email;
            await Send(message, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestApproved, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadMessage(NotificationType.RequestApproved, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            message.To = model.RequestType == RequestType.Movie
                ? MovieRequest.RequestedUser.Email
                : TvRequest.RequestedUser.Email;
            await Send(message, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestAvailable, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadMessage(NotificationType.RequestAvailable, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            message.To = model.RequestType == RequestType.Movie
                ? MovieRequest.RequestedUser.Email
                : TvRequest.RequestedUser.Email;
            await Send(message, settings);
        }

        protected override async Task Send(NotificationMessage model, EmailNotificationSettings settings)
        {
            await EmailProvider.Send(model, settings);
        }

        protected override async Task Test(NotificationOptions model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(
                "Test Message",
                "This is just a test! Success!", "", Customization.Logo);
            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: Test",
                To = settings.AdminEmail,
            };

            message.Other.Add("PlainTextBody", "This is just a test! Success!");

            await Send(message, settings);
        }
    }
}
