using System.Threading.Tasks;
using MimeKit;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Notifications
{
    public interface IEmailProvider
    {
        Task Send(NotificationMessage model, EmailNotificationSettings settings);
        Task SendAdHoc(NotificationMessage model, EmailNotificationSettings settings);
        Task Send(MimeMessage message, EmailNotificationSettings settings);
    }
}