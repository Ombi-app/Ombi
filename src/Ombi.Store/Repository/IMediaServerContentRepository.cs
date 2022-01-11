using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IMediaServerContentRepository<Content, Episode> : IExternalRepository<Content> where Content : Entity
    {
        Task Update(Content existingContent);
        IQueryable<Episode> GetAllEpisodes();
        Task<Episode> Add(Episode content);
        Task AddRange(IEnumerable<Episode> content);
        
        void UpdateWithoutSave(Content existingContent);
    }
}