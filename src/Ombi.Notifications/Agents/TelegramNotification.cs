using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Interfaces;
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
        public TelegramNotification(ITelegramApi api, ISettingsService<TelegramSettings> sn, ILogger<TelegramNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s) : base(sn, r, m, t,s)
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
            var parsed = await LoadTemplate(NotificationAgent.Telegram, NotificationType.NewRequest, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.NewRequest} is disabled for {NotificationAgent.Telegram}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            
            await Send(notification, settings);
        }

        protected override async Task Issue(NotificationOptions model, TelegramSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Telegram, NotificationType.Issue, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.Issue} is disabled for {NotificationAgent.Telegram}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, TelegramSettings settings)
        {
            var user = string.Empty;
            var title = string.Empty;
            var image = string.Empty;
            if (model.RequestType == RequestType.Movie)
            {
                user = MovieRequest.RequestedUser.UserAlias;
                title = MovieRequest.Title;
                image = MovieRequest.PosterPath;
            }
            else
            {
                user = TvRequest.RequestedUser.UserAlias;
                title = TvRequest.ParentRequest.Title;
                image = TvRequest.ParentRequest.PosterPath;
            }
            var message = $"Hello! The user '{user}' has requested {title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var notification = new NotificationMessage
            {
                Message = message
            };
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, TelegramSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Telegram, NotificationType.RequestDeclined, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestDeclined} is disabled for {NotificationAgent.Telegram}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, TelegramSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Telegram, NotificationType.RequestApproved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestApproved} is disabled for {NotificationAgent.Telegram}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message ?? string.Empty,
            };
            
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, TelegramSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Telegram, NotificationType.RequestAvailable, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestAvailable} is disabled for {NotificationAgent.Telegram}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
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
    }
}
