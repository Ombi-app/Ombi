using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IPlexContentRepository
    {
        Task<PlexContent> Add(PlexContent content);
        Task<bool> ContentExists(string providerId);
        Task<IEnumerable<PlexContent>> GetAll();
        Task<PlexContent> Get(string providerId);
        Task<PlexContent> GetByKey(string key);
        Task Update(PlexContent existingContent);
    }
}