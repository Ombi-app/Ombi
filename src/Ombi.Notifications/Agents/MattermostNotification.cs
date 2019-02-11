using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Mattermost;
using Ombi.Api.Mattermost.Models;
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
    public class MattermostNotification : BaseNotification<MattermostNotificationSettings>, IMattermostNotification
    {
        public MattermostNotification(IMattermostApi api, ISettingsService<MattermostNotificationSettings> sn, ILogger<MattermostNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "MattermostNotification";

        private IMattermostApi Api { get; }
        private ILogger<MattermostNotification> Logger { get; }

        protected override bool ValidateConfiguration(MattermostNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.WebhookUrl))
            {
                return false;
            }

            return true;
        }

        protected override async Task NewRequest(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }

        private void AddOtherInformation(NotificationOptions model, NotificationMessage notification,
            NotificationMessageContent parsed)
        {
            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("title", model.RequestType == RequestType.Movie ? MovieRequest.Title : TvRequest.Title);
        }

        protected override async Task NewIssue(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, MattermostNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, MattermostNotificationSettings settings)
        {
            try
            {
                var body = new MattermostMessage
                {
                    Username = string.IsNullOrEmpty(settings.Username) ? "Ombi" : settings.Username,
                    Channel = settings.Channel,
                    Text = model.Message,
                    IconUrl = settings.IconUrl,
                    Attachments = new List<MattermostAttachment>
                    {
                        new MattermostAttachment
                        {
                            Title = model.Other.ContainsKey("title") ? model.Other["title"] : string.Empty,
                            ImageUrl = model.Other.ContainsKey("image") ? model.Other["image"] : string.Empty,
                        }
                    }
                };
                await Api.PushAsync(settings.WebhookUrl, body);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.MattermostNotification, e, "Failed to send Mattermost Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        private async Task Run(NotificationOptions model, MattermostNotificationSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            AddOtherInformation(model, notification, parsed);
            await Send(notification, settings);
        }
    }
}
