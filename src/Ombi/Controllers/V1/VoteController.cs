using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine;
using Ombi.Core.Models;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    public class VoteController : ControllerBase
    {
        public VoteController(IVoteEngine engine)
        {
            _engine = engine;
        }

        private readonly IVoteEngine _engine;

        /// <summary>
        /// Returns the viewmodel to render on the UI
        /// </summary>
        [HttpGet]
        public Task<List<VoteViewModel>> GetView()
        {
            return _engine.GetMovieViewModel();
        }

        /// <summary>
        /// Upvotes a movie
        /// </summary>
        [HttpPost("up/movie/{requestId:int}")]
        public Task<VoteEngineResult> UpvoteMovie(int requestId)
        {
            return _engine.UpVote(requestId, RequestType.Movie);
        }

        /// <summary>
        /// Upvotes a tv show
        /// </summary>
        [HttpPost("up/tv/{requestId:int}")]
        public Task<VoteEngineResult> UpvoteTv(int requestId)
        {
            return _engine.UpVote(requestId, RequestType.TvShow);
        }

        /// <summary>
        /// Upvotes a album
        /// </summary>
        [HttpPost("up/album/{requestId:int}")]
        public Task<VoteEngineResult> UpvoteAlbum(int requestId)
        {
            return _engine.UpVote(requestId, RequestType.Album);
        }

        /// <summary>
        /// Downvotes a movie
        /// </summary>
        [HttpPost("down/movie/{requestId:int}")]
        public Task<VoteEngineResult> DownvoteMovie(int requestId)
        {
            return _engine.DownVote(requestId, RequestType.Movie);
        }

        /// <summary>
        /// Downvotes a tv show
        /// </summary>
        [HttpPost("down/tv/{requestId:int}")]
        public Task<VoteEngineResult> DownvoteTv(int requestId)
        {
            return _engine.DownVote(requestId, RequestType.TvShow);
        }

        /// <summary>
        /// Downvotes a album
        /// </summary>
        [HttpPost("down/album/{requestId:int}")]
        public Task<VoteEngineResult> DownvoteAlbum(int requestId)
        {
            return _engine.DownVote(requestId, RequestType.Album);
        }

        /// <summary>
        /// Get's all the votes for the request id
        /// </summary>
        /// <returns></returns>
        [HttpGet("movie/{requestId:int}")]
        public Task<List<Votes>> MovieVotes(int requestId)
        {
            return _engine.GetVotes(requestId, RequestType.Movie).ToListAsync();
        }

        /// <summary>
        /// Get's all the votes for the request id
        /// </summary>
        /// <returns></returns>
        [HttpGet("music/{requestId:int}")]
        public Task<List<Votes>> MusicVotes(int requestId)
        {
            return _engine.GetVotes(requestId, RequestType.Album).ToListAsync();
        }

        /// <summary>
        /// Get's all the votes for the request id
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv/{requestId:int}")]
        public Task<List<Votes>> TvVotes(int requestId)
        {
            return _engine.GetVotes(requestId, RequestType.TvShow).ToListAsync();
        }
    }
}