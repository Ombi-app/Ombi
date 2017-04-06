using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.TheMovieDbApi
{
    public class TheMovieDbApi : IMovieDbApi
    {
        public TheMovieDbApi()
        {
            Api = new Api.Api();
        }
        private const string ApiToken = "b8eabaf5608b88d0298aa189dd90bf00";
        private static readonly Uri BaseUri = new Uri("http://api.themoviedb.org/3/");
        public Api.Api Api { get; }

        public async Task<MovieResponse> GetMovieInformation(int movieId)
        {
            var url = BaseUri.ChangePath("movie/{0}", movieId.ToString());
            url = AddHeaders(url);
            return await Api.Get<MovieResponse>(url);
        }

        public async Task<TheMovieDbContainer<SearchResult>> SearchMovie(string searchTerm)
        {
            var url = BaseUri.ChangePath("search/movie/");
            url = AddHeaders(url);
            url = url.AddQueryParameter("query", searchTerm);
            return await Api.Get<TheMovieDbContainer<SearchResult>>(url);
        }

        public async Task<TheMovieDbContainer<SearchResult>> PopularMovies()
        {
            var url = BaseUri.ChangePath("movie/popular");
            url = AddHeaders(url);
            return await Api.Get<TheMovieDbContainer<SearchResult>>(url);
        }

        public async Task<TheMovieDbContainer<SearchResult>> TopRated()
        {
            var url = BaseUri.ChangePath("movie/top_rated");
            url = AddHeaders(url);
            return await Api.Get<TheMovieDbContainer<SearchResult>>(url);
        }

        public async Task<TheMovieDbContainer<SearchResult>> Upcoming()
        {
            var url = BaseUri.ChangePath("movie/upcoming");
            url = AddHeaders(url);
            return await Api.Get<TheMovieDbContainer<SearchResult>>(url);
        }

        public async Task<TheMovieDbContainer<SearchResult>> NowPlaying()
        {
            var url = BaseUri.ChangePath("movie/now_playing");
            url = AddHeaders(url);
            return await Api.Get<TheMovieDbContainer<SearchResult>>(url);
        }

        private Uri AddHeaders(Uri url)
        {
            return url.AddQueryParameter("api_key", ApiToken);
        }
    }
}
