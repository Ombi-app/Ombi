using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.TheMovieDbApi
{
    public class TheMovieDbApi
    {
        public TheMovieDbApi()
        {
            Api = new Api.Api();
        }
        private const string ApiToken = "b8eabaf5608b88d0298aa189dd90bf00";
        private static readonly Uri BaseUri = new Uri("https://api.themoviedb.org/3/");
        public Api.Api Api { get; }

        public async Task<MovieResponse> GetMovieInformation(int movieId)
        {
            var url = BaseUri.ChangePath("movie/{0}", movieId.ToString());
            AddHeaders(url);
            return await Api.Get<MovieResponse>(url);
        }

        public async Task<TheMovieDbContainer<SearchResult>> SearchMovie(string searchTerm)
        {
            var url = BaseUri.ChangePath("search/movie/");
            url = AddHeaders(url);
            url = url.AddQueryParameter("query", searchTerm);
            return await Api.Get<TheMovieDbContainer<SearchResult>>(url);
        }

        private Uri AddHeaders(Uri url)
        {
            return url.AddQueryParameter("api_key", ApiToken);
        }
    }
}
