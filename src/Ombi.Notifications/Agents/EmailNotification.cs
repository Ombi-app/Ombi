using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
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
        public EmailNotification(ISettingsService<EmailNotificationSettings> settings, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t, IEmailProvider prov, ISettingsService<CustomizationSettings> c) : base(settings, r, m, t)
        {
            EmailProvider = prov;
            CustomizationSettings = c;
        }
        private IEmailProvider EmailProvider { get; }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }
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

            var customization = await CustomizationSettings.GetSettingsAsync();

            var email = new EmailBasicTemplate();
            var html = email.LoadTemplate(parsed.Subject, parsed.Message,parsed.Image, customization.Logo);
            

            var message = new NotificationMessage
            {
                Message = html,
                Subject = parsed.Subject,
                To = settings.AdminEmail,
            };

            return message;
        }



        protected override async Task NewRequest(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.NewRequest, model, settings);
            if (message == null)
            {
                return;
            }

            //message.Other.Add("PlainTextBody", $"Hello! The user '{model.RequestedUser}' has requested the {model.RequestType} '{model.Title}'! Please log in to approve this request. Request Date: {model.DateTime:f}");

            await Send(message, settings);
        }

        protected override async Task Issue(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.Issue, model, settings);
            if (message == null)
            {
                return;
            }

            //message.Other.Add("PlainTextBody", $"Hello! The user '{model.RequestedUser}' has reported a new issue {model.Body} for the title {model.Title}!");

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
                img = MovieRequest.PosterPath;
            }
            else
            {
                user = TvRequest.RequestedUser.UserAlias;
                title = TvRequest.ParentRequest.Title;
                img = TvRequest.ParentRequest.PosterPath;
            }

            var customization = await CustomizationSettings.GetSettingsAsync();
            var html = email.LoadTemplate(
                "Ombi: A request could not be added.",
                $"Hello! The user '{user}' has requested {title} but it could not be added. This has been added into the requests queue and will keep retrying", img, customization.Logo);

            var message = new NotificationMessage
            {
                Message = html,
                Subject = $"Ombi: A request could not be added",
                To = settings.AdminEmail,
            };

            //message.Other.Add("PlainTextBody", $"Hello! The user '{model.RequestedUser}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying");


            await Send(message, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestDeclined, model, settings);
            if (message == null)
            {
                return;
            }

            //message.Other.Add("PlainTextBody", $"Hello! Your request for {model.Title} has been declined, Sorry!");


            await Send(message, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestApproved, model, settings);
            if (message == null)
            {
                return;
            }

            //message.Other.Add("PlainTextBody", $"Hello! Your request for {model.Title} has been approved!");

            await Send(message, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestAvailable, model, settings);
            if (message == null)
            {
                return;
            }

            //message.Other.Add("PlainTextBody", $"Hello! You requested {model.Title} on Ombi! This is now available on Plex! :)");

            await Send(message, settings);
        }

        protected override async Task Send(NotificationMessage model, EmailNotificationSettings settings)
        {
            await EmailProvider.Send(model, settings);
        }

        protected override async Task Test(NotificationOptions model, EmailNotificationSettings settings)
        {
            var email = new EmailBasicTemplate();
            var customization = await CustomizationSettings.GetSettingsAsync();
            var html = email.LoadTemplate(
                "Test Message",
                "This is just a test! Success!", "", customization.Logo);
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
