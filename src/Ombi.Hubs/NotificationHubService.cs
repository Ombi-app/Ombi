using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Ombi.Helpers;

namespace Ombi.Hubs;

public class NotificationHubService : INotificationHubService
{
    public const string NotificationEvent = "Notification";

    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public IEnumerable<NotificationHubUser> GetOnlineUsers()
    {
        return NotificationHub.UsersOnline.Values;
    }

    public Task SendNotificationToAdmins(string data, CancellationToken token = default)
    {
        return _hubContext.Clients
            .Clients(GetConnectionIdsWithRole(OmbiRoles.Admin))
            .SendAsync(NotificationEvent, data, token);
    }

    public Task SendNotificationToAll(string data, CancellationToken token = default)
    {
        return _hubContext.Clients.All.SendAsync(NotificationEvent, data, token);
    }
    
    private static List<string> GetConnectionIdsWithRole(string role)
    {
        return NotificationHub.UsersOnline
            .Where(x => x.Value.Roles.Contains(role))
            .Select(x => x.Key).ToList();
    }
}