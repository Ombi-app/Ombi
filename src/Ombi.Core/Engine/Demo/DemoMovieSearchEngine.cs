using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Config;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine.Demo
{
    public class DemoMovieSearchEngine : MovieSearchEngine, IDemoMovieSearchEngine
    {
        public DemoMovieSearchEngine(IPrincipal identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper, 
            ILogger<MovieSearchEngine> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, 
            IRepository<RequestSubscription> sub, IOptions<DemoLists> lists)
            : base(identity, service, movApi, mapper, logger, r, um, mem, s, sub)
        {
            _demoLists = lists.Value;
        }

        private readonly DemoLists _demoLists;

        public async Task<IEnumerable<SearchMovieViewModel>> Search(string search)
        {
            var result = await MovieApi.SearchMovie(search, null, "en");

            for (var i = 0; i < result.Count; i++)
            {
                if (!_demoLists.Movies.Contains(result[i].Id))
                {
                    result.RemoveAt(i);
                }  
            }
            if(result.Count > 0)
                return await TransformMovieResultsToResponse(result.Take(MovieLimit)); // Take x to stop us overloading the API
            return null;
        }

        new public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var rand = new Random();
            var responses = new List<SearchMovieViewModel>();
            for (int i = 0; i < 10; i++)
            {
                var item = rand.Next(_demoLists.Movies.Length);
                var movie = _demoLists.Movies[item];
                if (responses.Any(x => x.Id == movie))
                {
                    i--;
                    continue;
                }
                var movieResult = await MovieApi.GetMovieInformationWithExtraInfo(movie);
                var viewMovie = Mapper.Map<SearchMovieViewModel>(movieResult);

               responses.Add(await ProcessSingleMovie(viewMovie));
            }

            return responses;
        }

        new public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {
            return await NowPlayingMovies();
        }


        new public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            return await NowPlayingMovies();
        }

        new public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {

            return await NowPlayingMovies();
        }
    }

    public interface IDemoMovieSearchEngine
    {
        Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies();

        Task<IEnumerable<SearchMovieViewModel>> PopularMovies();

        Task<IEnumerable<SearchMovieViewModel>> Search(string search);

        Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies();

        Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies();

    }
}
