using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Hubs;

public interface INotificationHubService
{
    IEnumerable<NotificationHubUser> GetOnlineUsers();
    Task SendNotificationToAdmins(string data, CancellationToken token = default);
    Task SendNotificationToAll(string data, CancellationToken token = default);
}