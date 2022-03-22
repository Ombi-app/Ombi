using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;
using Ombi.Schedule.Jobs.MediaServer;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Plex.Interfaces
{
    public interface IPlexEpisodeSync : IMediaServerEpisodeSync<PlexEpisode, Metadata>, IBaseJob
    {
    }
}