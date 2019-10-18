using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public interface ILidarrArtistSync : IJob
    {
        void Dispose();
        Task<IEnumerable<LidarrArtistCache>> GetCachedContent();
    }
}