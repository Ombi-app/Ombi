using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public interface ILidarrArtistSync
    {
        Task CacheContent();
        void Dispose();
        Task<IEnumerable<LidarrArtistCache>> GetCachedContent();
    }
}