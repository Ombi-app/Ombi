using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IMediaServerContentRepositoryLight
    {
        Task Update(IMediaServerContent existingContent);
        IQueryable<IMediaServerEpisode> GetAllEpisodes();
        Task<IMediaServerEpisode> Add(IMediaServerEpisode content);
        Task AddRange(IEnumerable<IMediaServerEpisode> content);
        void UpdateWithoutSave(IMediaServerContent existingContent);
    }
}