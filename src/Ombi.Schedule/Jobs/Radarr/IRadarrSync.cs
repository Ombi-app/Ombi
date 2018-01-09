using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Radarr
{
    public interface IRadarrSync : IBaseJob
    {
        Task CacheContent();
        Task<IEnumerable<RadarrCache>> GetCachedContent();
    }
}