using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMovieRequestEngine : IRequestEngine<MovieRequests>
    {
        Task<RequestEngineResult> RequestMovie(MovieRequestViewModel model);

        Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search);

        Task RemoveMovieRequest(int requestId);
        Task RemoveAllMovieRequests();

        Task<MovieRequests> UpdateMovieRequest(MovieRequests request);
        Task<RequestEngineResult> ApproveMovie(MovieRequests request);
        Task<RequestEngineResult> ApproveMovieById(int requestId);
        Task<RequestEngineResult> DenyMovieById(int modelId, string denyReason);
    }
}