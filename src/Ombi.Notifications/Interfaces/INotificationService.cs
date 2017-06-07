using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ombi.Core.Settings.Models;
using Ombi.Notifications.Models;

namespace Ombi.Notifications
{
    public interface INotificationService
    {
        Task Publish(NotificationModel model);
        Task Publish(NotificationModel model, Settings.Settings.Models.Settings settings);
        Task PublishTest(NotificationModel model, Settings.Settings.Models.Settings settings, INotification type);
    }
}