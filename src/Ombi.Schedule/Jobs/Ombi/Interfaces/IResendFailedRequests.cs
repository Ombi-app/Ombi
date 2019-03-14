using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IResendFailedRequests
    {
        Task Start();
    }
}