using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.MediaCleanup;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;

namespace Ombi.Controllers.V1
{
    [Authorize]
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class MediaCleanupController : ControllerBase
    {
        private readonly IMediaCleanupEngine _engine;

        public MediaCleanupController(IMediaCleanupEngine engine)
        {
            _engine = engine;
        }

        [HttpGet]
        [EnableRateLimiting("MediaCleanupOverviewRead")]
        public Task<MediaCleanupOverview> GetOverview(
            [FromQuery] RequestType? requestType = null,
            [FromQuery] int? requestId = null,
            [FromQuery] int? mediaId = null,
            [FromQuery] bool includeMetrics = true,
            [FromQuery] bool includeLastPlayed = true)
        {
            return _engine.GetOverview(requestType, requestId, mediaId, includeMetrics, includeLastPlayed, HttpContext.RequestAborted);
        }

        [HttpGet("tv/{requestId:int}/episodes")]
        [EnableRateLimiting("MediaCleanupTvSelectionRead")]
        public Task<MediaCleanupTvSelectionViewModel> GetTvSelection(int requestId)
        {
            return _engine.GetTvSelection(requestId);
        }

        [HttpPost("own/{requestType}/{requestId:int}")]
        [EnableRateLimiting("MediaCleanupMutation")]
        public Task<MediaCleanupActionResult> RequestOwnRemoval(RequestType requestType, int requestId, [FromBody] MediaCleanupSelection selection = null)
        {
            return _engine.RequestOwnRemoval(requestType, requestId, selection);
        }

        [HttpPost("nominate/{requestType}/{requestId:int}")]
        [EnableRateLimiting("MediaCleanupMutation")]
        public Task<MediaCleanupActionResult> Nominate(RequestType requestType, int requestId, [FromBody] MediaCleanupSelection selection = null)
        {
            return _engine.Nominate(requestType, requestId, selection);
        }

        [HttpPost("vote/{cleanupRequestId}/{vote}")]
        [EnableRateLimiting("MediaCleanupMutation")]
        public Task<MediaCleanupActionResult> Vote(string cleanupRequestId, MediaCleanupVoteType vote)
        {
            return _engine.Vote(cleanupRequestId, vote);
        }

        [HttpPost("approve/{cleanupRequestId}")]
        [EnableRateLimiting("MediaCleanupMutation")]
        public Task<MediaCleanupActionResult> Approve(string cleanupRequestId)
        {
            return _engine.Approve(cleanupRequestId);
        }

        [HttpPost("reject/{cleanupRequestId}")]
        [EnableRateLimiting("MediaCleanupMutation")]
        public Task<MediaCleanupActionResult> Reject(string cleanupRequestId)
        {
            return _engine.Reject(cleanupRequestId);
        }

        [HttpPost("cancel/{cleanupRequestId}")]
        [EnableRateLimiting("MediaCleanupMutation")]
        public Task<MediaCleanupActionResult> Cancel(string cleanupRequestId)
        {
            return _engine.Cancel(cleanupRequestId);
        }
    }
}
