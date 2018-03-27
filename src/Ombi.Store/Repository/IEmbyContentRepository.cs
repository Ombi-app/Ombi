using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IEmbyContentRepository : IDisposable
    {
        Task<EmbyContent> Add(EmbyContent content);
        Task AddRange(IEnumerable<EmbyContent> content);
        Task<bool> ContentExists(string providerId);
        IQueryable<EmbyContent> Get();
        Task<EmbyContent> Get(string providerId);
        IQueryable<EmbyContent> GetAll();
        Task<EmbyContent> GetByEmbyId(string embyId);
        Task Update(EmbyContent existingContent);
        IQueryable<EmbyEpisode> GetAllEpisodes();
        Task<EmbyEpisode> Add(EmbyEpisode content);
        Task<EmbyEpisode> GetEpisodeByEmbyId(string key);
        Task AddRange(IEnumerable<EmbyEpisode> content);
    }
}