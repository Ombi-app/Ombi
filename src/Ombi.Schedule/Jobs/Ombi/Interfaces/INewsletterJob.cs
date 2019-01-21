using System.Threading.Tasks;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface INewsletterJob : IBaseJob
    {
        Task Start();
        Task Start(NewsletterSettings settings, bool test);
    }
}