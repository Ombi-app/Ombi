using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMovieRequestEngine : IRequestEngine<MovieRequestModel>
    {
        Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model);

        bool ShouldAutoApprove(RequestType requestType);
        

        Task<IEnumerable<MovieRequestModel>> SearchMovieRequest(string search);

        Task RemoveMovieRequest(int requestId);

        Task<MovieRequestModel> UpdateMovieRequest(MovieRequestModel request);
    }
}