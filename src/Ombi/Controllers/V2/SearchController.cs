using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Engine.V2;
using System.Collections.Generic;

namespace Ombi.Controllers.V2
{
    [ApiV2]
    [Authorize]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public SearchController(IMultiSearchEngine multiSearchEngine)
        {
            _multiSearchEngine = multiSearchEngine;
        }

        private readonly IMultiSearchEngine _multiSearchEngine;

        /// <summary>
        /// Runs the update job
        /// </summary>
        /// <returns></returns>
        [HttpGet("multi/{searchTerm}")]
        public async Task<List<MultiSearch>> ForceUpdate(string searchTerm)
        {
            return await _multiSearchEngine.MultiSearch(searchTerm);
        }
    }
}