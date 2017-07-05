using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMovieRequestEngine : IRequestEngine<MovieRequests>
    {
        Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model);

        Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search);

        Task RemoveMovieRequest(int requestId);

        Task<MovieRequests> UpdateMovieRequest(MovieRequests request);
    }
}