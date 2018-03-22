using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface INewsletterJob : IBaseJob
    {
        Task Start();
    }
}