using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Helpers;

namespace Ombi.Hubs
{
    public class NotificationHub : Hub
    {
        public NotificationHub(OmbiUserManager um)
        {
            _userManager = um;
        }

        public static ConcurrentDictionary<string, HubUsers> UsersOnline = new ConcurrentDictionary<string, HubUsers>();

        public static List<string> AdminConnectionIds
        {
            get
            {
                return UsersOnline.Where(x => x.Value.Roles.Contains(OmbiRoles.Admin)).Select(x => x.Key).ToList();
            }
        }

        public const string NotificationEvent = "Notification";

        private readonly OmbiUserManager _userManager;

        public override async Task OnConnectedAsync()
        {
            var identity = (ClaimsIdentity) Context.User.Identity;
            var userIdClaim = identity.Claims.FirstOrDefault(x => x.Type.Equals("Id", StringComparison.InvariantCultureIgnoreCase));
            if (userIdClaim == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            var user = await _userManager.Users.
                FirstOrDefaultAsync(x => x.Id == userIdClaim.Value);
            var claims = await _userManager.GetRolesAsync(user);
            UsersOnline.TryAdd(Context.ConnectionId, new HubUsers
            {
                UserId = userIdClaim.Value,
                Roles = claims
            });
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UsersOnline.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public Task Notification(string data)
        {
            return Clients.All.SendAsync(NotificationEvent, data);
        }
    }

    public class HubUsers
    {
        public string UserId { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
