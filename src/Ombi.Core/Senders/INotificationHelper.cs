using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public interface INotificationHelper
    {
        void NewRequest(FullBaseRequest model);
        void NewRequest(ChildRequests model);
        void NewRequest(AlbumRequest model);
        void Notify(MovieRequests model, NotificationType type);
        void Notify(ChildRequests model, NotificationType type);
        void Notify(AlbumRequest model, NotificationType type);
    }
}