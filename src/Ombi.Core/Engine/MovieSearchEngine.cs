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
using Ombi.Core.Rule.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Core.Authentication;
using Ombi.Helpers;

namespace Ombi.Core.Engine
{
    public class MovieSearchEngine : BaseMediaEngine, IMovieEngine
    {
        public MovieSearchEngine(IPrincipal identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper,
            ILogger<MovieSearchEngine> logger, IRuleEvaluator r, OmbiUserManager um, IMemoryCache mem)
            : base(identity, service, r, um)
        {
            MovieApi = movApi;
            Mapper = mapper;
            Logger = logger;
            MemCache = mem;
        }

        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }
        private ILogger<MovieSearchEngine> Logger { get; }
        private IMemoryCache MemCache { get; }

        /// <summary>
        /// Lookups the imdb information.
        /// </summary>
        /// <param name="theMovieDbId">The movie database identifier.</param>
        /// <returns></returns>
        public async Task<SearchMovieViewModel> LookupImdbInformation(int theMovieDbId)
        {
            var movieInfo = await MovieApi.GetMovieInformationWithVideo(theMovieDbId);
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movieInfo);

            return await ProcessSingleMovie(viewMovie, true);
        }

        /// <summary>
        /// Searches the specified movie.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> Search(string search)
        {
            var result = await MovieApi.SearchMovie(search);

            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(10)); // Take 10 to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets popular movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {
            var result = await MemCache.GetOrCreateAsync(CacheKeys.PopularMovies, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(12);
                return await MovieApi.PopularMovies();
            });
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(10)); // Take 10 to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets top rated movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var result = await MemCache.GetOrCreateAsync(CacheKeys.TopRatedMovies, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(12);
                return await MovieApi.TopRated();
            });
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(10)); // Take 10 to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets upcoming movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var result = await MemCache.GetOrCreateAsync(CacheKeys.UpcomingMovies, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(12);
                return await MovieApi.Upcoming();
            });
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(10)); // Take 10 to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets now playing movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var result = await MemCache.GetOrCreateAsync(CacheKeys.NowPlayingMovies, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(12);
                return await MovieApi.NowPlaying();
            });
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Take(10)); // Take 10 to stop us overloading the API
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
            }

            // So when we run the rule to check if it's available in Plex we need the ImdbId
            // But we only pass down the SearchViewModel that doesn't contain this
            // So set the ImdbId to viewMovie.Id and then set it back afterwards
            var oldId = viewMovie.Id;
            viewMovie.CustomId = viewMovie.ImdbId ?? string.Empty;

            await RunSearchRules(viewMovie);

            viewMovie.Id = oldId;
            return viewMovie;
        }


        private async Task<SearchMovieViewModel> ProcessSingleMovie(MovieSearchResult movie)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            return await ProcessSingleMovie(viewMovie);
        }
    }
}