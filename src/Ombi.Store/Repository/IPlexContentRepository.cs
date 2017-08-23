using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IPlexContentRepository
    {
        Task<PlexContent> Add(PlexContent content);
        Task AddRange(IEnumerable<PlexContent> content);
        Task<bool> ContentExists(string providerId);
        Task<IEnumerable<PlexContent>> GetAll();
        Task<PlexContent> Get(string providerId);
        Task<PlexContent> GetByKey(string key);
        Task Update(PlexContent existingContent);
        IQueryable<PlexEpisode> GetAllEpisodes();
        Task<PlexEpisode> Add(PlexEpisode content);
        Task<PlexEpisode> GetEpisodeByKey(string key);
        Task AddRange(IEnumerable<PlexEpisode> content);
    }
}