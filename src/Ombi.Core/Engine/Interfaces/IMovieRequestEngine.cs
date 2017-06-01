using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public interface IMovieRequestEngine
    {
        Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model);
        bool ShouldAutoApprove(RequestType requestType);
        Task<IEnumerable<MovieRequestModel>> GetMovieRequests(int count, int position);
        Task<IEnumerable<MovieRequestModel>> SearchMovieRequest(string search);
        Task RemoveMovieRequest(int requestId);
        Task<MovieRequestModel> UpdateMovieRequest(MovieRequestModel request);
        RequestCountModel RequestCount();
    }
}