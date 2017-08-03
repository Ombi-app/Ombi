using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Pushbullet;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Interfaces;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Agents
{
    public class PushbulletNotification : BaseNotification<PushbulletSettings>, IPushbulletNotification
    {
        public PushbulletNotification(IPushbulletApi api, ISettingsService<PushbulletSettings> sn, ILogger<PushbulletNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t) : base(sn, r, m, t)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "PushbulletNotification";

        private IPushbulletApi Api { get; }
        private ILogger<PushbulletNotification> Logger { get; }

        protected override bool ValidateConfiguration(PushbulletSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.AccessToken))
            {
                return false;
            }
           
            return true;
        }

        protected override async Task NewRequest(NotificationOptions model, PushbulletSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.NewRequest, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            
            await Send(notification, settings);
        }

        protected override async Task Issue(NotificationOptions model, PushbulletSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.Issue, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, PushbulletSettings settings)
        {
            string user;
            string title;
            if (model.RequestType == RequestType.Movie)
            {
                user = MovieRequest.RequestedUser.UserAlias;
                title = MovieRequest.Title;
            }
            else
            {
                user = TvRequest.RequestedUser.UserAlias;
                title = TvRequest.ParentRequest.Title;
            }
            var message = $"Hello! The user '{user}' has requested {title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var notification = new NotificationMessage
            {
                Message = message
            };
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, PushbulletSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.RequestDeclined, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, PushbulletSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.RequestApproved, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, PushbulletSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.RequestAvailable, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, PushbulletSettings settings)
        {
            try
            {
                await Api.Push(settings.AccessToken, model.Subject, model.Message, settings.ChannelTag);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.DiscordNotification, e, "Failed to send Pushbullet Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, PushbulletSettings settings)
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
