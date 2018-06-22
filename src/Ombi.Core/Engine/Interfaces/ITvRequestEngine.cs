using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine.Interfaces
{
    public interface ITvRequestEngine : IRequestEngine<TvRequests>
    {

        Task RemoveTvRequest(int requestId);
        Task<TvRequests> GetTvRequest(int requestId);
        Task<RequestEngineResult> RequestTvShow(TvRequestViewModel tv);
        Task<RequestEngineResult> DenyChildRequest(int requestId);
        Task<RequestsViewModel<TvRequests>> GetRequestsLite(int count, int position, OrderFilterModel type);
        Task<IEnumerable<TvRequests>> SearchTvRequest(string search);
        Task<IEnumerable<TreeNode<TvRequests, List<ChildRequests>>>> SearchTvRequestTree(string search);
        Task<TvRequests> UpdateTvRequest(TvRequests request);
        Task<IEnumerable<TreeNode<TvRequests, List<ChildRequests>>>> GetRequestsTreeNode(int count, int position);
        Task<IEnumerable<ChildRequests>> GetAllChldren(int tvId);
        Task<ChildRequests> UpdateChildRequest(ChildRequests request);
        Task RemoveTvChild(int requestId);
        Task<RequestEngineResult> ApproveChildRequest(int id);
        Task<IEnumerable<TvRequests>> GetRequestsLite();
    }
}