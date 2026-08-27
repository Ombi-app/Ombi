using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    /// <summary>
    /// Generic repository for external-database entities, including cache tables backed by <see cref="LongEntity"/>.
    /// </summary>
    public class ExternalRepository<T> : BaseRepository<T, ExternalContext>, IExternalRepository<T> where T : class, IEntity
    {
        public ExternalRepository(ExternalContext ctx) : base(ctx)
        {
        }
    }
}