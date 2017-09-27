using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Pushbullet;
using Ombi.Api.Pushover;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Interfaces;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Agents
{
    public class PushoverNotification : BaseNotification<PushoverSettings>, IPushoverNotification
    {
        public PushoverNotification(IPushoverApi api, ISettingsService<PushoverSettings> sn, ILogger<PushbulletNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s) : base(sn, r, m, t, s)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "PushoverNotification";

        private IPushoverApi Api { get; }
        private ILogger<PushbulletNotification> Logger { get; }

        protected override bool ValidateConfiguration(PushoverSettings settings)
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

        protected override async Task NewRequest(NotificationOptions model, PushoverSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushover, NotificationType.NewRequest, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            
            await Send(notification, settings);
        }

        protected override async Task Issue(NotificationOptions model, PushoverSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushover, NotificationType.Issue, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, PushoverSettings settings)
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

        protected override async Task RequestDeclined(NotificationOptions model, PushoverSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushover, NotificationType.RequestDeclined, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, PushoverSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushover, NotificationType.RequestApproved, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, PushoverSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushover, NotificationType.RequestAvailable, model);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, PushoverSettings settings)
        {
            try
            {
                await Api.PushAsync(settings.AccessToken, model.Message, settings.UserToken);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.PushoverNotification, e, "Failed to send Pushover Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, PushoverSettings settings)
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
