using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search;
using StackExchange.Profiling;

namespace Ombi.Controllers
{
    [Authorize]
    public class SearchController : BaseV1ApiController
    {
        public SearchController(IMovieEngine movie, ITvSearchEngine tvEngine, ILogger<SearchController> logger)
        {
            MovieEngine = movie;
            TvEngine = tvEngine;
            Logger = logger;
        }
        private ILogger<SearchController> Logger { get; }

        private IMovieEngine MovieEngine { get; }
        private ITvSearchEngine TvEngine { get; }

        /// <summary>
        /// Searches for a movie.
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [HttpGet("movie/{searchTerm}")]
        public async Task<IEnumerable<SearchMovieViewModel>> SearchMovie(string searchTerm)
        {
            using (MiniProfiler.Current.Step("SearchingMovie"))
            {
                Logger.LogDebug("Searching : {searchTerm}", searchTerm);

                return await MovieEngine.Search(searchTerm);
            }
        }

        /// <summary>
        /// Gets extra information on the movie e.g. IMDBId
        /// </summary>
        /// <param name="theMovieDbId">The movie database identifier.</param>
        /// <returns></returns>
        /// <remarks>
        /// We use TheMovieDb as the Movie Provider
        /// </remarks>
        [HttpGet("movie/info/{theMovieDbId}")]
        public async Task<SearchMovieViewModel> GetExtraMovieInfo(int theMovieDbId)
        {
            return await MovieEngine.LookupImdbInformation(theMovieDbId);
        }

        /// <summary>
        /// Returns Popular Movies
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/popular")]
        public async Task<IEnumerable<SearchMovieViewModel>> Popular()
        {
            return await MovieEngine.PopularMovies();
        }
        /// <summary>
        /// Retuns Now Playing Movies
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/nowplaying")]
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            return await MovieEngine.NowPlayingMovies();
        }
        /// <summary>
        /// Returns top rated movies.
        /// </summary>
        /// <returns></returns>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        [HttpGet("movie/toprated")]
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            return await MovieEngine.TopRatedMovies();
        }
        /// <summary>
        /// Returns Upcoming movies.
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/upcoming")]
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            return await MovieEngine.UpcomingMovies();
        }

        /// <summary>
        /// Searches for a Tv Show.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <remarks>We use TvMaze as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/{searchTerm}")]
        public async Task<IEnumerable<SearchTvShowViewModel>> SearchTv(string searchTerm)
        {
            return await TvEngine.Search(searchTerm);
        }

        /// <summary>
        /// Gets extra show information.
        /// </summary>
        /// <param name="tvdbId">The TVDB identifier.</param>
        /// <remarks>We use TvMaze as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/info/{tvdbId}")]
        public async Task<SearchTvShowViewModel> GetShowInfo(int tvdbId)
        {
            return await TvEngine.GetShowInformation(tvdbId);
        }

        /// <summary>
        /// Returns Popular Tv Shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/popular")]
        public async Task<IEnumerable<SearchTvShowViewModel>> PopularTv()
        {
            return await TvEngine.Popular();
        }
        /// <summary>
        /// Returns most Anticiplateds tv shows.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/anticipated")]
        public async Task<IEnumerable<SearchTvShowViewModel>> AnticiplatedTv()
        {
            return await TvEngine.Anticipated();
        }
        /// <summary>
        /// Returns Most watched shows.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/mostwatched")]
        public async Task<IEnumerable<SearchTvShowViewModel>> MostWatched()
        {
            return await TvEngine.MostWatches();
        }
        /// <summary>
        /// Returns trending shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/trending")]
        public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        {
            return await TvEngine.Trending();
        }
    }
}
