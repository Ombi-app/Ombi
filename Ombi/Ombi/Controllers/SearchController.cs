using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Controllers
{
    public class SearchController : BaseApiController
    {
        [HttpGet("movie/{searchTerm}")]
        public async Task<List<SearchResult>> SearchMovie(string searchTerm)
        {
            var api = new TheMovieDbApi.TheMovieDbApi();
            var result = await api.SearchMovie(searchTerm);
            return result.results;
        }

       
    }
}
