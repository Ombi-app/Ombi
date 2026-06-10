using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities.Requests;
using System;
using Ombi.Store.Entities;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ombi.Attributes;
using Ombi.Helpers;
using Ombi.Core.Services;
using System.Collections.Generic;

namespace Ombi.Controllers.V2
{
    public class RequestsController : V2Controller
    {

        private readonly IMovieRequestEngine _movieRequestEngine;
        private readonly ITvRequestEngine _tvRequestEngine;
        private readonly IMusicRequestEngine _musicRequestEngine;
        private readonly IVoteEngine _voteEngine;
        private readonly ILogger<RequestsController> _logger;
        private readonly IRecentlyRequestedService _recentlyRequestedService;

        public RequestsController(IMovieRequestEngine movieRequestEngine, ITvRequestEngine tvRequestEngine, IMusicRequestEngine musicRequestEngine,
            IVoteEngine voteEngine, ILogger<RequestsController> logger, IRecentlyRequestedService recentlyRequestedService)
        {
            _movieRequestEngine = movieRequestEngine;
            _tvRequestEngine = tvRequestEngine;
            _musicRequestEngine = musicRequestEngine;
            _voteEngine = voteEngine;
            _logger = logger;
            _recentlyRequestedService = recentlyRequestedService;
        }

        /// <summary>
        /// Gets movie requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        /// <param name="requestedBy">Optional. Only return requests made by this user id</param>
        [HttpGet("movie/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _movieRequestEngine.GetRequests(count, position, sort, sortOrder, requestedBy);
        }
        
        [HttpGet("movie/availble/{count:int}/{position:int}/{sort}/{sortOrder}")]
        [HttpGet("movie/available/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetAvailableRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Available, requestedBy);
        }
        
        [HttpGet("movie/processing/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetProcessingRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.ProcessingRequest, requestedBy);
        }
        
        [HttpGet("movie/pending/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetPendingRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.PendingApproval, requestedBy);
        }
        
        [HttpGet("movie/denied/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetDeniedRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _movieRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Denied, requestedBy);
        }

        /// <summary>
        /// Gets the unavailable movie requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        /// <param name="requestedBy">Optional. Only return requests made by this user id</param>
        [HttpGet("movie/unavailable/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<MovieRequests>> GetNotAvailableRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _movieRequestEngine.GetUnavailableRequests(count, position, sort, sortOrder, requestedBy);
        }

        /// <summary>
        /// Gets Tv requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        /// <param name="requestedBy">Optional. Only return requests made by this user id</param>
        [HttpGet("tv/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetTvRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, requestedBy);
        }

        [HttpGet("tv/pending/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetPendingTvRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.PendingApproval, requestedBy);
        }

        [HttpGet("tv/processing/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetProcessingTvRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.ProcessingRequest, requestedBy);
        }

        [HttpGet("tv/available/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetAvailableTvRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.Available, requestedBy);
        }

        [HttpGet("tv/denied/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetDeniedTvRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _tvRequestEngine.GetRequests(count, position, sort, sortOrder, RequestStatus.Denied, requestedBy);
        }

        /// <summary>
        /// Gets unavailable Tv requests.
        /// </summary>
        /// <param name="count">The count of items you want to return. e.g. 30</param>
        /// <param name="position">The position. e.g. position 60 for a 2nd page (since we have already got the first 30 items)</param>
        /// <param name="sort">The item to sort on e.g. "requestDate"</param>
        /// <param name="sortOrder">asc or desc</param>
        /// <param name="requestedBy">Optional. Only return requests made by this user id</param>
        [HttpGet("tv/unavailable/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<ChildRequests>> GetNotAvailableTvRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _tvRequestEngine.GetUnavailableRequests(count, position, sort, sortOrder, requestedBy);
        }

        [PowerUser]
        [HttpPost("movie/advancedoptions")]
        public async Task<RequestEngineResult> UpdateAdvancedOptions([FromBody] MediaAdvancedOptions options)
        {
            return await _movieRequestEngine.UpdateAdvancedOptions(options);
        }

        [PowerUser]
        [HttpPost("tv/advancedoptions")]
        public async Task<RequestEngineResult> UpdateTvAdvancedOptions([FromBody] MediaAdvancedOptions options)
        {
            return await _tvRequestEngine.UpdateAdvancedOptions(options);
        }

        [HttpGet("album/available/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetAvailableAlbumRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Available, requestedBy);
        }
        
        [HttpGet("album/processing/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetProcessingAlbumRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.ProcessingRequest, requestedBy);
        }
        
        [HttpGet("album/pending/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetPendingAlbumRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.PendingApproval, requestedBy);
        }
        
        [HttpGet("album/denied/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetDeniedAlbumRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _musicRequestEngine.GetRequestsByStatus(count, position, sort, sortOrder, RequestStatus.Denied, requestedBy);
        }

        [HttpGet("album/{count:int}/{position:int}/{sort}/{sortOrder}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetAlbumRequests(int count, int position, string sort, string sortOrder, [FromQuery] string requestedBy = null)
        {
            return await _musicRequestEngine.GetRequests(count, position, sort, sortOrder, requestedBy);
        }

        /// <summary>
        /// Requests a tv show/episode/season.
        /// </summary>
        /// <param name="tv">The tv.</param>
        /// <returns></returns>
        [HttpPost("tv")]
        public async Task<RequestEngineResult> RequestTv([FromBody] TvRequestViewModelV2 tv)
        {
            tv.RequestedByAlias = GetApiAlias();
            var result = await _tvRequestEngine.RequestTvShow(tv);
            if (result.Result)
            {
                var voteResult = await _voteEngine.UpVote(result.RequestId, RequestType.TvShow);
                if (voteResult.IsError)
                {
                    _logger.LogError("Couldn't automatically add the vote for the tv {0} because {1}", tv.TheMovieDbId, voteResult.ErrorMessage);
                }
            }

            return result;
        }

        [PowerUser]
        [HttpPost("reprocess/{type}/{requestId}/{is4K}")]
        public async Task<IActionResult> ReProcessRequest(RequestType type, int requestId, bool? is4K)
        {
            switch (type)
            {
                case RequestType.TvShow:
                    return Ok(await _tvRequestEngine.ReProcessRequest(requestId, false, HttpContext.RequestAborted));
                case RequestType.Movie:
                    return Ok(await _movieRequestEngine.ReProcessRequest(requestId, is4K ?? false, HttpContext.RequestAborted));
            }

            return BadRequest();
        }

        [HttpPost("movie/collection/{collectionId}")]
        public async Task<RequestEngineResult> RequestCollection(int collectionId)
        {
            return await _movieRequestEngine.RequestCollection(collectionId, HttpContext.RequestAborted);
        }

        [HttpGet("recentlyRequested")]
        public Task<IEnumerable<RecentlyRequestedModel>> RecentlyRequested()
        {
            return _recentlyRequestedService.GetRecentlyRequested(CancellationToken);
        }

        private string GetApiAlias()
        {
            // Make sure this only applies when using the API KEY
            if (HttpContext.Request.Headers.Keys.Contains("ApiKey", StringComparer.InvariantCultureIgnoreCase))
            {
                if (HttpContext.Request.Headers.TryGetValue("ApiAlias", out var apiAlias))
                {
                    return apiAlias;
                }
            }

            return null;
        }
    }
}