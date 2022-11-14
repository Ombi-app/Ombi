using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.CloudService;
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
    public class MobileNotification : BaseNotification<MobileNotificationSettings>, IMobileNotification
    {
        public MobileNotification(ICloudMobileNotification api, ISettingsService<MobileNotificationSettings> sn, ILogger<MobileNotification> log, INotificationTemplatesRepository r,
            IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s, IRepository<MobileDevices> notification,
            UserManager<OmbiUser> um, IRepository<RequestSubscription> sub, IMusicRequestRepository music, IRepository<Issues> issueRepository,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t, s, log, sub, music, userPref, um)
        {
            _api = api;
            _logger = log;
            _notifications = notification;
            _userManager = um;
            _issueRepository = issueRepository;
        }

        public override string NotificationName => "MobileNotification";

        private readonly ICloudMobileNotification _api;
        private readonly ILogger _logger;
        private readonly IRepository<MobileDevices> _notifications;
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
                Subject = "New Request",
                Data = GetNotificationData(parsed, NotificationType.NewRequest)
            };

            // Get admin devices
            var playerIds = await GetPrivilegedUsersPlayerIds();
            await Send(playerIds, notification);
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
                Subject = "New Issue",
                Data = GetNotificationData(parsed, NotificationType.Issue)
            };

            // Get admin devices
            var playerIds = await GetAdmins();
            await Send(playerIds, notification);
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
                Data = GetNotificationData(parsed, NotificationType.IssueComment)
            };
            if (model.Substitutes.TryGetValue("AdminComment", out var isAdminString))
            {
                var isAdmin = bool.Parse(isAdminString);
                if (isAdmin)
                {
                    model.Substitutes.TryGetValue("IssueId", out var issueId);
                    // Send to user
                    var playerIds = await GetUsersForIssue(model, int.Parse(issueId), NotificationType.IssueComment);
                    await Send(playerIds, notification);
                }
                else
                {
                    // Send to admin
                    var playerIds = await GetAdmins();
                    await Send(playerIds, notification);
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
                Subject = "Issue Resolved",
                Data = GetNotificationData(parsed, NotificationType.IssueResolved)
            };

            // Send to user
            var playerIds = await GetUsers(model, NotificationType.IssueResolved);

            await Send(playerIds, notification);
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
                Subject = "Request Error",
                Data = GetNotificationData(parsed, NotificationType.ItemAddedToFaultQueue)
            };

            // Get admin devices
            var playerIds = await GetAdmins();
            await Send(playerIds, notification);
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
                Subject = "Request Declined",
                Data = GetNotificationData(parsed, NotificationType.RequestDeclined)
            };

            // Send to user
            var playerIds = await GetUsers(model, NotificationType.RequestDeclined);
            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification);
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
                Subject = "Request Approved",
                Data = GetNotificationData(parsed, NotificationType.RequestApproved)
            };

            // Send to user
            var playerIds = await GetUsers(model, NotificationType.RequestApproved);

            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification);
        }

        protected override async Task AvailableRequest(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.RequestAvailable, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.RequestAvailable} is disabled for {NotificationAgent.Mobile}");
                return;
            }

            var data = GetNotificationData(parsed, NotificationType.RequestAvailable);

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
                Subject = "Request Available",
                Data = data
            };
            // Send to user
            var playerIds = await GetUsers(model, NotificationType.RequestAvailable);

            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification);
        }

        private static Dictionary<string,string> GetNotificationData(NotificationMessageContent parsed, NotificationType type)
        {
            var notificationData = parsed.Data.ToDictionary(x => x.Key, x => x.Value);
            notificationData[nameof(NotificationType)] = type.ToString();
            return notificationData;
        }

        protected override Task Send(NotificationMessage model, MobileNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected async Task Send(List<string> playerIds, NotificationMessage model)
        {
            if (playerIds == null || !playerIds.Any())
            {
                return;
            }
            foreach (var token in playerIds)
            {
                await _api.SendMessage(new MobileNotificationRequest()
                {
                    Body = model.Message,
                    Title = model.Subject,
                    To = token,
                    Data = new Dictionary<string, string>(model.Data)
                });
            }

            _logger.LogDebug("Sent message to {0} recipients", playerIds.Count);
        }

        protected override async Task Test(NotificationOptions model, MobileNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
                Subject = "Test Notification"
            };
            // Send to user
            var user = await _userManager.Users.Include(x => x.NotificationUserIds).FirstOrDefaultAsync(x => x.Id.Equals(model.UserId));
            if (user == null)
            {
                return;
            }

            var playerIds = user.NotificationUserIds.Select(x => x.PlayerId).ToList();
            await Send(playerIds, notification);
        }

        private async Task<List<string>> GetAdmins()
        {
            return await GetNotificationRecipients(await _userManager.GetUsersInRoleAsync(OmbiRoles.Admin));
        }
        private async Task<List<string>> GetPrivilegedUsersPlayerIds()
        {
            return await GetNotificationRecipients(await GetPrivilegedUsers());
        }

        private async Task<List<string>> GetNotificationRecipients(IEnumerable<OmbiUser> users)
        {
            
            var adminUsers = users.Select(x => x.Id).ToList();
            var notificationUsers = _notifications.GetAll().Include(x => x.User).Where(x => adminUsers.Contains(x.UserId));
            var playerIds = await notificationUsers.Select(x => x.Token).ToListAsync();
            if (!playerIds.Any())
            {
                _logger.LogInformation(
                    $"there are no users to send a notification for agent {NotificationAgent.Mobile}");
                return null;
            }
            return playerIds;
        }

        private async Task<List<string>> GetUsers(NotificationOptions model, NotificationType type)
        {
            var notificationIds = new List<MobileDevices>();
            if (MovieRequest != null || TvRequest != null)
            {
                var userId = model.RequestType == RequestType.Movie
                    ? MovieRequest?.RequestedUser?.Id
                    : TvRequest?.RequestedUser?.Id;

                var userNotificationIds = await _notifications.GetAll().Where(x => x.UserId == userId).ToListAsync();
                notificationIds.AddRange(userNotificationIds);
            }
            if (model.UserId.HasValue() && (!notificationIds?.Any() ?? true))
            {
                var user = _userManager.Users.FirstOrDefault(x => x.Id == model.UserId);
                var userNotificationIds = await _notifications.GetAll().Where(x => x.UserId == model.UserId).ToListAsync();
                notificationIds.AddRange(userNotificationIds);
            }

            if (!notificationIds?.Any() ?? true)
            {
                _logger.LogInformation(
                    $"there are no users to send a notification for {type}, for agent {NotificationAgent.Mobile}");
                return null;
            }
            var playerIds = notificationIds.Select(x => x.Token).ToList();
            return playerIds;
        }

        private async Task<List<string>> GetUsersForIssue(NotificationOptions model, int issueId, NotificationType type)
        {
            var notificationIds = new List<MobileDevices>();

            var issue = await _issueRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Id == issueId);

            // Get the user that raised the issue to send the notification to
            var userRaised = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == issue.UserReportedId);

            notificationIds = await _notifications.GetAll().Where(x => x.UserId == userRaised.Id).ToListAsync();

            if (!notificationIds?.Any() ?? true)
            {
                _logger.LogInformation(
                    $"there are no users to send a notification for {type}, for agent {NotificationAgent.Mobile}");
                return null;
            }
            var playerIds = notificationIds.Select(x => x.Token).ToList();
            return playerIds;
        }

        private async Task AddSubscribedUsers(List<string> playerIds)
        {
            if (await Subscribed.AnyAsync())
            {
                foreach (var user in Subscribed)
                {
                    var notificationIds = await _notifications.GetAll().Where(x => x.UserId == user.Id).ToListAsync();

                    if (notificationIds.Any())
                    {
                        playerIds.AddRange(notificationIds.Select(x => x.Token));
                    }
                }
            }
        }

        protected override async Task PartiallyAvailable(NotificationOptions model, MobileNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Mobile, NotificationType.PartiallyAvailable, model);
            if (parsed.Disabled)
            {
                _logger.LogInformation($"Template {NotificationType.PartiallyAvailable} is disabled for {NotificationAgent.Mobile}");
                return;
            }

            var notification = new NotificationMessage
            {
                Message = parsed.Message,
                Subject = "Request Partially Available",
                Data = GetNotificationData(parsed, NotificationType.PartiallyAvailable)
            };


            var playerIds = await GetUsers(model, NotificationType.PartiallyAvailable);

            await AddSubscribedUsers(playerIds);
            await Send(playerIds, notification);
        }
    }
}