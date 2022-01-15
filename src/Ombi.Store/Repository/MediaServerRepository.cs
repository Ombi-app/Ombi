using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public abstract class MediaServerContentRepository<T> : ExternalRepository<T>, IMediaServerContentRepository<T> where T : MediaServerContent
    {
        protected ExternalContext Db { get; }
        public abstract RecentlyAddedType RecentlyAddedType { get; }

        public MediaServerContentRepository(ExternalContext db) : base(db)
        {
            Db = db;
        }

        public abstract Task Update(IMediaServerContent existingContent);

        // TOOD: this smells: trying to wrap ExternalRepository methods in IMediaServerContentRepositoryLight for generic consumption
        public IQueryable<IMediaServerContent> GetAllContent() => (IQueryable<IMediaServerContent>)GetAll();
        public async Task<IMediaServerContent> FindContent(object key) => (IMediaServerContent)await Find(key);

        public abstract IQueryable<IMediaServerEpisode> GetAllEpisodes();
        public abstract Task<IMediaServerEpisode> Add(IMediaServerEpisode content);
        public abstract Task AddRange(IEnumerable<IMediaServerEpisode> content);
        public abstract void UpdateWithoutSave(IMediaServerContent existingContent);
    }
}