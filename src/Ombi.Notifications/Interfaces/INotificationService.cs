using System.Threading.Tasks;
using Ombi.Notifications;
using Ombi.Notifications.Models;

namespace Ombi.Core.Notifications
{
    public interface INotificationService
    {
        Task Publish(NotificationOptions model);
        Task Publish(NotificationOptions model, Ombi.Settings.Settings.Models.Settings settings);
        Task PublishTest(NotificationOptions model, Ombi.Settings.Settings.Models.Settings settings, INotification type);
    }
}