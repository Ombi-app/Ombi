using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Webhook;
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
    public class WebhookNotification : BaseNotification<WebhookSettings>, IWebhookNotification
    {
        public WebhookNotification(IWebhookApi api, ISettingsService<WebhookSettings> sn, ILogger<WebhookNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "WebhookNotification";

        private IWebhookApi Api { get; }
        private ILogger<WebhookNotification> Logger { get; }

        protected override bool ValidateConfiguration(WebhookSettings settings)
        {
            return settings.Enabled && !string.IsNullOrEmpty(settings.WebhookUrl);
        }

        protected override async Task NewRequest(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }


        protected override async Task NewIssue(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, WebhookSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, WebhookSettings settings)
        {
            try
            {
                await Api.PushAsync(settings.WebhookUrl, settings.ApplicationToken, model.Data);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.WebhookNotification, e, "Failed to send webhook notification");
            }
        }

        protected override async Task Test(NotificationOptions model, WebhookSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        private async Task Run(NotificationOptions model, WebhookSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Webhook, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Webhook}");
                return;
            }

            var notification = new NotificationMessage
            {
                Data = parsed.Data,
            };

            await Send(notification, settings);
        }
    }
}
