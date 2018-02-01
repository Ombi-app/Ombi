using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Notifications;
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
    public class MobileNotification : BaseNotification<MobileNotificationSettings>
    {
        public MobileNotification(IOneSignalApi api, ISettingsService<MobileNotificationSettings> sn, ILogger<MobileNotification> log, INotificationTemplatesRepository r,
            IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s, IRepository<NotificationUserId> notification,
            UserManager<OmbiUser> um) : base(sn, r, m, t, s)
        {
            _api = api;
            _logger = log;
            _notifications = notification;
            _userManager = um;
        }

        public override string NotificationName => "MobileNotification";

        private readonly IOneSignalApi _api;
        private readonly ILogger<MobileNotification> _logger;
        private readonly IRepository<NotificationUserId> _notifications;
        private readonly UserManager<OmbiUser> _userManager;

        protected override bool ValidateConfiguration(MobileNotificationSettings settings)
        {
            return false;
        }

        protected override async Task NewRequest(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.NewRequest, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.NewRequest} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            // Get admin devices
            var adminUsers = (await _userManager.GetUsersInRoleAsync(OmbiRoles.Admin)).Select(x => x.Id).ToList();
            var notificationUsers = _notifications.GetAll().Include(x => x.User).Where(x => adminUsers.Contains(x.UserId));
            var playerIds = await notificationUsers.Select(x => x.PlayerId).ToListAsync();
            await Send(playerIds, notification, settings);
        }

        protected override async Task NewIssue(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.Issue, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.Issue} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task IssueResolved(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.IssueResolved, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.IssueResolved} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, MobileNotificationSettings settings)
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

        protected override async Task RequestDeclined(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.RequestDeclined, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.RequestDeclined} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.RequestApproved, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.RequestApproved} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.RequestAvailable, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.RequestAvailable} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }
        protected override Task Send(NotificationMessage model, MobileNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected async Task Send(List<string> playerIds, NotificationMessage model, MobileNotificationSettings settings)
        {
            var response = await _api.PushNotification(playerIds, model.Message);
            _logger.LogDebug("Sent message to {0} recipients with message id {1}", response.recipients, response.id);
        }

        protected override async Task Test(NotificationOptions model, MobileNotificationSettings settings)
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