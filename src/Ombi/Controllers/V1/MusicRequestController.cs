using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Attributes;
using Ombi.Core.Engine;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Ombi.Controllers.V1
{
    [Authorize]
    [Route("api/v1/request/music")]
    [Produces("application/json")]
    [ApiController]
    public class MusicRequestController : ControllerBase
    {
        public MusicRequestController(IMusicRequestEngine engine, IVoteEngine voteEngine, ILogger<MusicRequestController> log)
        {
            _engine = engine;
            _voteEngine = voteEngine;
            _log = log;
        }

        private readonly IMusicRequestEngine _engine;
        private readonly IVoteEngine _voteEngine;
        private readonly ILogger _log;

        /// <summary>
        /// Gets album requests.
        /// </summary>
        /// <param name="count">The count of items you want to return.</param>
        /// <param name="position">The position.</param>
        /// <param name="orderType"> The way we want to order.</param>
        /// <param name="statusType"></param>
        /// <param name="availabilityType"></param>
        [HttpGet("{count:int}/{position:int}/{orderType:int}/{statusType:int}/{availabilityType:int}")]
        public async Task<RequestsViewModel<AlbumRequest>> GetRequests(int count, int position, int orderType, int statusType, int availabilityType)
        {
            return await _engine.GetRequests(count, position, new OrderFilterModel
            {
                OrderType = (OrderType)orderType,
                AvailabilityFilter = (FilterType)availabilityType,
                StatusFilter = (FilterType)statusType,
            });
        }

        /// <summary>
        /// Gets the total amount of album requests.
        /// </summary>
        [HttpGet("total")]
        public async Task<int> GetTotalAlbums()
        {
            return await _engine.GetTotal();
        }

        /// <summary>
        /// Gets all album requests.
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<AlbumRequest>> GetRequests()
        {
            return await _engine.GetRequests();
        }

        /// <summary>
        /// Requests a album.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RequestEngineResult> RequestAlbum([FromBody] MusicAlbumRequestViewModel album)
        {
            album.RequestedByAlias = GetApiAlias();
            var result = await _engine.RequestAlbum(album);
            if (result.Result)
            {
                var voteResult = await _voteEngine.UpVote(result.RequestId, RequestType.Album);
                if (voteResult.IsError)
                {
                    _log.LogError("Couldn't automatically add the vote for the album {0} because {1}", album.ForeignAlbumId, voteResult.ErrorMessage);
                }
            }

            return result;
        }

        /// <summary>
        /// Searches for a specific album request
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<AlbumRequest>> Search(string searchTerm)
        {
            return await _engine.SearchAlbumRequest(searchTerm);
        }

        /// <summary>
        /// Deletes the specified album request.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        [HttpDelete("{requestId:int}")]
        [Authorize(Roles = "Admin,PowerUser,ManageOwnRequests")]
        public async Task DeleteRequest(int requestId)
        {
            await _engine.RemoveAlbumRequest(requestId);
        }

        /// <summary>
        /// Approves the specified album request.
        /// </summary>
        /// <param name="model">The albums's ID</param>
        /// <returns></returns>
        [HttpPost("approve")]
        [PowerUser]
        public async Task<RequestEngineResult> ApproveAlbum([FromBody] AlbumUpdateModel model)
        {
            return await _engine.ApproveAlbumById(model.Id);
        }

        /// <summary>
        /// Set's the specified album as available 
        /// </summary>
        /// <param name="model">The album's ID</param>
        /// <returns></returns>
        [HttpPost("available")]
        [PowerUser]
        public async Task<RequestEngineResult> MarkAvailable([FromBody] AlbumUpdateModel model)
        {
            return await _engine.MarkAvailable(model.Id);
        }

        /// <summary>
        /// Set's the specified album as unavailable 
        /// </summary>
        /// <param name="model">The album's ID</param>
        /// <returns></returns>
        [HttpPost("unavailable")]
        [PowerUser]
        public async Task<RequestEngineResult> MarkUnAvailable([FromBody] AlbumUpdateModel model)
        {
            return await _engine.MarkUnavailable(model.Id);
        }

        /// <summary>
        /// Denies the specified album request.
        /// </summary>
        /// <param name="model">The album's ID</param>
        /// <returns></returns>
        [HttpPut("deny")]
        [PowerUser]
        public async Task<RequestEngineResult> Deny([FromBody] DenyAlbumModel model)
        {
            return await _engine.DenyAlbumById(model.Id, model.Reason);
        }

        /// <summary>
        /// Gets model containing remaining number of music requests.
        /// </summary>
        [HttpGet("remaining")]
        public async Task<RequestQuotaCountModel> GetRemainingMusicRequests()
        {
            return await _engine.GetRemainingRequests();
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