using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    // TOOD: this is a mess done to bypass the fact that 
    // I can't pass around IMediaServerContentRepository as a parameter
    // because I want to pass it a generic IMediaServerContent as a 'type'
    // and casting from concrete classes doesn't work due to my poor C# knowledge

    // My workaround so far has been to use this lightened interface, 
    // but the ever-growing number of wrapper methods for methods coming from IRepository<T>
    // is starting to smell (see implementing class MediaServerContentRepository).
    public interface IMediaServerContentRepositoryLight
    {
        RecentlyAddedType RecentlyAddedType{ get; }
        Task Update(IMediaServerContent existingContent);

        // IQueryable<IMediaServerContent> GetAllContent();
        // Task<IMediaServerContent> FindContent(object key);
        // Task<int> SaveChangesAsync();

        IQueryable<IMediaServerEpisode> GetAllEpisodes();
        Task<IMediaServerEpisode> Add(IMediaServerEpisode content);
        Task AddRange(IEnumerable<IMediaServerEpisode> content);
        void UpdateWithoutSave(IMediaServerContent existingContent);
    }
}