using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs
{
    public interface IPlexContentCacher
    {
        Task CacheContent();
    }
}