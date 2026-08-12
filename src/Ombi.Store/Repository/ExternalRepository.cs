using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class ExternalRepository<T> : BaseRepository<T, ExternalContext>, IExternalRepository<T> where T : class, IEntity
    {
        public ExternalRepository(ExternalContext ctx) : base(ctx)
        {
        }
    }
}