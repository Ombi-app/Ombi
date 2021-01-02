using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IJellyfinContentRepository : IRepository<JellyfinContent>
    {
        IQueryable<JellyfinContent> Get();
        Task<JellyfinContent> GetByTheMovieDbId(string mov);
        Task<JellyfinContent> GetByTvDbId(string tv);
        Task<JellyfinContent> GetByImdbId(string imdbid);
        Task<JellyfinContent> GetByJellyfinId(string jellyfinId);
        Task Update(JellyfinContent existingContent);
        IQueryable<JellyfinEpisode> GetAllEpisodes();
        Task<JellyfinEpisode> Add(JellyfinEpisode content);
        Task<JellyfinEpisode> GetEpisodeByJellyfinId(string key);
        Task AddRange(IEnumerable<JellyfinEpisode> content);

        void UpdateWithoutSave(JellyfinContent existingContent);
    }
}
