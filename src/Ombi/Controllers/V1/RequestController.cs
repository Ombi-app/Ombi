using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Attributes;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Controllers.V1
{
    [Authorize]
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        public RequestController(IMovieRequestEngine engine, ITvRequestEngine tvRequestEngine, IVoteEngine vote,
            ILogger<RequestController> log)
        {
            MovieRequestEngine = engine;
            TvRequestEngine = tvRequestEngine;
            VoteEngine = vote;
            Log = log;
        }

        private IMovieRequestEngine MovieRequestEngine { get; }
        private ITvRequestEngine TvRequestEngine { get; }
        private IVoteEngine VoteEngine { get; }
        private ILogger Log { get; }

        /// <summary>
        /// Gets movie requests.
        /// </summary>
        /// <param name="count">The count of items you want to return.</param>
        /// <param name="position">The position.</param>
        /// <param name="orderType"> The way we want to order.</param>
        /// <param name="statusType"></param>
        /// <param name="availabilityType"></param>
        [HttpGet("movie/{count:int}/{position:int}/{orderType:int}/{statusType:int}/{availabilityType:int}")]
        public async Task<RequestsViewModel<MovieRequests>> GetRequests(int count, int position, int orderType, int statusType, int availabilityType)
        {
            return await MovieRequestEngine.GetRequests(count, position, new OrderFilterModel
            {
                OrderType = (OrderType)orderType,
                AvailabilityFilter = (FilterType)availabilityType,
                StatusFilter = (FilterType)statusType,
            });
        }

        /// <summary>
        /// Returns information about the Single Movie Request
        /// </summary>
        /// <param name="requestId">the movie request id</param>
        [HttpGet("movie/info/{requestId}")]
        public async Task<MovieRequests> GetMovieRequest(int requestId)
        {
            return await MovieRequestEngine.GetRequest(requestId);
        }

        /// <summary>
        /// Gets the total amount of movie requests.
        /// </summary>
        [HttpGet("movie/total")]
        public async Task<int> GetTotalMovies()
        {
            return await MovieRequestEngine.GetTotal();
        }

        /// <summary>
        /// Gets all movie requests.
        /// </summary>
        [HttpGet("movie")]
        public async Task<IEnumerable<MovieRequests>> GetRequests()
        {
            return await MovieRequestEngine.GetRequests();
        }

        /// <summary>
        /// Requests a movie.
        /// </summary>
        /// <param name="movie">The movie.</param>
        /// <returns></returns>
        [HttpPost("movie")]
        public async Task<RequestEngineResult> RequestMovie([FromBody] MovieRequestViewModel movie)
        {
            movie.RequestedByAlias = GetApiAlias();
            var result = await MovieRequestEngine.RequestMovie(movie);
            if (result.Result)
            {
                var voteResult = await VoteEngine.UpVote(result.RequestId, RequestType.Movie);
                if (voteResult.IsError)
                {
                    Log.LogError("Couldn't automatically add the vote for the movie {0} because {1}", movie.TheMovieDbId, voteResult.ErrorMessage);
                }
            }

            return result;
        }

        /// <summary>
        /// Searches for a specific movie request
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [HttpGet("movie/search/{searchTerm}")]
        public async Task<IEnumerable<MovieRequests>> Search(string searchTerm)
        {
            return await MovieRequestEngine.SearchMovieRequest(searchTerm);
        }

        /// <summary>
        /// Deletes the specified movie request.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        [HttpDelete("movie/{requestId:int}")]
        [Authorize(Roles = "Admin,PowerUser,ManageOwnRequests")]
        public async Task DeleteRequest(int requestId)
        {
            await MovieRequestEngine.RemoveMovieRequest(requestId);
        }

        /// <summary>
        /// Deletes the all movie request.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("movie/all")]
        [PowerUser]
        public async Task DeleteAllRequests()
        {
            await MovieRequestEngine.RemoveAllMovieRequests();
        }

        /// <summary>
        /// Updates the specified movie request.
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPut("movie")]
        [PowerUser]
        public async Task<MovieRequests> UpdateRequest([FromBody] MovieRequests model)
        {
            return await MovieRequestEngine.UpdateMovieRequest(model);
        }

        /// <summary>
        /// Approves the specified movie request.
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPost("movie/approve")]
        [PowerUser]
        public async Task<RequestEngineResult> ApproveMovie([FromBody] MovieUpdateModel model)
        {
            return await MovieRequestEngine.ApproveMovieById(model.Id);
        }

        /// <summary>
        /// Set's the specified Movie as available 
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPost("movie/available")]
        [PowerUser]
        public async Task<RequestEngineResult> MarkMovieAvailable([FromBody] MovieUpdateModel model)
        {
            return await MovieRequestEngine.MarkAvailable(model.Id);
        }

        /// <summary>
        /// Set's the specified Movie as unavailable 
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPost("movie/unavailable")]
        [PowerUser]
        public async Task<RequestEngineResult> MarkMovieUnAvailable([FromBody] MovieUpdateModel model)
        {
            return await MovieRequestEngine.MarkUnavailable(model.Id);
        }

        /// <summary>
        /// Denies the specified movie request.
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPut("movie/deny")]
        [PowerUser]
        public async Task<RequestEngineResult> DenyMovie([FromBody] DenyMovieModel model)
        {
            return await MovieRequestEngine.DenyMovieById(model.Id, model.Reason);
        }

        /// <summary>
        /// Gets the total amount of TV requests.
        /// </summary>
        [HttpGet("tv/total")]
        public async Task<int> GetTotalTV()
        {
            return await TvRequestEngine.GetTotal();
        }

        /// <summary>
        /// Gets the tv requests.
        /// </summary>
        /// <param name="count">The count of items you want to return.</param>
        /// <param name="position">The position.</param>
        /// <param name="orderType"></param>
        /// <param name="statusType"></param>
        /// <param name="availabilityType"></param>
        /// <returns></returns>
        [HttpGet("tv/{count:int}/{position:int}/{orderType:int}/{statusFilterType:int}/{availabilityFilterType:int}")]
        public async Task<RequestsViewModel<TvRequests>> GetTvRequests(int count, int position, int orderType, int statusType, int availabilityType)
        {
            return await TvRequestEngine.GetRequests(count, position, new OrderFilterModel
            {
                OrderType = (OrderType)orderType,
                AvailabilityFilter = (FilterType)availabilityType,
                StatusFilter = (FilterType)statusType,
            });
        }

        /// <summary>
        /// Gets the tv requests lite.
        /// </summary>
        /// <param name="count">The count of items you want to return.</param>
        /// <param name="position">The position.</param>
        /// <param name="orderType"></param>
        /// <param name="statusType"></param>
        /// <param name="availabilityType"></param>
        /// <returns></returns>
        [HttpGet("tvlite/{count:int}/{position:int}/{orderType:int}/{statusFilterType:int}/{availabilityFilterType:int}")]
        public async Task<RequestsViewModel<TvRequests>> GetTvRequestsLite(int count, int position, int orderType, int statusType, int availabilityType)
        {
            return await TvRequestEngine.GetRequestsLite(count, position, new OrderFilterModel
            {
                OrderType = (OrderType)orderType,
                AvailabilityFilter = (FilterType)availabilityType,
                StatusFilter = (FilterType)statusType,
            });
        }

        /// <summary>
        /// Gets the tv requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv")]
        public async Task<IEnumerable<TvRequests>> GetTvRequests()
        {
            return await TvRequestEngine.GetRequests();
        }

        /// <summary>
        /// Gets the tv requests without the whole object graph (Does not include seasons/episodes).
        /// </summary>
        /// <returns></returns>
        [HttpGet("tvlite")]
        public async Task<IEnumerable<TvRequests>> GetTvRequestsLite()
        {
            return await TvRequestEngine.GetRequestsLite();
        }

        /// <summary>
        /// Returns the full request object for the specified requestId
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpGet("tv/{requestId:int}")]
        public async Task<TvRequests> GetTvRequest(int requestId)
        {
            return await TvRequestEngine.GetTvRequest(requestId);
        }

        /// <summary>
        /// Requests a tv show/episode/season.
        /// </summary>
        /// <param name="tv">The tv.</param>
        /// <returns></returns>
        [HttpPost("tv")]
        public async Task<RequestEngineResult> RequestTv([FromBody] TvRequestViewModel tv)
        {
            tv.RequestedByAlias = GetApiAlias();
            var result = await TvRequestEngine.RequestTvShow(tv);
            if (result.Result)
            {
                var voteResult = await VoteEngine.UpVote(result.RequestId, RequestType.TvShow);
                if (voteResult.IsError)
                {
                    Log.LogError("Couldn't automatically add the vote for the tv {0} because {1}", tv.TvDbId, voteResult.ErrorMessage);
                }
            }

            return result;
        }

        /// <summary>
        /// Searches for a specific tv request
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [HttpGet("tv/search/{searchTerm}")]
        public async Task<IEnumerable<TvRequests>> SearchTv(string searchTerm)
        {
            return await TvRequestEngine.SearchTvRequest(searchTerm);
        }

        /// <summary>
        /// Deletes the a specific tv request
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        [HttpDelete("tv/{requestId:int}")]
        [Authorize(Roles = "Admin,PowerUser,ManageOwnRequests")]
        public async Task DeleteTvRequest(int requestId)
        {
            await TvRequestEngine.RemoveTvRequest(requestId);
        }

        /// <summary>
        /// Updates the a specific tv request
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPut("tv")]
        [PowerUser]
        public async Task<TvRequests> UpdateRequest([FromBody] TvRequests model)
        {
            return await TvRequestEngine.UpdateTvRequest(model);
        }

        /// <summary>
        /// Updates the root path for this tv show
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="rootFolderId"></param>
        /// <returns></returns>
        [HttpPut("tv/root/{requestId:int}/{rootFolderId:int}")]
        [PowerUser]
        public async Task<bool> UpdateRootFolder(int requestId, int rootFolderId)
        {
            await TvRequestEngine.UpdateRootPath(requestId, rootFolderId);
            return true;
        }

        /// <summary>
        /// Updates the quality profile for this tv show
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="qualityId"></param>
        /// <returns></returns>
        [HttpPut("tv/quality/{requestId:int}/{qualityId:int}")]
        [PowerUser]
        public async Task<bool> UpdateQuality(int requestId, int qualityId)
        {
            await TvRequestEngine.UpdateQualityProfile(requestId, qualityId);
            return true;
        }

        /// <summary>
        /// Updates the a specific child request
        /// </summary>
        /// <param name="child">The model.</param>
        /// <returns></returns>
        [HttpPut("tv/child")]
        [PowerUser]
        public async Task<ChildRequests> UpdateChild([FromBody] ChildRequests child)
        {
            return await TvRequestEngine.UpdateChildRequest(child);
        }

        /// <summary>
        /// Denies the a specific child request
        /// </summary>
        /// <param name="model">This is the child request's ID</param>
        /// <returns></returns>
        [HttpPut("tv/deny")]
        [PowerUser]
        public async Task<RequestEngineResult> DenyChild([FromBody] DenyTvModel model)
        {
            return await TvRequestEngine.DenyChildRequest(model.Id, model.Reason);
        }

        /// <summary>
        /// Set's the specified tv child as available 
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPost("tv/available")]
        [PowerUser]
        public async Task<RequestEngineResult> MarkTvAvailable([FromBody] TvUpdateModel model)
        {
            return await TvRequestEngine.MarkAvailable(model.Id);
        }

        /// <summary>
        /// Set's the specified tv child as unavailable 
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPost("tv/unavailable")]
        [PowerUser]
        public async Task<RequestEngineResult> MarkTvUnAvailable([FromBody] TvUpdateModel model)
        {
            return await TvRequestEngine.MarkUnavailable(model.Id);
        }

        /// <summary>
        /// Updates the a specific child request
        /// </summary>
        /// <param name="model">This is the child request's ID</param>
        /// <returns></returns>
        [HttpPost("tv/approve")]
        [PowerUser]
        public async Task<RequestEngineResult> ApproveChild([FromBody] TvUpdateModel model)
        {
            return await TvRequestEngine.ApproveChildRequest(model.Id);
        }

        /// <summary>
        /// Deletes the a specific tv request
        /// </summary>
        /// <param name="requestId">The model.</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,PowerUser,ManageOwnRequests")]
        [HttpDelete("tv/child/{requestId:int}")]
        public async Task<bool> DeleteChildRequest(int requestId)
        {
            await TvRequestEngine.RemoveTvChild(requestId);
            return true;
        }


        /// <summary>
        /// Retuns all children requests for the request id
        /// </summary>
        /// <param name="requestId">The Request Id</param>
        /// <returns></returns>
        [HttpGet("tv/{requestId:int}/child")]
        public async Task<IEnumerable<ChildRequests>> GetAllChildren(int requestId)
        {
            return await TvRequestEngine.GetAllChldren(requestId);
        }

        /// <summary>
        /// Gets the count of total requests
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        [AllowAnonymous]
        public RequestCountModel GetCountOfRequests()
        {
            // Doesn't matter if we use the TvEngine or MovieEngine, this method is in the base class
            return TvRequestEngine.RequestCount();
        }

        /// <summary>
        /// Checks if the passed in user has a request
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("userhasrequest")]
        public async Task<bool> UserHasRequest(string userId)
        {
            var movies = await MovieRequestEngine.UserHasRequest(userId);
            var tv = await TvRequestEngine.UserHasRequest(userId);

            return movies || tv;
        }

        /// <summary>
        /// Subscribes for notifications to a movie request
        /// </summary>
        [HttpPost("movie/subscribe/{requestId:int}")]
        public async Task<bool> SubscribeToMovie(int requestId)
        {
            await MovieRequestEngine.SubscribeToRequest(requestId, RequestType.Movie);
            return true;
        }

        /// <summary>
        /// Subscribes for notifications to a TV request
        /// </summary>
        [HttpPost("tv/subscribe/{requestId:int}")]
        public async Task<bool> SubscribeToTv(int requestId)
        {
            await TvRequestEngine.SubscribeToRequest(requestId, RequestType.TvShow);
            return true;
        }

        /// <summary>
        /// UnSubscribes for notifications to a movie request
        /// </summary>
        [HttpPost("movie/unsubscribe/{requestId:int}")]
        public async Task<bool> UnSubscribeToMovie(int requestId)
        {
            await MovieRequestEngine.UnSubscribeRequest(requestId, RequestType.Movie);
            return true;
        }

        /// <summary>
        /// UnSubscribes for notifications to a TV request
        /// </summary>
        [HttpPost("tv/unsubscribe/{requestId:int}")]
        public async Task<bool> UnSubscribeToTv(int requestId)
        {
            await TvRequestEngine.UnSubscribeRequest(requestId, RequestType.TvShow);
            return true;
        }

        /// <summary>
        /// Gets model containing remaining number of movie requests.
        /// </summary>
        [HttpGet("movie/remaining")]
        public async Task<RequestQuotaCountModel> GetRemainingMovieRequests()
        {
            return await MovieRequestEngine.GetRemainingRequests();
        }

        /// <summary>
        /// Gets model containing remaining number of tv requests.
        /// </summary>
        [HttpGet("tv/remaining")]
        public async Task<RequestQuotaCountModel> GetRemainingTvRequests()
        {
            return await TvRequestEngine.GetRemainingRequests();
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