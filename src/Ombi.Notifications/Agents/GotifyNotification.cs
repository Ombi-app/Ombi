using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Gotify;
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
    public class GotifyNotification : BaseNotification<GotifySettings>, IGotifyNotification
    {
        public GotifyNotification(IGotifyApi api, ISettingsService<GotifySettings> sn, ILogger<GotifyNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "GotifyNotification";

        private IGotifyApi Api { get; }
        private ILogger<GotifyNotification> Logger { get; }

        protected override bool ValidateConfiguration(GotifySettings settings)
        {
            return settings.Enabled && !string.IsNullOrEmpty(settings.BaseUrl) && !string.IsNullOrEmpty(settings.ApplicationToken);
        }

        protected override async Task NewRequest(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }


        protected override async Task NewIssue(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, GotifySettings settings)
        {
            try
            {
                await Api.PushAsync(settings.BaseUrl, settings.ApplicationToken, model.Subject, model.Message, settings.Priority);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.GotifyNotification, e, "Failed to send Gotify notification");
            }
        }

        protected override async Task Test(NotificationOptions model, GotifySettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        private async Task Run(NotificationOptions model, GotifySettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Gotify, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Gotify}");
                return;
            }

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            await Send(notification, settings);
        }

        protected override async Task PartiallyAvailable(NotificationOptions model, GotifySettings settings)
        {
            await Run(model, settings, NotificationType.PartiallyAvailable);
        }
    }
}
