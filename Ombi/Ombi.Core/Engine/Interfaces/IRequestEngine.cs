using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public interface IRequestEngine
    {
        Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model);
        bool ShouldAutoApprove(RequestType requestType);
        Task<IEnumerable<RequestViewModel>> GetRequests();
        Task<IEnumerable<RequestViewModel>> GetRequests(int count, int position);
        Task<IEnumerable<RequestViewModel>> SearchRequest(string search);
        Task RemoveRequest(int requestId);
        Task<RequestViewModel> UpdateRequest(RequestViewModel request);
    }
}