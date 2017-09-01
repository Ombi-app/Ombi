using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IEmbyContentRepository
    {
        Task<EmbyContent> Add(EmbyContent content);
        Task AddRange(IEnumerable<EmbyContent> content);
        Task<bool> ContentExists(string providerId);
        IQueryable<EmbyContent> Get();
        Task<EmbyContent> Get(string providerId);
        Task<IEnumerable<EmbyContent>> GetAll();
        Task<EmbyContent> GetByEmbyId(string embyId);
        Task Update(EmbyContent existingContent);
    }
}