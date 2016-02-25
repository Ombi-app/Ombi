using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TMDbLib.Client;
using TMDbLib.Objects.Search;

namespace RequestPlex.Api
{
    public class TheMovieDbApi : MovieBase
    {

        public async Task<List<SearchMovie>> SearchMovie(string searchTerm)
        {
            var client = new TMDbClient(ApiKey);
            var results = await client.SearchMovie(searchTerm);
            return results.Results;
        }

        public async Task<List<SearchTv>> SearchTv(string searchTerm)
        {
            var client = new TMDbClient(ApiKey);
            var results = await client.SearchTvShow(searchTerm);
            return results.Results;
        }
    }
}
