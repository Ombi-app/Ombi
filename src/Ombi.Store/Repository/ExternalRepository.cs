using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class ExternalRepository<T> : BaseRepository<T, IExternalContext>, IExternalRepository<T> where T : Entity
    {
        public ExternalRepository(IExternalContext ctx) : base(ctx)
        {
        }
    }
}