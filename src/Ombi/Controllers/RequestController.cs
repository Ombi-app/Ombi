using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;
using System.Diagnostics;
using Ombi.Core.Models.UI;
using Ombi.Models;
using Ombi.Store.Entities;

namespace Ombi.Controllers
{
    [Authorize]
    [ApiV1]
    [Produces("application/json")]
    public class RequestController : Controller
    {
        public RequestController(IMovieRequestEngine engine, ITvRequestEngine tvRequestEngine)
        {
            MovieRequestEngine = engine;
            TvRequestEngine = tvRequestEngine;
        }

        private IMovieRequestEngine MovieRequestEngine { get; }
        private ITvRequestEngine TvRequestEngine { get; }

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
            return await MovieRequestEngine.RequestMovie(movie);
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
        public async Task DeleteRequest(int requestId)
        {
            await MovieRequestEngine.RemoveMovieRequest(requestId);
        }

        /// <summary>
        /// Updates the specified movie request.
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPut("movie")]
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
        public async Task<RequestEngineResult> DenyMovie([FromBody] MovieUpdateModel model)
        {
            return await MovieRequestEngine.DenyMovieById(model.Id);
        }

        /// <summary>
        /// Gets the tv requests.
        /// </summary>
        /// <param name="count">The count of items you want to return.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        [HttpGet("tv/{count:int}/{position:int}/tree")]
        public async Task<IEnumerable<TreeNode<TvRequests, List<ChildRequests>>>> GetTvRequestsTree(int count, int position)
        {
            return await TvRequestEngine.GetRequestsTreeNode(count, position);
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
            return await TvRequestEngine.RequestTvShow(tv);
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
        /// Searches for a specific tv request
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [HttpGet("tv/search/{searchTerm}/tree")]
        public async Task<IEnumerable<TreeNode<TvRequests, List<ChildRequests>>>> SearchTvTree(string searchTerm)
        {
            return await TvRequestEngine.SearchTvRequestTree(searchTerm);
        }

        /// <summary>
        /// Deletes the a specific tv request
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        [HttpDelete("tv/{requestId:int}")]
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
        public async Task<TvRequests> UpdateRequest([FromBody] TvRequests model)
        {
            return await TvRequestEngine.UpdateTvRequest(model);
        }

        /// <summary>
        /// Updates the a specific child request
        /// </summary>
        /// <param name="child">The model.</param>
        /// <returns></returns>
        [HttpPut("tv/child")]
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
        public async Task<RequestEngineResult> DenyChild([FromBody] TvUpdateModel model)
        {
            return await TvRequestEngine.DenyChildRequest(model.Id);
        }

        /// <summary>
        /// Set's the specified tv child as available 
        /// </summary>
        /// <param name="model">The Movie's ID</param>
        /// <returns></returns>
        [HttpPost("tv/available")]
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
        public async Task<RequestEngineResult> ApproveChild([FromBody] TvUpdateModel model)
        {
            return await TvRequestEngine.ApproveChildRequest(model.Id);
        }

        /// <summary>
        /// Deletes the a specific tv request
        /// </summary>
        /// <param name="requestId">The model.</param>
        /// <returns></returns>
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
    }
}