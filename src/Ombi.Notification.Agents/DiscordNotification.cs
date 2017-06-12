using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Discord;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Notification.Discord
{
    public class DiscordNotification : BaseNotification<DiscordNotificationSettings>
    {
        public DiscordNotification(IDiscordApi api, ISettingsService<DiscordNotificationSettings> sn, ILogger<DiscordNotification> log) : base(sn)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "DiscordNotification";

        private IDiscordApi Api { get; }
        private ILogger<DiscordNotification> Logger { get; }

        protected override bool ValidateConfiguration(DiscordNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.WebhookUrl))
            {
                return false;
            }
            try
            {
                var a = settings.Token;
                var b = settings.WebookId;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        protected override async Task NewRequest(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"{model.Title} has been requested by user: {model.User}";

            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task Issue(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! Your request for {model.Title} has been declined, Sorry!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! The request for {model.Title} has now been approved!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! The request for {model.Title} is now available!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, DiscordNotificationSettings settings)
        {
            try
            {
                await Api.SendMessage(model.Message, settings.WebookId, settings.Token, settings.Username);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.DiscordNotification, e, "Failed to send Discord Notification");
            }
        }

        protected override async Task Test(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }
    }
}
