using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Engine.V2;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Ombi.Core;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search;
using Ombi.Models;

namespace Ombi.Controllers.V2
{
    [ApiV2]
    [Authorize]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public SearchController(IMultiSearchEngine multiSearchEngine, IMovieEngine movieEngine,
            ITvSearchEngine tvSearchEngine)
        {
            _multiSearchEngine = multiSearchEngine;
            _movieEngine = movieEngine;
            _movieEngine.ResultLimit = 12;
            _tvSearchEngine = tvSearchEngine;
            _tvSearchEngine.ResultLimit = 12;
        }

        private readonly IMultiSearchEngine _multiSearchEngine;
        private readonly IMovieEngine _movieEngine;
        private readonly ITvSearchEngine _tvSearchEngine;

        /// <summary>
        /// Returns search results for both TV and Movies
        /// </summary>
        /// <returns></returns>
        [HttpGet("multi/{searchTerm}")]
        public async Task<List<MultiSearch>> MultiSearch(string searchTerm)
        {
            return await _multiSearchEngine.MultiSearch(searchTerm);
        }

        /// <summary>
        /// Returns similar movies to the movie id passed in
        /// </summary>
        /// <remarks>
        /// We use TheMovieDb as the Movie Provider
        /// </remarks>
        [HttpPost("movie/similar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> SimilarMovies([FromBody] SimilarMoviesRefineModel model)
        {
            return await _movieEngine.SimilarMovies(model.TheMovieDbId, model.LanguageCode);
        }

       
        /// <summary>
        /// Returns Popular Movies
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/popular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> Popular()
        {
            return await _movieEngine.PopularMovies();
        }

        /// <summary>
        /// Returns Now Playing Movies
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/nowplaying")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            return await _movieEngine.NowPlayingMovies();
        }

        /// <summary>
        /// Returns top rated movies.
        /// </summary>
        /// <returns></returns>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        [HttpGet("movie/toprated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            return await _movieEngine.TopRatedMovies();
        }

        /// <summary>
        /// Returns Upcoming movies.
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            return await _movieEngine.UpcomingMovies();
        }

        /// <summary>
        /// Returns Popular Tv Shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/popular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> PopularTv()
        {
            return await _tvSearchEngine.Popular();
        }

        /// <summary>
        /// Returns most Anticiplateds tv shows.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/anticipated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> AnticipatedTv()
        {
            return await _tvSearchEngine.Anticipated();
        }


        /// <summary>
        /// Returns Most watched shows.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/mostwatched")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> MostWatched()
        {
            return await _tvSearchEngine.MostWatches();
        }

        /// <summary>
        /// Returns trending shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/trending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        {
            return await _tvSearchEngine.Trending();
        }
    }
}