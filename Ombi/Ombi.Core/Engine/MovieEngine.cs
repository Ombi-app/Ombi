using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.TheMovieDbApi;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Core.Engine
{
    public class MovieEngine : IMovieEngine
    {

        public MovieEngine(IRequestService service, IMovieDbApi movApi, IMapper mapper)
        {
            RequestService = service;
            MovieApi = movApi;
            Mapper = mapper;
        }
        private IRequestService RequestService { get; }
        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }

        public async Task<IEnumerable<SearchMovieViewModel>> LookupImdbInformation(IEnumerable<SearchMovieViewModel> movies)
        {
            var retVal = new List<SearchMovieViewModel>();
            Dictionary<int, RequestModel> dbMovies = await RequestedMovies();
            foreach (var m in movies)
            {

                var movieInfo = await MovieApi.GetMovieInformationWithVideo(m.Id);
                var viewMovie = Mapper.Map<SearchMovieViewModel>(movieInfo);
               
                retVal.Add(viewMovie);
                // TODO needs to be careful about this, it's adding extra time to search...
                // https://www.themoviedb.org/talk/5807f4cdc3a36812160041f2
                //var videoId = movieInfo?.video ?? false
                //    ? movieInfo?.videos?.results?.FirstOrDefault()?.key
                //    : string.Empty;

                //viewMovie.Trailer = string.IsNullOrEmpty(videoId)
                //    ? string.Empty
                //    : $"https://www.youtube.com/watch?v={videoId}";
                if (dbMovies.ContainsKey(movieInfo.Id) /*&& canSee*/) // compare to the requests db
                {
                    var dbm = dbMovies[movieInfo.Id];

                    viewMovie.Requested = true;
                    viewMovie.Approved = dbm.Approved;
                    viewMovie.Available = dbm.Available;
                }
            }
            return retVal;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> ProcessMovieSearch(string search)
        {
            var result = await MovieApi.SearchMovie(search);
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {
            var result = await MovieApi.PopularMovies();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var result = await MovieApi.TopRated();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var result = await MovieApi.Upcoming();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var result = await MovieApi.NowPlaying();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }


        private async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse(IEnumerable<MovieSearchResult> movies)
        {
            await Task.Yield();
            var viewMovies = new List<SearchMovieViewModel>();
            //var counter = 0;
            Dictionary<int, RequestModel> dbMovies = await RequestedMovies();
            foreach (var movie in movies)
            {
                var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
              
                viewMovies.Add(viewMovie);



                //    var canSee = CanUserSeeThisRequest(viewMovie.Id, Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests), dbMovies);

                //    var plexSettings = await PlexService.GetSettingsAsync();
                //    var embySettings = await EmbySettings.GetSettingsAsync();
                //    if (plexSettings.Enable)
                //    {
                //        var content = PlexContentRepository.GetAll();
                //        var plexMovies = PlexChecker.GetPlexMovies(content);

                //        var plexMovie = PlexChecker.GetMovie(plexMovies.ToArray(), movie.Title,
                //            movie.ReleaseDate?.Year.ToString(),
                //            viewMovie.ImdbId);
                //        if (plexMovie != null)
                //        {
                //            viewMovie.Available = true;
                //            viewMovie.PlexUrl = plexMovie.Url;
                //        }
                //    }
                //    if (embySettings.Enable)
                //    {
                //        var embyContent = EmbyContentRepository.GetAll();
                //        var embyMovies = EmbyChecker.GetEmbyMovies(embyContent);

                //        var embyMovie = EmbyChecker.GetMovie(embyMovies.ToArray(), movie.Title,
                //            movie.ReleaseDate?.Year.ToString(), viewMovie.ImdbId);
                //        if (embyMovie != null)
                //        {
                //            viewMovie.Available = true;
                //        }
                //    }
                if (dbMovies.ContainsKey(movie.Id) /*&& canSee*/) // compare to the requests db
                {
                    var dbm = dbMovies[movie.Id];

                    viewMovie.Requested = true;
                    viewMovie.Approved = dbm.Approved;
                    viewMovie.Available = dbm.Available;
                }
                //    else if (canSee)
                //    {
                //        bool exists = IsMovieInCache(movie, viewMovie.ImdbId);
                //        viewMovie.Approved = exists;
                //        viewMovie.Requested = exists;
                //    }
                //    viewMovies.Add(viewMovie);
                //}


            }
            return viewMovies;
        }


        private long _dbMovieCacheTime = 0;
        private Dictionary<int, RequestModel> _dbMovies;
        private async Task<Dictionary<int, RequestModel>> RequestedMovies()
        {
            long now = DateTime.Now.Ticks;
            if (_dbMovies == null || (now - _dbMovieCacheTime) > 10000)
            {
                var allResults = await RequestService.GetAllAsync();
                allResults = allResults.Where(x => x.Type == RequestType.Movie);

                var distinctResults = allResults.DistinctBy(x => x.ProviderId);
                _dbMovies = distinctResults.ToDictionary(x => x.ProviderId);
                _dbMovieCacheTime = now;
            }
            return _dbMovies;
        }
    }
}
