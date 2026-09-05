using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    /// <summary>
    /// Generic repository for Ombi application entities constrained to types implementing <see cref="IEntity"/>.
    /// </summary>
    public class Repository<T> : BaseRepository<T, OmbiContext>, IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Creates a repository backed by the primary Ombi database context.
        /// </summary>
        /// <param name="ctx">Application database context.</param>
        public Repository(OmbiContext ctx) : base(ctx)
        {
        }
    }
}