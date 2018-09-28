using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;

namespace Ombi.Controllers
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    public class VoteController : Controller
    {
        public VoteController(IVoteEngine engine)
        {
            _engine = engine;
        }

        private readonly IVoteEngine _engine;

        [HttpGet]
        public Task<List<VoteViewModel>> GetView()
        {
            return _engine.GetMovieViewModel();
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