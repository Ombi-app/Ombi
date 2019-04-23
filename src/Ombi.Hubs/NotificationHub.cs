using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;
using System.Xml;

namespace Ombi.Hubs
{
    public class NotificationHub : Hub
    {
        public static ConcurrentDictionary<string, string> UsersOnline = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var identity = (ClaimsIdentity) Context.User.Identity;
            var userIdClaim = identity.Claims.FirstOrDefault(x => x.Type.Equals("Id", StringComparison.InvariantCultureIgnoreCase));
            if (userIdClaim == null)
            {
                return base.OnConnectedAsync();
            }

            UsersOnline.TryAdd(Context.ConnectionId, userIdClaim.Value);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UsersOnline.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public Task Notification(string data)
        {
            return Clients.All.SendAsync("Notification", data);
        }
    }
}
