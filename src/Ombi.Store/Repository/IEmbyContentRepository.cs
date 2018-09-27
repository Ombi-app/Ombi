using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IEmbyContentRepository : IRepository<EmbyContent>
    {
        IQueryable<EmbyContent> Get();
        Task<EmbyContent> GetByTheMovieDbId(string mov);
        Task<EmbyContent> GetByTvDbId(string tv);
        Task<EmbyContent> GetByImdbId(string imdbid);
        Task<EmbyContent> GetByEmbyId(string embyId);
        Task Update(EmbyContent existingContent);
        IQueryable<EmbyEpisode> GetAllEpisodes();
        Task<EmbyEpisode> Add(EmbyEpisode content);
        Task<EmbyEpisode> GetEpisodeByEmbyId(string key);
        Task AddRange(IEnumerable<EmbyEpisode> content);

        void UpdateWithoutSave(EmbyContent existingContent);
    }
}