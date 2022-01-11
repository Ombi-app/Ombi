using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IJellyfinContentRepository : IMediaServerContentRepository<JellyfinContent, JellyfinEpisode>
    {
        Task<JellyfinContent> GetByJellyfinId(string jellyfinId);
        Task<JellyfinEpisode> GetEpisodeByJellyfinId(string key);
        
        // TODO: merge these with IEmbyContentRepository
        IQueryable<JellyfinContent> Get();
        Task<JellyfinContent> GetByTheMovieDbId(string mov);
        Task<JellyfinContent> GetByTvDbId(string tv);
        Task<JellyfinContent> GetByImdbId(string imdbid);

    }
}
