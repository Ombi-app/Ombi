using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IEmbyContentRepository : IMediaServerContentRepository<EmbyContent>
    {
        Task<EmbyContent> GetByEmbyId(string embyId);
        Task<EmbyEpisode> GetEpisodeByEmbyId(string key);

        // TODO: merge these with IJellyfinContentRepository
        IQueryable<EmbyContent> Get();
        Task<EmbyContent> GetByTheMovieDbId(string mov);
        Task<EmbyContent> GetByTvDbId(string tv);
        Task<EmbyContent> GetByImdbId(string imdbid);

    }
}