using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Models;
using Ombi.Models;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    [Authorize]
    public class RecentlyAddedController : Controller
    {
        public RecentlyAddedController(IRecentlyAddedEngine engine)
        {
            _recentlyAdded = engine;
        }

        private readonly IRecentlyAddedEngine _recentlyAdded;

        /// <summary>
        /// Returns the recently added movies between two dates
        /// </summary>
        [HttpPost("movies")]
        [ProducesResponseType(typeof(IEnumerable<RecentlyAddedMovieModel>), 200)]
        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies([FromBody] RecentlyAddedRangeModel model)
        {
            return _recentlyAdded.GetRecentlyAddedMovies(model.From, model.To);
        }
    }
}