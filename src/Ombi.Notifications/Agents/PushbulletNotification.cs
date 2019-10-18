using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Pushbullet;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Agents
{
    public class PushbulletNotification : BaseNotification<PushbulletSettings>, IPushbulletNotification
    {
        public PushbulletNotification(IPushbulletApi api, ISettingsService<PushbulletSettings> sn, ILogger<PushbulletNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref)
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
            await Run(model, settings, NotificationType.NewRequest);
        }


        protected override async Task NewIssue(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, PushbulletSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, PushbulletSettings settings)
        {
            try
            {
                await Api.Push(settings.AccessToken, model.Subject, model.Message, settings.ChannelTag);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.PushbulletNotification, e, "Failed to send Pushbullet Notification");
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

        private async Task Run(NotificationOptions model, PushbulletSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushbullet, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Pushbullet}");
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
