using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public interface IRequestEngine
    {
        Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model);
        bool ShouldAutoApprove(RequestType requestType);
    }
}