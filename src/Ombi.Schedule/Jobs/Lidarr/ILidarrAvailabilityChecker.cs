using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public interface ILidarrAvailabilityChecker
    {
        Task Start();
    }
}