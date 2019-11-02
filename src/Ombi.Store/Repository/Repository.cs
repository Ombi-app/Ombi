using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class Repository<T> : BaseRepository<T, OmbiContext>, IRepository<T> where T : Entity
    {
        public Repository(OmbiContext ctx) : base(ctx)
        {
        }
    }
}