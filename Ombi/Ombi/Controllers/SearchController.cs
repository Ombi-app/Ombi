using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core;
using Ombi.Core.Models.Search;

namespace Ombi.Controllers
{
    public class SearchController : BaseV1ApiController
    {
        public SearchController(IMovieEngine movie)
        {
            MovieEngine = movie;
        }

        private IMovieEngine MovieEngine { get; }

        [HttpGet("movie/{searchTerm}")]
        public async Task<IEnumerable<SearchMovieViewModel>> SearchMovie(string searchTerm)
        {
            return await MovieEngine.ProcessMovieSearch(searchTerm);
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



    }
}
