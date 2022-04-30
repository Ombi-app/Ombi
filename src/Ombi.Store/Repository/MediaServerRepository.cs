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
        public abstract IQueryable<IMediaServerEpisode> GetAllEpisodes();
        public abstract Task<IMediaServerEpisode> Add(IMediaServerEpisode content);
        public abstract Task AddRange(IEnumerable<IMediaServerEpisode> content);
        public abstract void UpdateWithoutSave(IMediaServerContent existingContent);
        public abstract Task UpdateRange(IEnumerable<IMediaServerContent> existingContent);
        public abstract Task DeleteTv(T tv);
    }
}