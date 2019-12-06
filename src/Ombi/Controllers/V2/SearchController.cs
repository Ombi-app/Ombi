using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Ombi.Core;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Engine.V2;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Models.Search.V2.Music;
using Ombi.Models;

namespace Ombi.Controllers.V2
{
    public class SearchController : V2Controller
    {
        public SearchController(IMultiSearchEngine multiSearchEngine, ITvSearchEngine tvSearchEngine,
            IMovieEngineV2 v2Movie, ITVSearchEngineV2 v2Tv, IMusicSearchEngineV2 musicEngine)
        {
            _multiSearchEngine = multiSearchEngine;
            _tvSearchEngine = tvSearchEngine;
            _tvSearchEngine.ResultLimit = 12;
            _movieEngineV2 = v2Movie;
            _movieEngineV2.ResultLimit = 12;
            _tvEngineV2 = v2Tv;
            _musicEngine = musicEngine;
        }

        private readonly IMultiSearchEngine _multiSearchEngine;
        private readonly IMovieEngineV2 _movieEngineV2;
        private readonly ITVSearchEngineV2 _tvEngineV2;
        private readonly ITvSearchEngine _tvSearchEngine;
        private readonly IMusicSearchEngineV2 _musicEngine;

        /// <summary>
        /// Returns search results for both TV and Movies
        /// </summary>
        /// <remarks>The ID's returned by this are all TheMovieDbID's even for the TV Shows. You can call <see cref="GetTvInfoByMovieId"/> to get TV
        ///  Show information using the MovieDbId.</remarks>
        /// <param name="searchTerm">The search you want, this can be for a movie or TV show e.g. Star Wars will return
        ///  all Star Wars movies and Star Wars Rebels the TV Sho</param>
        /// <returns></returns>
        [HttpGet("multi/{searchTerm}")]
        public async Task<List<MultiSearchResult>> MultiSearch(string searchTerm)
        {
            return await _multiSearchEngine.MultiSearch(searchTerm, Request.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns details for a single movie
        /// </summary>
        /// <param name="movieDbId">The MovieDB Id</param>
        [HttpGet("movie/{movieDbId}")]
        public async Task<MovieFullInfoViewModel> GetMovieInfo(int movieDbId)
        {
            return await _movieEngineV2.GetFullMovieInformation(movieDbId, Request.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns basic information about the provided collection
        /// </summary>
        /// <param name="collectionId">The collection id from TheMovieDb</param>
        /// <returns></returns>
        [HttpGet("movie/collection/{collectionId}")]
        public async Task<MovieCollectionsViewModel> GetMovieCollections(int collectionId)
        {
            return await _movieEngineV2.GetCollection(collectionId, Request.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns details for a single show
        /// </summary>
        /// <remarks>TVMaze is the TV Show Provider</remarks>
        /// <param name="tvdbid">The TVDB Id</param>
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
        /// Returns Popular Movies using paging
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/popular/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> Popular(int currentPosition, int amountToLoad)
        {
            return await _movieEngineV2.PopularMovies(currentPosition, amountToLoad, Request.HttpContext.RequestAborted);
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
        /// Returns Now Playing Movies by page
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/nowplaying/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies(int currentPosition, int amountToLoad)
        {
            return await _movieEngineV2.NowPlayingMovies(currentPosition, amountToLoad);
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
        /// Returns top rated movies by page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        [HttpGet("movie/toprated/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies(int currentPosition, int amountToLoad)
        {
            return await _movieEngineV2.TopRatedMovies(currentPosition, amountToLoad);
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
        /// Returns Upcoming movies by page.
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/upcoming/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies(int currentPosition, int amountToLoad)
        {
            return await _movieEngineV2.UpcomingMovies(currentPosition, amountToLoad);
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
        /// Returns Popular Tv Shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/popular/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> PopularTv(int currentPosition, int amountToLoad)
        {
            return await _tvSearchEngine.Popular(currentPosition, amountToLoad);
        }

        /// <summary>
        /// Returns Popular Tv Shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/popular/{currentPosition}/{amountToLoad}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> PopularTvWithImages(int currentPosition, int amountToLoad)
        {
            return await _tvSearchEngine.Popular(currentPosition, amountToLoad, true);
        }

        /// <summary>
        /// Returns most Anticipated tv shows.
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
        /// Returns most Anticipated tv shows by page.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/anticipated/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> AnticipatedTv(int currentPosition, int amountToLoad)
        {
            return await _tvSearchEngine.Anticipated(currentPosition, amountToLoad);
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
            return await _tvSearchEngine.Popular();
        }

        /// <summary>
        /// Returns Most watched shows by page.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/mostwatched/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Obsolete("This method is obsolete, Trakt API no longer supports this")]
        public async Task<IEnumerable<SearchTvShowViewModel>> MostWatched(int currentPosition, int amountToLoad)
        {
            return await _tvSearchEngine.Popular(currentPosition, amountToLoad);
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

        /// <summary>
        /// Returns trending shows by page
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/trending/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchTvShowViewModel>> Trending(int currentPosition, int amountToLoad)
        {
            return await _tvSearchEngine.Trending(currentPosition, amountToLoad);
        }

        
        /// <summary>
        /// Returns all the movies that is by the actor id 
        /// </summary>
        /// <param name="actorId">TheMovieDb Actor ID</param>
        /// <returns></returns>
        [HttpGet("actor/{actorId}/movie")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActorCredits> GetMoviesByActor(int actorId)
        {
           return await _movieEngineV2.GetMoviesByActor(actorId, null);
        }


        [HttpGet("artist/{artistId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ArtistInformation> GetArtistInformation(string artistId)
        {
            return await _musicEngine.GetArtistInformation(artistId);
        }

        [HttpGet("releasegroupart/{musicBrainzId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<AlbumArt> GetReleaseGroupARt(string musicBrainzId)
        {
            return await _musicEngine.GetReleaseGroupArt(musicBrainzId, CancellationToken);
        }
    }
}