using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search;
using Ombi.Models;
using Ombi.Core.Engine.Demo;
using Ombi.Helpers;

namespace Ombi.Controllers.V1
{
    [Authorize]
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class SearchController : Controller
    {
        public SearchController(IMovieEngine movie, ITvSearchEngine tvEngine, ILogger<SearchController> logger, IMusicSearchEngine music,
            IDemoMovieSearchEngine demoMovieSearch, IDemoTvSearchEngine demoTvSearchEngine)
        {
            MovieEngine = movie;
            TvEngine = tvEngine;
            Logger = logger;
            MusicEngine = music;
            DemoMovieSearch = demoMovieSearch;
            DemoTvSearch = demoTvSearchEngine;
            IsDemo = DemoSingleton.Instance.Demo;
        }

        private ILogger<SearchController> Logger { get; }

        private IMovieEngine MovieEngine { get; }
        private ITvSearchEngine TvEngine { get; }
        private IMusicSearchEngine MusicEngine { get; }
        private IDemoMovieSearchEngine DemoMovieSearch { get; }
        private IDemoTvSearchEngine DemoTvSearch { get; }
        private readonly bool IsDemo;

        /// <summary>
        /// Searches for a movie.
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [HttpGet("movie/{searchTerm}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> SearchMovie(string searchTerm)
        {
            Logger.LogDebug("Searching : {searchTerm}", searchTerm);

            if (IsDemo)
            {
                return await DemoMovieSearch.Search(searchTerm);
            }
            return await MovieEngine.Search(searchTerm, null, null);
        }

        /// <summary>
        /// Searches for movies by a certain actor.
        /// </summary>
        /// <param name="model">language code is optional, by default it will be en. Language code uses ISO 639-1</param>
        /// <returns></returns>
        [HttpPost("movie/actor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> SearchActor([FromBody] SearchActorModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            return Json(await MovieEngine.SearchActor(model.SearchTerm, model.LanguageCode));
        }

        /// <summary>
        /// Searches for a movie.
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <param name="model">The refinement model, language code and year are both optional. Language code uses ISO 639-1</param>
        /// <returns></returns>
        [HttpPost("movie")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> SearchMovie([FromBody] SearchMovieRefineModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            Logger.LogDebug("Searching : {0}, Year: {1}, Lang: {2}", model.SearchTerm, model.Year, model.LanguageCode);

            return Json(await MovieEngine.Search(model.SearchTerm, model.Year, model.LanguageCode));
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<SearchMovieViewModel> GetExtraMovieInfo(int theMovieDbId)
        {
            return await MovieEngine.LookupImdbInformation(theMovieDbId);
        }


        /// <summary>
        /// Gets extra information on the movie e.g. IMDBId
        /// </summary>
        /// <param name="model">TheMovieDb and Language Code, Pass in the language code (ISO 639-1) to get it back in a different lang </param>
        /// <returns></returns>
        /// <remarks>
        /// We use TheMovieDb as the Movie Provider
        /// </remarks>
        [HttpPost("movie/info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetExtraMovieInfo([FromBody] SearchMovieExtraInfoRefineModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }
            return Json(await MovieEngine.LookupImdbInformation(model.TheMovieDbId, model.LanguageCode));
        }

        /// <summary>
        /// Returns similar movies to the movie id passed in
        /// </summary>
        /// <param name="model">the movie</param>
        /// <remarks>
        /// We use TheMovieDb as the Movie Provider
        /// </remarks>
        [HttpPost("movie/similar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> SimilarMovies([FromBody] SimilarMoviesRefineModel model)
        {
            return await MovieEngine.SimilarMovies(model.TheMovieDbId, model.LanguageCode);
        }

        /// <summary>
        /// Returns similar movies to the movie id passed in
        /// </summary>
        /// <param name="theMovieDbId">ID of the movie</param>
        /// <remarks>
        /// We use TheMovieDb as the Movie Provider
        /// </remarks>
        [HttpGet("movie/{theMovieDbId}/similar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> SimilarMovies(int theMovieDbId)
        {
            return await MovieEngine.SimilarMovies(theMovieDbId, null);
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
            if (IsDemo)
            {
                return await DemoMovieSearch.PopularMovies();
            }
            return await MovieEngine.PopularMovies();
        }
        /// <summary>
        /// Retuns Now Playing Movies
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/nowplaying")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            if (IsDemo)
            {
                return await DemoMovieSearch.NowPlayingMovies();
            }
            return await MovieEngine.NowPlayingMovies();
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
            if (IsDemo)
            {
                return await DemoMovieSearch.TopRatedMovies();
            }
            return await MovieEngine.TopRatedMovies();
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
            if (IsDemo)
            {
                return await DemoMovieSearch.UpcomingMovies();
            }
            return await MovieEngine.UpcomingMovies();
        }

        /// <summary>
        /// Searches for a Tv Show.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <remarks>We use TvMaze as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/{searchTerm}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> SearchTv(string searchTerm)
        {
            if (IsDemo)
            {
                return await DemoTvSearch.Search(searchTerm);
            }
            return await TvEngine.Search(searchTerm);
        }

        /// <summary>
        /// Gets extra show information.
        /// </summary>
        /// <param name="tvdbId">The TVDB identifier.</param>
        /// <remarks>We use TvMaze as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/info/{tvdbId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> PopularTv()
        {
            if (IsDemo)
            {
                return await DemoTvSearch.NowPlayingMovies();
            }
            return await TvEngine.Popular();
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
            if (IsDemo)
            {
                return await DemoTvSearch.NowPlayingMovies();
            }
            return await TvEngine.Anticipated();
        }


        /// <summary>
        /// Returns Most watched shows.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/mostwatched")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Obsolete("This method is obsolete, Trakt API no longer supports this")]
        public async Task<IEnumerable<SearchTvShowViewModel>> MostWatched()
        {
            if (IsDemo)
            {
                return await DemoTvSearch.NowPlayingMovies();
            }
            return await TvEngine.Popular();
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
            if (IsDemo)
            {
                return await DemoTvSearch.NowPlayingMovies();
            }
            return await TvEngine.Trending();
        }

        /// <summary>
        /// Returns the artist information we searched for
        /// </summary>
        /// <remarks>We use Lidarr as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("music/artist/{searchTerm}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchArtistViewModel>> SearchArtist(string searchTerm)
        {
            return await MusicEngine.SearchArtist(searchTerm);
        }

        /// <summary>
        /// Returns the album information we searched for
        /// </summary>
        /// <remarks>We use Lidarr as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("music/album/{searchTerm}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchAlbumViewModel>> SearchAlbum(string searchTerm)
        {
            return await MusicEngine.SearchAlbum(searchTerm);
        }

        /// <summary>
        /// Returns the album information specified by the foreignAlbumId passed in
        /// </summary>
        /// <remarks>We use Lidarr as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("music/album/info/{foreignAlbumId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<SearchAlbumViewModel> GetAlbumInformation(string foreignAlbumId)
        {
            return await MusicEngine.GetAlbumInformation(foreignAlbumId);
        }

        /// <summary>
        /// Returns all albums for the artist using the ForeignArtistId
        /// </summary>
        /// <remarks>We use Lidarr as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("music/artist/album/{foreignArtistId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchAlbumViewModel>> GetAlbumsByArtist(string foreignArtistId)
        {
            return await MusicEngine.GetArtistAlbums(foreignArtistId);
        }
    }
}
