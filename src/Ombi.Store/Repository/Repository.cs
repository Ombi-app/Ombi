using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    /// <summary>
    /// Generic repository for Ombi application entities constrained to types implementing <see cref="IEntity"/>.
    /// </summary>
    public class Repository<T> : BaseRepository<T, OmbiContext>, IRepository<T> where T : class, IEntity
    {
        public Repository(OmbiContext ctx) : base(ctx)
        {
        }
    }
}