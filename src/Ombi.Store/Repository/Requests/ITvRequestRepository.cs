using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public interface ITvRequestRepository 
    {
        IOmbiContext Db { get; }
        Task<TvRequests> Add(TvRequests request);
        Task<ChildRequests> AddChild(ChildRequests request);
        Task Delete(TvRequests request);
        Task DeleteChild(ChildRequests request);
        IQueryable<TvRequests> Get();
        Task<TvRequests> GetRequestAsync(int tvDbId);
        TvRequests GetRequest(int tvDbId);
        Task Update(TvRequests request);
        Task UpdateChild(ChildRequests request);
        IQueryable<ChildRequests> GetChild();
        Task Save();
        Task DeleteChildRange(IEnumerable<ChildRequests> request);
    }
}