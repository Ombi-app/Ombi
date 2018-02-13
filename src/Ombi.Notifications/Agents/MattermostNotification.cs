using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Discord;
using Ombi.Api.Discord.Models;
using Ombi.Api.Mattermost;
using Ombi.Api.Mattermost.Models;
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
    public class MattermostNotification : BaseNotification<MattermostNotificationSettings>, IMattermostNotification
    {
        public MattermostNotification(IMattermostApi api, ISettingsService<MattermostNotificationSettings> sn, ILogger<MattermostNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s) : base(sn, r, m, t,s,log)
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
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.NewRequest, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.NewRequest} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task NewIssue(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.Issue, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.Issue} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task IssueComment(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.IssueComment, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.IssueComment} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task IssueResolved(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.IssueResolved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.IssueResolved} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, MattermostNotificationSettings settings)
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
            notification.Other.Add("image", image);
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.RequestDeclined, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestDeclined} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.RequestApproved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestApproved} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, MattermostNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mattermost, NotificationType.RequestAvailable, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestAvailable} is disabled for {NotificationAgent.Mattermost}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, MattermostNotificationSettings settings)
        {
            try
            {
                var body = new MattermostBody
                {
                    username = string.IsNullOrEmpty(settings.Username) ? "Ombi" : settings.Username,
                    channel = settings.Channel,
                    text = model.Message,
                    icon_url = settings.IconUrl
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
    }
}
