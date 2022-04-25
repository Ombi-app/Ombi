using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;
using System.Collections.Generic;
using Ombi.Core;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Engine.V2;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Models.Search.V2.Music;
using Ombi.Models;
using Ombi.Api.RottenTomatoes.Models;
using Ombi.Api.RottenTomatoes;
using Ombi.Helpers;

// Due to conflicting Genre models in
// Ombi.TheMovieDbApi.Models and Ombi.Api.TheMovieDb.Models   
using Genre = Ombi.TheMovieDbApi.Models.Genre;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Controllers.V2
{
    public class SearchController : V2Controller
    {
        public SearchController(IMultiSearchEngine multiSearchEngine,
            IMovieEngineV2 v2Movie, ITVSearchEngineV2 v2Tv, IMusicSearchEngineV2 musicEngine, IRottenTomatoesApi rottenTomatoesApi,
            IMediaCacheService mediaCacheService)
        {
            _multiSearchEngine = multiSearchEngine;
            _movieEngineV2 = v2Movie;
            _movieEngineV2.ResultLimit = 20;
            _tvEngineV2 = v2Tv;
            _musicEngine = musicEngine;
            _rottenTomatoesApi = rottenTomatoesApi;
            _mediaCacheService = mediaCacheService;
        }

        private readonly IMultiSearchEngine _multiSearchEngine;
        private readonly IMovieEngineV2 _movieEngineV2;
        private readonly ITVSearchEngineV2 _tvEngineV2;
        private readonly IMusicSearchEngineV2 _musicEngine;
        private readonly IRottenTomatoesApi _rottenTomatoesApi;
        private readonly IMediaCacheService _mediaCacheService;

        /// <summary>
        /// Returns search results for both TV and Movies
        /// </summary>
        /// <remarks>The ID's returned by this are all TheMovieDbID's even for the TV Shows. You can call <see cref="GetTvInfoByMovieId"/> to get TV
        ///  Show information using the MovieDbId.</remarks>
        /// <param name="searchTerm">The search you want, this can be for a movie or TV show e.g. Star Wars will return
        ///  all Star Wars movies and Star Wars Rebels the TV Sho</param>
        /// <param name="filter">Filter for the search</param>
        /// <returns></returns>
        [HttpPost("multi/{searchTerm}")]
        public async Task<List<MultiSearchResult>> MultiSearch(string searchTerm, [FromBody] MultiSearchFilter filter)
        {
            return await _multiSearchEngine.MultiSearch(Uri.UnescapeDataString(searchTerm), filter, Request.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Gets the genres for either Tv or Movies depending on media type
        /// </summary>
        /// <param name="media">Either `tv` or `movie`.</param>
        [HttpGet("Genres/{media}")]
        public Task<IEnumerable<Genre>> GetGenres(string media)
        {
            return _multiSearchEngine.GetGenres(media, HttpContext.RequestAborted);
        }

        [HttpGet("Languages")]
        public Task<IEnumerable<Language>> GetLanguages()
        {
            return _multiSearchEngine.GetLanguages(HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns details for a single movie
        /// </summary>
        /// <param name="movieDbId">The MovieDB Id</param>
        [HttpGet("movie/{movieDbId}")]
        public Task<MovieFullInfoViewModel> GetMovieInfo(int movieDbId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetMovieInfo) + movieDbId,
            () => _movieEngineV2.GetFullMovieInformation(movieDbId, Request.HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        [HttpGet("movie/imdb/{imdbid}")]
        public Task<MovieFullInfoViewModel> GetMovieInfoByImdbId(string imdbId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetMovieInfoByImdbId) + imdbId,
            () => _movieEngineV2.GetMovieInfoByImdbId(imdbId, Request.HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns details for a single movie
        /// </summary>
        [HttpGet("movie/request/{requestId}")]
        public Task<MovieFullInfoViewModel> GetMovieByRequest(int requestId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetMovieByRequest) + requestId,
            () => _movieEngineV2.GetMovieInfoByRequestId(requestId, Request.HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns basic information about the provided collection
        /// </summary>
        /// <param name="collectionId">The collection id from TheMovieDb</param>
        /// <returns></returns>
        [HttpGet("movie/collection/{collectionId}")]
        public Task<MovieCollectionsViewModel> GetMovieCollections(int collectionId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetMovieCollections) + collectionId,
                () => _movieEngineV2.GetCollection(collectionId, Request.HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns details for a single show
        /// </summary>
        /// <remarks>TVMaze is the TV Show Provider</remarks>
        /// <param name="tvdbid">The TVDB Id</param>
        [HttpGet("tv/{tvdbId}")]
        public Task<SearchFullInfoTvShowViewModel> GetTvInfo(string tvdbid)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetTvInfo) + tvdbid,
                () => _tvEngineV2.GetShowInformation(tvdbid, HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns details for a single show
        /// </summary>
        /// <remarks>TVMaze is the TV Show Provider</remarks>
        /// 
        [HttpGet("tv/request/{requestId}")]
        public Task<SearchFullInfoTvShowViewModel> GetTvInfoByRequest(int requestId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetTvInfoByRequest) + requestId,
                () => _tvEngineV2.GetShowByRequest(requestId, HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns details for a single show
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv/moviedb/{moviedbid}")]
        public Task<SearchFullInfoTvShowViewModel> GetTvInfoByMovieId(string moviedbid)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetTvInfoByMovieId) + moviedbid,
                () => _tvEngineV2.GetShowInformation(moviedbid, HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
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
        public Task<IEnumerable<SearchMovieViewModel>> SimilarMovies([FromBody] SimilarMoviesRefineModel model)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(SimilarMovies) + model.TheMovieDbId + model.LanguageCode,
                () => _movieEngineV2.SimilarMovies(model.TheMovieDbId, model.LanguageCode),
                DateTimeOffset.Now.AddHours(12));
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
        public Task<IEnumerable<SearchMovieViewModel>> Popular(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(Popular) + "Movies" + currentPosition + amountToLoad,
            () => _movieEngineV2.PopularMovies(currentPosition, amountToLoad, Request.HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns Advanced Searched Media using paging
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpPost("advancedSearch/movie/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<SearchMovieViewModel>> AdvancedSearchMovie([FromBody]DiscoverModel model, int currentPosition, int amountToLoad)
        {
            return _movieEngineV2.AdvancedSearch(model, currentPosition, amountToLoad, Request.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns Seasonal Movies
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/seasonal/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<SearchMovieViewModel>> Seasonal(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(Seasonal) + "Movies" + currentPosition + amountToLoad,
             () => _movieEngineV2.SeasonalList(currentPosition, amountToLoad, Request.HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(1));
        }

        /// <summary>
        /// Returns Recently Requested Movies using Paging
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("movie/requested/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchMovieViewModel>> RecentlyRequestedMovies(int currentPosition, int amountToLoad)
        {
            return await _movieEngineV2.RecentlyRequestedMovies(currentPosition, amountToLoad, Request.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns Recently Requested Tv using Paging
        /// </summary>
        /// <remarks>We use TheMovieDb as the Movie Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/requested/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<SearchFullInfoTvShowViewModel>> RecentlyRequestedTv(int currentPosition, int amountToLoad)
        {
            return await _tvEngineV2.RecentlyRequestedShows(currentPosition, amountToLoad, Request.HttpContext.RequestAborted);
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
        public Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(NowPlayingMovies) + currentPosition + amountToLoad,
            () => _movieEngineV2.NowPlayingMovies(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
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
        public Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(TopRatedMovies) + currentPosition + amountToLoad,
                () => _movieEngineV2.TopRatedMovies(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
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
        public Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(UpcomingMovies) + currentPosition + amountToLoad,
                () => _movieEngineV2.UpcomingMovies(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns Popular Tv Shows
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/popular/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<SearchTvShowViewModel>> PopularTv(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(PopularTv) + currentPosition + amountToLoad,
                () => _tvEngineV2.Popular(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
        }

        /// <summary>
        /// Returns most Anticipated tv shows by page.
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/anticipated/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<SearchTvShowViewModel>> AnticipatedTv(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(AnticipatedTv) + currentPosition + amountToLoad,
                () => _tvEngineV2.Anticipated(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
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
        public Task<IEnumerable<SearchTvShowViewModel>> MostWatched(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(MostWatched) + currentPosition + amountToLoad,
            () => _tvEngineV2.Popular(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
        }


        /// <summary>
        /// Returns trending shows by page
        /// </summary>
        /// <remarks>We use Trakt.tv as the Provider</remarks>
        /// <returns></returns>
        [HttpGet("tv/trending/{currentPosition}/{amountToLoad}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<SearchTvShowViewModel>> Trending(int currentPosition, int amountToLoad)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(Trending) + currentPosition + amountToLoad,
                () => _tvEngineV2.Trending(currentPosition, amountToLoad),
                DateTimeOffset.Now.AddHours(12));
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
        
        /// <summary>
        /// Returns all the tv shows that is by the actor id 
        /// </summary>
        /// <param name="actorId">TheMovieDb Actor ID</param>
        /// <returns></returns>
        [HttpGet("actor/{actorId}/tv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActorCredits> GetTvByActor(int actorId)
        {
            return await _tvEngineV2.GetTvByActor(actorId, null);
        }


        [HttpGet("artist/{artistId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ArtistInformation> GetArtistInformation(string artistId)
        {
            return await _musicEngine.GetArtistInformation(artistId);
        }

        [HttpGet("artist/request/{requestId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ArtistInformation> GetArtistInformationByRequestId(int requestId)
        {
            return await _musicEngine.GetArtistInformationByRequestId(requestId);
        }

        [HttpGet("artist/album/{albumId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<ReleaseGroup> GetAlbumInformation(string albumId)
        {
            return _musicEngine.GetAlbum(albumId);
        }

        [HttpGet("releasegroupart/{musicBrainzId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<AlbumArt> GetReleaseGroupARt(string musicBrainzId)
        {
            return await _musicEngine.GetReleaseGroupArt(musicBrainzId, CancellationToken);
        }

        [HttpGet("ratings/movie/{name}/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<MovieRatings> GetRottenMovieRatings(string name, int year)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetRottenMovieRatings) + name + year,
                () => _rottenTomatoesApi.GetMovieRatings(name, year),
                DateTimeOffset.Now.AddHours(12));
        }

        [HttpGet("ratings/tv/{name}/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<TvRatings> GetRottenTvRatings(string name, int year)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetRottenTvRatings) + name + year,
            () => _rottenTomatoesApi.GetTvRatings(name, year),
                DateTimeOffset.Now.AddHours(12));
        }

        [HttpGet("stream/movie/{movieDbId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<StreamingData>> GetMovieStreams(int movieDBId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetMovieStreams) + movieDBId,
                () => _movieEngineV2.GetStreamInformation(movieDBId, HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }

        [HttpGet("stream/tv/{movieDbId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public Task<IEnumerable<StreamingData>> GetTvStreams(int movieDbId)
        {
            return _mediaCacheService.GetOrAddAsync(nameof(GetTvStreams) + movieDbId,
                () => _tvEngineV2.GetStreamInformation(movieDbId, HttpContext.RequestAborted),
                DateTimeOffset.Now.AddHours(12));
        }
    }
}