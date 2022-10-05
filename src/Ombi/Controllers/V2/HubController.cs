using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Core.Authentication;
using Ombi.Hubs;
using Ombi.Models;
using Ombi.Store.Entities;

namespace Ombi.Controllers.V2
{
    [Admin]
    public class HubController : V2Controller
    {
        private readonly INotificationHubService _notificationHubService;
        private readonly OmbiUserManager _userManager;

        public HubController(
            INotificationHubService notificationHubService,
            OmbiUserManager userManager
        )
        {
            _notificationHubService = notificationHubService;
            _userManager = userManager;
        }

        /// <summary>
        /// Returns the currently connected users in Ombi
        /// </summary>
        /// <returns></returns>
        [HttpGet("Users")]
        public async Task<List<ConnectedUsersViewModel>> GetConnectedUsers()
        {
            IEnumerable<NotificationHubUser> users = _notificationHubService.GetOnlineUsers();
            List<ConnectedUsersViewModel> model = new();
            foreach (NotificationHubUser user in users)
            {
                OmbiUser ombiUser = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == user.UserId);
                if (ombiUser == null)
                {
                    continue;
                }

                model.Add(new ConnectedUsersViewModel
                {
                    UserId = ombiUser.Id,
                    DisplayName = ombiUser.UserAlias,
                    UserType = ombiUser.UserType
                });
            }

            return model;
        }
    }
}