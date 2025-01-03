using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ombi.Api.Ntfy;
using Ombi.Api.Ntfy;
using Ombi.Api.Ntfy.Models;
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
    public class NtfyNotification : BaseNotification<NtfySettings>, INtfyNotification
    {
        public NtfyNotification(INtfyApi api, ISettingsService<NtfySettings> sn, ILogger<NtfyNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref, UserManager<OmbiUser> um) : base(sn, r, m, t, s, log, sub, music, userPref, um)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "NtfyNotification";

        private INtfyApi Api { get; }
        private ILogger<NtfyNotification> Logger { get; }

        protected override bool ValidateConfiguration(NtfySettings settings)
        {
            return settings.Enabled && !string.IsNullOrEmpty(settings.BaseUrl);
        }

        protected override async Task NewRequest(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }


        protected override async Task NewIssue(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, NtfySettings settings)
        {
            try
            {
                await Api.PushAsync(settings.BaseUrl, settings.AuthorizationHeader, new NtfyNotificationBody()
                {
                    topic = settings.Topic, // To change
                    title = model.Subject,
                    message = model.Message,
                    priority = settings.Priority
                });
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.NtfyNotification, e, "Failed to send Ntfy notification");
            }
        }

        protected override async Task Test(NotificationOptions model, NtfySettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        private async Task Run(NotificationOptions model, NtfySettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Ntfy, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Ntfy}");
                return;
            }

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            await Send(notification, settings);
        }

        protected override async Task PartiallyAvailable(NotificationOptions model, NtfySettings settings)
        {
            await Run(model, settings, NotificationType.PartiallyAvailable);
        }
    }
}
