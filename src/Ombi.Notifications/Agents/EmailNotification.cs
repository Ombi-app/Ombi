using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ombi.Core.Settings;
using Ombi.Helpers;
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
            ILogger<EmailNotification> log, UserManager<OmbiUser> um, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(settings, r, m, t, c, log, sub, music, userPref)
        {
            EmailProvider = prov;
            Logger = log;
            _userManager = um;
        }
        private IEmailProvider EmailProvider { get; }
        private ILogger<EmailNotification> Logger { get; }
        public override string NotificationName => nameof(EmailNotification);
        private readonly UserManager<OmbiUser> _userManager;

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
            var html = email.LoadTemplate(parsed.Subject, parsed.Message, parsed.Image, Customization.Logo);


            var message = new NotificationMessage
            {
                Message = html,
                Subject = parsed.Subject,
            };

            if (model.Substitutes.TryGetValue("AdminComment", out var isAdminString))
            {
                var isAdmin = bool.Parse(isAdminString);
                if (isAdmin)
                {
                    var user = _userManager.Users.FirstOrDefault(x => x.Id == model.UserId);
                    // Send to user
                    message.To = user.Email;
                }
                else
                {
                    // Send to admin
                    message.To = settings.AdminEmail;
                }
            }
            else
            {
                // Send to admin
                message.To = settings.AdminEmail;
            }

            return message;
        }

        private async Task<string> LoadPlainTextMessage(NotificationType type, NotificationOptions model, EmailNotificationSettings settings)
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

            var plaintext = await LoadPlainTextMessage(NotificationType.NewRequest, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            await Send(message, settings);
        }

        protected override async Task NewIssue(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.Issue, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadPlainTextMessage(NotificationType.Issue, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            // Issues should be sent to admin
            message.To = settings.AdminEmail;

            await Send(message, settings);
        }

        protected override async Task IssueComment(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.IssueComment, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadPlainTextMessage(NotificationType.IssueComment, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            if (model.Substitutes.TryGetValue("AdminComment", out var isAdminString))
            {
                var isAdmin = bool.Parse(isAdminString);
                message.To = isAdmin ? model.Recipient : settings.AdminEmail;
            }
            else
            {
                message.To = model.Recipient;
            }


            await Send(message, settings);
        }

        protected override async Task IssueResolved(NotificationOptions model, EmailNotificationSettings settings)
        {
            if (!model.Recipient.HasValue())
            {
                return;
            }
            var message = await LoadTemplate(NotificationType.IssueResolved, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadPlainTextMessage(NotificationType.IssueResolved, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            // Issues resolved should be sent to the user
            message.To = model.Recipient;

            await Send(message, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, EmailNotificationSettings settings)
        {
            if (!model.Recipient.HasValue())
            {
                return;
            }
            var message = await LoadTemplate(NotificationType.ItemAddedToFaultQueue, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadPlainTextMessage(NotificationType.ItemAddedToFaultQueue, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            // Issues resolved should be sent to the user
            message.To = settings.AdminEmail;
            await Send(message, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestDeclined, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadPlainTextMessage(NotificationType.RequestDeclined, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            await SendToSubscribers(settings, message);
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

            var plaintext = await LoadPlainTextMessage(NotificationType.RequestApproved, model, settings);
            message.Other.Add("PlainTextBody", plaintext);

            await SendToSubscribers(settings, message);

            message.To = model.RequestType == RequestType.Movie
                ? MovieRequest.RequestedUser.Email
                : TvRequest.RequestedUser.Email;
            await Send(message, settings);
        }

        private async Task SendToSubscribers(EmailNotificationSettings settings, NotificationMessage message)
        {
            if (await SubsribedUsers.AnyAsync())
            {
                foreach (var user in SubsribedUsers)
                {
                    if (user.Email.IsNullOrEmpty())
                    {
                        continue;
                    }

                    message.To = user.Email;

                    await Send(message, settings);
                }
            }
        }

        protected override async Task AvailableRequest(NotificationOptions model, EmailNotificationSettings settings)
        {
            var message = await LoadTemplate(NotificationType.RequestAvailable, model, settings);
            if (message == null)
            {
                return;
            }

            var plaintext = await LoadPlainTextMessage(NotificationType.RequestAvailable, model, settings);
            message.Other.Add("PlainTextBody", plaintext);
            await SendToSubscribers(settings, message);
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
