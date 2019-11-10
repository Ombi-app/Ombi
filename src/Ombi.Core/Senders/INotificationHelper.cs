using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public interface INotificationHelper
    {
        Task NewRequest(FullBaseRequest model);
        Task NewRequest(ChildRequests model);
        Task NewRequest(AlbumRequest model);
        Task Notify(MovieRequests model, NotificationType type);
        Task Notify(ChildRequests model, NotificationType type);
        Task Notify(AlbumRequest model, NotificationType type);
        Task Notify(NotificationOptions model);
    }
}