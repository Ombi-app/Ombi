using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public interface ILidarrAlbumSync : IJob
    {
        void Dispose();
        Task<IEnumerable<LidarrAlbumCache>> GetCachedContent();
    }
}