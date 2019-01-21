using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Pushbullet;
using Ombi.Api.Pushover;
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
    public class PushoverNotification : BaseNotification<PushoverSettings>, IPushoverNotification
    {
        public PushoverNotification(IPushoverApi api, ISettingsService<PushoverSettings> sn, ILogger<PushoverNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "PushoverNotification";

        private IPushoverApi Api { get; }
        private ILogger<PushoverNotification> Logger { get; }

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
            await Run(model, settings, NotificationType.NewRequest);
        }

        protected override async Task NewIssue(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, PushoverSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, PushoverSettings settings)
        {
            try
            {
                //&+' < >
                await Api.PushAsync(settings.AccessToken, model.Message.StripCharacters('&','+','<','>'), settings.UserToken, settings.Priority, settings.Sound);
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

        private async Task Run(NotificationOptions model, PushoverSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Pushover, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Pushover}");
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
