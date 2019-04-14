using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Plex.Interfaces
{
    public interface IPlexEpisodeSync : IBaseJob
    {
        Task<HashSet<PlexEpisode>> ProcessEpsiodes(Metadata[] episodes, IQueryable<PlexEpisode> currentEpisodes);
    }
}