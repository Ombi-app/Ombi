using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMovieRequestEngine : IRequestEngine<MovieRequests>
    {
        Task<RequestEngineResult> RequestMovie(MovieRequestViewModel model);

        Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search);

        Task RemoveMovieRequest(int requestId);
        Task RemoveAllMovieRequests();
        Task<MovieRequests> GetRequest(int requestId);
        Task<MovieRequests> UpdateMovieRequest(MovieRequests request);
        Task<RequestEngineResult> ApproveMovie(MovieRequests request);
        Task<RequestEngineResult> ApproveMovieById(int requestId);
        Task<RequestEngineResult> DenyMovieById(int modelId, string denyReason);
        Task<RequestsViewModel<MovieRequests>> GetRequests(int count, int position, string sortProperty, string sortOrder);

        Task<RequestsViewModel<MovieRequests>> GetUnavailableRequests(int count, int position, string sortProperty,
            string sortOrder);
        Task<RequestsViewModel<MovieRequests>> GetRequestsByStatus(int count, int position, string sortProperty, string sortOrder, RequestStatus status);
        Task<RequestEngineResult> UpdateAdvancedOptions(MediaAdvancedOptions options);
    }
}