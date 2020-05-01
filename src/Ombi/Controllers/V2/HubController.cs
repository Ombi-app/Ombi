using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Core.Authentication;
using Ombi.Hubs;
using Ombi.Models;

namespace Ombi.Controllers.V2
{
    [Admin]
    public class HubController : V2Controller
    {
        public HubController(OmbiUserManager um)
        {
            _um = um;
        }

        private readonly OmbiUserManager _um;

        /// <summary>
        /// Returns the currently connected users in Ombi
        /// </summary>
        /// <returns></returns>
        [HttpGet("Users")]
        public async Task<List<ConnectedUsersViewModel>> GetConnectedUsers()
        {
            var users = NotificationHub.UsersOnline.Values;
            var allUsers = _um.Users;
            var model = new List<ConnectedUsersViewModel>();
            foreach (var user in users)
            {
                var ombiUser = await allUsers.FirstOrDefaultAsync(x => x.Id == user.UserId);

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