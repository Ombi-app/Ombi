using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities.Requests;

namespace Ombi.Controllers.V2
{
    public class RequestsController : V2Controller
    {

        private readonly IMovieRequestEngine _movieRequestEngine;
        private readonly ITvRequestEngine _tvRequestEngine;
        private readonly IMusicRequestEngine _musicRequestEngine;

        public RequestsController(IMovieRequestEngine movieRequestEngine, ITvRequestEngine tvRequestEngine, IMusicRequestEngine musicRequestEngine)
        {
            _movieRequestEngine = movieRequestEngine;
            _tvRequestEngine = tvRequestEngine;
            _musicRequestEngine = musicRequestEngine;
        }

        /// <summary>
        /// Gets movie requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        [HttpGet("movie/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetRequests(int count, int position, string sort, string sortOrder)
        {
            return await _movieRequestEngine.GetRequests(count, position, sort, sortOrder);
        }
        
        [HttpGet("movie/availble/{count:int}/{position:int}/{sort}/{sortOrder}")]
        [HttpGet("movie/available/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetAvailableRequests(int count, int position, string sort, string sortOrder)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Available);
        }
        
        [HttpGet("movie/processing/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetProcessingRequests(int count, int position, string sort, string sortOrder)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.ProcessingRequest);
        }
        
        [HttpGet("movie/pending/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetPendingRequests(int count, int position, string sort, string sortOrder)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.PendingApproval);
        }
        
        [HttpGet("movie/denied/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetDeniedRequests(int count, int position, string sort, string sortOrder)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Denied);
        }

        /// <summary>
        /// Gets the unavailable movie requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        [HttpGet("movie/unavailable/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetNotAvailableRequests(int count, int position, string sort, string sortOrder)
        {
            return await _movieRequestEngine.GetUnavailableRequests(count, position, sort, sortOrder);
        }

        /// <summary>
        /// Gets Tv requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        [HttpGet("tv/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetTvRequests(int count, int position, string sort, string sortOrder)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder);
        }

        [HttpGet("tv/pending/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetPendingTvRequests(int count, int position, string sort, string sortOrder)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.PendingApproval);
        }

        [HttpGet("tv/processing/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetProcessingTvRequests(int count, int position, string sort, string sortOrder)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.ProcessingRequest);
        }

        [HttpGet("tv/available/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetAvailableTvRequests(int count, int position, string sort, string sortOrder)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.Available);
        }

        [HttpGet("tv/denied/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetDeniedTvRequests(int count, int position, string sort, string sortOrder)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.Denied);
        }

        /// <summary>
        /// Gets unavailable Tv requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        [HttpGet("tv/unavailable/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetNotAvailableTvRequests(int count, int position, string sort, string sortOrder)
        {
            return await _tvRequestEngine.GetUnavailableRequests(count, position, sort, sortOrder);
        }

        [HttpPost("movie/advancedoptions")]
        public async Task<RequestEngineResult> UpdateAdvancedOptions([FromBody] MediaAdvancedOptions options)
        {
            return await _movieRequestEngine.UpdateAdvancedOptions(options);
        }

        [HttpPost("tv/advancedoptions")]
        public async Task<RequestEngineResult> UpdateTvAdvancedOptions([FromBody] MediaAdvancedOptions options)
        {
            return await _tvRequestEngine.UpdateAdvancedOptions(options);
        }

        [HttpGet("album/available/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetAvailableAlbumRequests(int count, int position, string sort, string sortOrder)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Available);
        }
        
        [HttpGet("album/processing/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetProcessingAlbumRequests(int count, int position, string sort, string sortOrder)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.ProcessingRequest);
        }
        
        [HttpGet("album/pending/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetPendingAlbumRequests(int count, int position, string sort, string sortOrder)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.PendingApproval);
        }
        
        [HttpGet("album/denied/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetDeniedAlbumRequests(int count, int position, string sort, string sortOrder)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Denied);
        }

        [HttpGet("album/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetAlbumRequests(int count, int position, string sort, string sortOrder)
        {
            return await _musicRequestEngine.GetRequests(count, position, sort, sortOrder);
        }
    }
}