using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IUserPlayedEpisodeRepository : IExternalRepository<UserPlayedEpisode>
    {
        Task<UserPlayedEpisode> Get(int theMovieDbId, int seasonNumber, int episodeNumber, string userId);
    }
}