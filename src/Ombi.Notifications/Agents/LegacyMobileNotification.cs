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
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Agents
{
    public class LegacyMobileNotification : BaseNotification<MobileNotificationSettings>, ILegacyMobileNotification
    {
        public LegacyMobileNotification(IOneSignalApi api, ISettingsService<MobileNotificationSettings> sn, ILogger<LegacyMobileNotification> log, INotificationTemplatesRepository r,
            IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s, IRepository<NotificationUserId> notification,
            UserManager<OmbiUser> um, IRepository<RequestSubscription> sub, IMusicRequestRepository music, IRepository<Issues> issueRepository,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref)
        {
            _api = api;
            _logger = log;
            _notifications = notification;
            _userManager = um;
            _issueRepository = issueRepository;
        }

        public override string NotificationName => "LegacyMobileNotification";

        private readonly IOneSignalApi _api;
        private readonly ILogger<LegacyMobileNotification> _logger;
        private readonly IRepository<NotificationUserId> _notifications;
        private readonly UserManager<OmbiUser> _userManager;
        private readonly IRepository<Issues> _issueRepository;

        protected override bool ValidateConfiguration(MobileNotificationSettings settings)
        {
            return true;
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
            var playerIds = await GetAdmins(NotificationType.NewRequest);
            await Send(playerIds, notification, settings, model, true);
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

            // Get admin devices
            var playerIds = await GetAdmins(NotificationType.Issue);
            await Send(playerIds, notification, settings, model);
        }

        protected override async Task IssueComment(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.IssueComment, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.IssueComment} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            if (model.Substitutes.TryGetValue("AdminComment", out var isAdminString))
            {
                var isAdmin = bool.Parse(isAdminString);
                if (isAdmin)
                {
                    model.Substitutes.TryGetValue("IssueId", out var issueId);
                    // Send to user
                    var playerIds = await GetUsersForIssue(model, int.Parse(issueId), NotificationType.IssueComment);
                    await Send(playerIds, notification, settings, model);
                }
                else
                {
                    // Send to admin
                    var playerIds = await GetAdmins(NotificationType.IssueComment);
                    await Send(playerIds, notification, settings, model);
                }
            }
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

            // Send to user
            var playerIds = GetUsers(model, NotificationType.IssueResolved);

            await Send(playerIds, notification, settings, model);
        }


        protected override async Task AddedToRequestQueue(NotificationOptions model, MobileNotificationSettings settings)
        {

            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.ItemAddedToFaultQueue, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.ItemAddedToFaultQueue} is disabled for {NotificationAgent.Mobile}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            // Get admin devices
            var playerIds = await GetAdmins(NotificationType.Test);
            await Send(playerIds, notification, settings, model);
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

            // Send to user
            var playerIds = GetUsers(model, NotificationType.RequestDeclined);
            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification, settings, model);
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

            // Send to user
            var playerIds = GetUsers(model, NotificationType.RequestApproved);

            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification, settings, model);
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
            // Send to user
            var playerIds = GetUsers(model, NotificationType.RequestAvailable);

            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification, settings, model);
        }
        protected override Task Send(NotificationMessage model, MobileNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected async Task Send(List<string> playerIds, NotificationMessage model, MobileNotificationSettings settings, NotificationOptions requestModel, bool isAdminNotification = false)
        {
            if (playerIds == null || !playerIds.Any())
            {
                return;
            }
            var response = await _api.PushNotification(playerIds, model.Message, isAdminNotification, requestModel.RequestId, (int)requestModel.RequestType);
            _logger.LogDebug("Sent message to {0} recipients with message id {1}", response.recipients, response.id);
        }

        protected override async Task Test(NotificationOptions model, MobileNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            // Send to user
            var user = await _userManager.Users.Include(x => x.NotificationUserIds).FirstOrDefaultAsync(x => x.Id.Equals(model.UserId));
            if (user == null)
            {
                return;
            }

            var playerIds = user.NotificationUserIds.Select(x => x.PlayerId).ToList();
            await Send(playerIds, notification, settings, model);
        }

        private async Task<List<string>> GetAdmins(NotificationType type)
        {
            var adminUsers = (await _userManager.GetUsersInRoleAsync(OmbiRoles.Admin)).Select(x => x.Id).ToList();
            var notificationUsers = _notifications.GetAll().Include(x => x.User).Where(x => adminUsers.Contains(x.UserId));
            var playerIds = await notificationUsers.Select(x => x.PlayerId).ToListAsync();
            if (!playerIds.Any())
            {
                _logger.LogInformation(
                    $"there are no admins to send a notification for {type}, for agent {NotificationAgent.Mobile}");
                return null;
            }
            return playerIds;
        }

        private List<string> GetUsers(NotificationOptions model, NotificationType type)
        {
            var notificationIds = new List<NotificationUserId>();
            if (MovieRequest != null || TvRequest != null)
            {
                notificationIds = model.RequestType == RequestType.Movie
                    ? MovieRequest?.RequestedUser?.NotificationUserIds
                    : TvRequest?.RequestedUser?.NotificationUserIds;
            }
            if (model.UserId.HasValue() && (!notificationIds?.Any() ?? true))
            {
                var user = _userManager.Users.Include(x => x.NotificationUserIds).FirstOrDefault(x => x.Id == model.UserId);
                notificationIds = user.NotificationUserIds;
            }

            if (!notificationIds?.Any() ?? true)
            {
                _logger.LogInformation(
                    $"there are no users to send a notification for {type}, for agent {NotificationAgent.Mobile}");
                return null;
            }
            var playerIds = notificationIds.Select(x => x.PlayerId).ToList();
            return playerIds;
        }

        private async Task<List<string>> GetUsersForIssue(NotificationOptions model, int issueId, NotificationType type)
        {
            var notificationIds = new List<NotificationUserId>();

            var issue = await _issueRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Id == issueId);

            // Get the user that raised the issue to send the notification to
            var userRaised = await _userManager.Users.Include(x => x.NotificationUserIds).FirstOrDefaultAsync(x => x.Id == issue.UserReportedId);

            notificationIds = userRaised.NotificationUserIds;

            if (!notificationIds?.Any() ?? true)
            {
                _logger.LogInformation(
                    $"there are no users to send a notification for {type}, for agent {NotificationAgent.Mobile}");
                return null;
            }
            var playerIds = notificationIds.Select(x => x.PlayerId).ToList();
            return playerIds;
        }

        private async Task AddSubscribedUsers(List<string> playerIds)
        {
            if (await SubsribedUsers.AnyAsync())
            {
                foreach (var user in SubsribedUsers)
                {
                    var notificationId = user.NotificationUserIds;
                    if (notificationId.Any())
                    {
                        playerIds.AddRange(notificationId.Select(x => x.PlayerId));
                    }
                }
            }
        }
    }
}