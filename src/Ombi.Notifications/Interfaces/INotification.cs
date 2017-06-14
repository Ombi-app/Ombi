using System.Threading.Tasks;
using Ombi.Core.Settings.Models;
using Ombi.Notifications.Models;

namespace Ombi.Notifications
{
    public interface INotification
    {
        string NotificationName { get; }

        Task NotifyAsync(NotificationOptions model);

        /// <summary>
        /// Sends a notification to the user, this is usually for testing the settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        Task NotifyAsync(NotificationOptions model, Settings.Settings.Models.Settings settings);
    }
}