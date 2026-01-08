using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.External.NotificationServices.Notifications.Models;

namespace Ombi.Api.External.NotificationServices.Notifications
{
    public interface IOneSignalApi
    {
        Task<OneSignalNotificationResponse> PushNotification(List<string> playerIds, string message, bool isAdminNotification, int requestId, int requestType);
    }
}