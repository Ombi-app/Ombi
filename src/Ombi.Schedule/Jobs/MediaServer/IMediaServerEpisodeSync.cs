using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.MediaServer.Models.Media.Tv;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.MediaServer
{
    public interface IMediaServerEpisodeSync<T, U> : IBaseJob
    where T:  IMediaServerEpisode
    where U: IMediaServerEpisodes
    {
        Task<HashSet<T>> ProcessEpisodes(IAsyncEnumerable<U> serverEpisodes, ICollection<T> currentEpisodes);
    }
}