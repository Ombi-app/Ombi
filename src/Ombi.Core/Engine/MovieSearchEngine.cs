using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine
{
    public class MovieSearchEngine : BaseMediaEngine, IMovieEngine
    {
        public MovieSearchEngine(IPrincipal identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper,
            ILogger<MovieSearchEngine> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub)
            : base(identity, service, r, um, mem, s, sub)
        {
            MovieApi = movApi;
            Mapper = mapper;
            Logger = logger;
        }

        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }
        private ILogger<MovieSearchEngine> Logger { get; }

        private const int MovieLimit = 10;

        /// <summary>
        /// Lookups the imdb information.
        /// </summary>
        /// <param name="theMovieDbId">The movie database identifier.</param>
        /// <returns></returns>
        public async Task<SearchMovieViewModel> LookupImdbInformation(int theMovieDbId, string langCode = "en")
        {
            var movieInfo = await MovieApi.GetMovieInformationWithExtraInfo(theMovieDbId, langCode);
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movieInfo);

            return await ProcessSingleMovie(viewMovie, true);
        }

        /// <summary>
        /// Searches the specified movie.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> Search(string search, int? year, string langaugeCode)
        {
            var result = await MovieApi.SearchMovie(search, year, langaugeCode);

            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Get similar movies to the id passed in
        /// </summary>
        /// <param name="theMovieDbId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> SimilarMovies(int theMovieDbId)
        {
            var result = await MovieApi.SimilarMovies(theMovieDbId);
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets popular movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {
            var result = await Cache.GetOrAdd(CacheKeys.PopularMovies, async () => await MovieApi.PopularMovies(), DateTime.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets top rated movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var result = await Cache.GetOrAdd(CacheKeys.TopRatedMovies, async () => await MovieApi.TopRated(), DateTime.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets upcoming movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var result = await Cache.GetOrAdd(CacheKeys.UpcomingMovies, async () => await MovieApi.Upcoming(), DateTime.Now.AddHours(12));
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets now playing movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var result = await Cache.GetOrAdd(CacheKeys.NowPlayingMovies, async () => await MovieApi.NowPlaying(), DateTime.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        private async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse(
            IEnumerable<MovieSearchResult> movies)
        {
            var viewMovies = new List<SearchMovieViewModel>();
            foreach (var movie in movies)
            {
                viewMovies.Add(await ProcessSingleMovie(movie));
            }
            return viewMovies;
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie(SearchMovieViewModel viewMovie, bool lookupExtraInfo = false)
        {
            if (lookupExtraInfo)
            {
                var showInfo = await MovieApi.GetMovieInformation(viewMovie.Id);
                viewMovie.Id = showInfo.Id; // TheMovieDbId
                viewMovie.ImdbId = showInfo.ImdbId;
                var usDates = viewMovie.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == "US");
                viewMovie.DigitalReleaseDate = usDates?.ReleaseDate?.FirstOrDefault(x => x.Type == ReleaseDateType.Digital)?.ReleaseDate;
            }

            viewMovie.TheMovieDbId = viewMovie.Id.ToString();

            await RunSearchRules(viewMovie);

            // This requires the rules to be run first to populate the RequestId property
            await CheckForSubscription(viewMovie);

            return viewMovie;
        }

        private async Task CheckForSubscription(SearchMovieViewModel viewModel)
        {
            // Check if this user requested it
            var user = await GetUser();
            var request = await RequestService.MovieRequestService.GetAll()
                .AnyAsync(x => x.RequestedUserId.Equals(user.Id) && x.TheMovieDbId == viewModel.Id);
            if (request)
            {
                viewModel.ShowSubscribe = false;
            }
            else
            {
                viewModel.ShowSubscribe = true;
                var sub = await _subscriptionRepository.GetAll().FirstOrDefaultAsync(s => s.UserId == user.Id
                                                                                          && s.RequestId == viewModel.RequestId && s.RequestType == RequestType.Movie);
                viewModel.Subscribed = sub != null;
            }
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie(MovieSearchResult movie)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            return await ProcessSingleMovie(viewMovie);
        }
    }
}