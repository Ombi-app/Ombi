using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs
{
    public interface IPlexContentSync : IBaseJob
    {
        Task CacheContent(bool recentlyAddedSearch = false);
    }
}