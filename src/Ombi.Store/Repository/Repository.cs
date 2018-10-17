using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class Repository<T> : BaseRepository<T,IOmbiContext>, IRepository<T> where T : Entity
    {
        public Repository(IOmbiContext ctx) : base(ctx)
        {
        }
    }
}