using System;
using System.Collections.Generic;

using RequestPlex.Api.Interfaces;
using RequestPlex.Api.Models;

using RestSharp;

using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace RequestPlex.Api
{
    public class TheMovieDbApi : MovieBase
    {

        public List<SearchMovie> SearchMovie(string searchTerm)
        {
            var client = new TMDbClient(ApiKey);
            var results = client.SearchMovie(searchTerm);
            return results.Results;
        }

        public List<SearchTv> SearchTv(string searchTerm)
        {
            var client = new TMDbClient(ApiKey);
            var results = client.SearchTvShow(searchTerm);
            return results.Results;
        }
    }
}
