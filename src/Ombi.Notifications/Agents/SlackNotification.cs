using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Slack;
using Ombi.Api.Slack.Models;
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
    public class SlackNotification : BaseNotification<SlackNotificationSettings>, ISlackNotification
    {
        public SlackNotification(ISlackApi api, ISettingsService<SlackNotificationSettings> sn, ILogger<SlackNotification> log, INotificationTemplatesRepository r, IMovieRequestRepository m, ITvRequestRepository t,
            ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub) : base(sn, r, m, t, s, log, sub)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "SlackNotification";

        private ISlackApi Api { get; }
        private ILogger<SlackNotification> Logger { get; }

        protected override bool ValidateConfiguration(SlackNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.WebhookUrl))
            {
                return false;
            }
            try
            {
                var a = settings.Token;
                var b = settings.Team;
                var c = settings.Service;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        protected override async Task NewRequest(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.NewRequest, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.NewRequest} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task NewIssue(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.Issue, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.Issue} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task IssueComment(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.IssueComment, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.IssueComment} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task IssueResolved(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.IssueResolved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.IssueResolved} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, SlackNotificationSettings settings)
        {
            var user = string.Empty;
            var title = string.Empty;
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

        protected override async Task RequestDeclined(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.RequestDeclined, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestDeclined} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.RequestApproved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestApproved} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, SlackNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Slack, NotificationType.RequestAvailable, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestAvailable} is disabled for {NotificationAgent.Slack}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, SlackNotificationSettings settings)
        {
            try
            {
                var body = new SlackNotificationBody
                {
                    channel = settings.Channel,
                    icon_emoji = settings.IconEmoji,
                    icon_url = settings.IconUrl,
                    text = model.Message,
                    username = settings.Username
                };

                await Api.PushAsync(settings.Team, settings.Token, settings.Service, body);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.SlackNotification, e, "Failed to send Slack Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, SlackNotificationSettings settings)
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
