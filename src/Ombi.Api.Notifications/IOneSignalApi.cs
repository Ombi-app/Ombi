using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Notifications.Models;

namespace Ombi.Api.Notifications
{
    public interface IOneSignalApi
    {
        Task<OneSignalNotificationResponse> PushNotification(List<string> playerIds, string message, bool isAdminNotification, int requestId, int requestType);
    }
}