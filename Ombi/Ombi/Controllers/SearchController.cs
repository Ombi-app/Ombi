using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Models.Search;

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

        [HttpGet("movie/{searchTerm}")]
        public async Task<IEnumerable<SearchMovieViewModel>> SearchMovie(string searchTerm)
        {
            Logger.LogDebug("Searching : {searchTerm}", searchTerm);
            return await MovieEngine.Search(searchTerm);
        }

        [HttpPost("movie/extrainfo")]
        public async Task<IEnumerable<SearchMovieViewModel>> GetImdbInfo([FromBody]IEnumerable<SearchMovieViewModel> model)
        {
            return await MovieEngine.LookupImdbInformation(model);
        }

        [HttpGet("movie/popular")]
        public async Task<IEnumerable<SearchMovieViewModel>> Popular()
        {
            return await MovieEngine.PopularMovies();
        }
        [HttpGet("movie/nowplaying")]
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            return await MovieEngine.NowPlayingMovies();
        }
        [HttpGet("movie/toprated")]
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            return await MovieEngine.TopRatedMovies();
        }
        [HttpGet("movie/upcoming")]
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            return await MovieEngine.UpcomingMovies();
        }

        [HttpGet("tv/{searchTerm}")]
        public async Task<IEnumerable<SearchTvShowViewModel>> SearchTv(string searchTerm)
        {
            return await TvEngine.Search(searchTerm);
        }

        //[HttpGet("tv/popular")]
        //public async Task<IEnumerable<SearchTvShowViewModel>> PopularTv()
        //{
        //    return await TvEngine.Popular();
        //}
        //[HttpGet("tv/anticiplated")]
        //public async Task<IEnumerable<SearchTvShowViewModel>> AnticiplatedTv()
        //{
        //    return await TvEngine.Anticipated();
        //}
        //[HttpGet("tv/mostwatched")]
        //public async Task<IEnumerable<SearchTvShowViewModel>> MostWatched()
        //{
        //    return await TvEngine.MostWatches();
        //}
        //[HttpGet("tv/trending")]
        //public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        //{
        //    return await TvEngine.Trending();
        //}
    }
}
