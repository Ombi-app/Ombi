using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Models;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class RecentlyAddedController : ControllerBase
    {
        public RecentlyAddedController(IRecentlyAddedEngine engine)
        {
            _recentlyAdded = engine;
        }

        private readonly IRecentlyAddedEngine _recentlyAdded;

        /// <summary>
        /// Returns the recently added movies for the past 7 days
        /// </summary>
        [HttpGet("movies")]
        [ProducesResponseType(typeof(IEnumerable<RecentlyAddedMovieModel>), 200)]
        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies()
        {
            return _recentlyAdded.GetRecentlyAddedMovies(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        }

        /// <summary>
        /// Returns the recently added tv shows for the past 7 days
        /// </summary>
        [HttpGet("tv")]
        [ProducesResponseType(typeof(IEnumerable<RecentlyAddedMovieModel>), 200)]
        public IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedShows()
        {
            return _recentlyAdded.GetRecentlyAddedTv(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, false);
        }

        /// <summary>
        /// Returns the recently added tv shows for the past 7 days and groups them by season
        /// </summary>
        [HttpGet("tv/grouped")]
        [ProducesResponseType(typeof(IEnumerable<RecentlyAddedMovieModel>), 200)]
        public IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedShowsGrouped()
        {
            return _recentlyAdded.GetRecentlyAddedTv(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, true);
        }
    }
}