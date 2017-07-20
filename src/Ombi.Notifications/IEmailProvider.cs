using System.Threading.Tasks;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Notifications
{
    public interface IEmailProvider
    {
        Task Send(NotificationMessage model, EmailNotificationSettings settings);
        Task SendAdHoc(NotificationMessage model, EmailNotificationSettings settings);
    }
}