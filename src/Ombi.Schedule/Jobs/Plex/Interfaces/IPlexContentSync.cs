using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs
{
    public interface IPlexContentSync
    {
        Task CacheContent();
    }
}