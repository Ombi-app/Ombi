using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IWelcomeEmail : IBaseJob
    {
        Task SendEmail(OmbiUser user);
    }
}