using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Api.Telegram;

namespace Ombi.Notifications.Agents
{
    public class TelegramNotification : BaseNotification<TelegramSettings>, ITelegramNotification
    {
        public TelegramNotification(ITelegramApi api, ISettingsService<TelegramSettings> sn, ILogger<TelegramNotification> log, 
                                    INotificationTemplatesRepository r, IMovieRequestRepository m, 
                                    ITvRequestRepository t, ISettingsService<CustomizationSettings> s
            , IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t,s,log, sub, music, userPref)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "TelegramNotification";

        private ITelegramApi Api { get; }
        private ILogger<TelegramNotification> Logger { get; }

        protected override bool ValidateConfiguration(TelegramSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            return !settings.BotApi.IsNullOrEmpty() && !settings.ChatId.IsNullOrEmpty();
        }

        protected override async Task NewRequest(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }

        protected override async Task NewIssue(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, TelegramSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, TelegramSettings settings)
        {
            try
            {
                await Api.Send(model.Message, settings.BotApi, settings.ChatId, settings.ParseMode);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.TelegramNotifcation, e, "Failed to send Telegram Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, TelegramSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        private async Task Run(NotificationOptions model, TelegramSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Telegram, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Telegram}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }
    }
}
