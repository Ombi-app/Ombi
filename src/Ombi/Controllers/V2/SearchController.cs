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
using Ombi.Core.Models.Search.V2;
using Ombi.Models;

namespace Ombi.Controllers.V2
{
    [ApiV2]
    [Authorize]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public SearchController(IMultiSearchEngine multiSearchEngine,
            ITvSearchEngine tvSearchEngine, IMovieEngineV2 v2Movie, ITVSearchEngineV2 v2Tv)
        {
            _multiSearchEngine = multiSearchEngine;
            _tvSearchEngine = tvSearchEngine;
            _tvSearchEngine.ResultLimit = 12;
            _movieEngineV2 = v2Movie;
            _movieEngineV2.ResultLimit = 12;
            _tvEngineV2 = v2Tv;
        }

        private readonly IMultiSearchEngine _multiSearchEngine;
        private readonly IMovieEngineV2 _movieEngineV2;
        private readonly ITVSearchEngineV2 _tvEngineV2;
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
        /// Returns details for a single movie
        /// </summary>
        [HttpGet("movie/{movieDbId}")]
        public async Task<MovieFullInfoViewModel> GetMovieInfo(int movieDbId)
        {
            return await _movieEngineV2.GetFullMovieInformation(movieDbId);
        }


        /// <summary>
        /// Returns details for a single show
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv/{tvdbId}")]
        public async Task<SearchFullInfoTvShowViewModel> GetTvInfo(int tvdbid)
        {
            return await _tvEngineV2.GetShowInformation(tvdbid);
        }

        /// <summary>
        /// Returns details for a single show
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv/moviedb/{moviedbid}")]
        public async Task<SearchFullInfoTvShowViewModel> GetTvInfoByMovieId(int moviedbid)
        {
            var tvDbId = await _movieEngineV2.GetTvDbId(moviedbid);
            return await _tvEngineV2.GetShowInformation(tvDbId);
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
            return await _movieEngineV2.SimilarMovies(model.TheMovieDbId, model.LanguageCode);
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
            return await _movieEngineV2.PopularMovies();
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
            return await _movieEngineV2.NowPlayingMovies();
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
            return await _movieEngineV2.TopRatedMovies();
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
            return await _movieEngineV2.UpcomingMovies();
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