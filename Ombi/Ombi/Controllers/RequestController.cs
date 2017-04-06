using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Models.Search;

namespace Ombi.Controllers
{
    public class RequestController : BaseApiController
    {
        public RequestController(IRequestEngine engine)
        {
            RequestEngine = engine;
        }

        private IRequestEngine RequestEngine { get; }

        [HttpPost("movie")]
        public async Task<RequestEngineResult> SearchMovie([FromBody]SearchMovieViewModel movie)
        {
            return await RequestEngine.RequestMovie(movie);
        }
    }
}
