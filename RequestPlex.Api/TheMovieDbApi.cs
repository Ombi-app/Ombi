using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace RequestPlex.Api
{
    public class TheMovieDbApi : MovieBase
    {
        public TheMovieDbApi()
        {
            Client = new TMDbClient(ApiKey);
        }

        public TMDbClient Client { get; set; }
        public async Task<List<SearchMovie>> SearchMovie(string searchTerm)
        {
            var results = await Client.SearchMovie(searchTerm);
            return results.Results;
        }

        [Obsolete("Should use TheTvDbApi for TV")]
        public async Task<List<SearchTv>> SearchTv(string searchTerm)
        {
            var results = await Client.SearchTvShow(searchTerm);
            return results.Results;
        }

        public async Task<List<MovieResult>> GetCurrentPlayingMovies()
        {
            var movies = await Client.GetMovieList(MovieListType.NowPlaying);
            return movies.Results;
        }
        public async Task<List<MovieResult>> GetUpcomingMovies()
        {
            var movies = await Client.GetMovieList(MovieListType.Upcoming);
            return movies.Results;
        }

        public async Task<Movie> GetMovieInformation(int tmdbId)
        {
            var movies = await Client.GetMovie(tmdbId);
            return movies;
        }

        [Obsolete("Should use TheTvDbApi for TV")]
        public async Task<TvShow> GetTvShowInformation(int tmdbId)
        {
            var show = await Client.GetTvShow(tmdbId);
            return show;
        }
    }
}
