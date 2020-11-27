using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public interface ITvRequestRepository : IRepository<TvRequests>
    {
        OmbiContext Db { get; }
        Task<ChildRequests> AddChild(ChildRequests request);
        Task DeleteChild(ChildRequests request);
        IQueryable<TvRequests> Get();
        IQueryable<TvRequests> GetLite();
        IQueryable<TvRequests> Get(string userId);
        IQueryable<TvRequests> GetLite(string userId);
        Task<TvRequests> GetRequestAsync(int tvDbId);
        TvRequests GetRequest(int tvDbId);
        Task Update(TvRequests request);
        Task UpdateChild(ChildRequests request);
        IQueryable<ChildRequests> GetChild();
        IQueryable<ChildRequests> GetChild(string userId);
        Task MarkEpisodeAsAvailable(int id);
        Task MarkChildAsAvailable(int id);
        Task Save();
        Task DeleteChildRange(IEnumerable<ChildRequests> request);
    }
}