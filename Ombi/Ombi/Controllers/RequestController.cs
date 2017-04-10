using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;

namespace Ombi.Controllers
{
    public class RequestController : BaseV1ApiController
    {
        public RequestController(IRequestEngine engine)
        {
            RequestEngine = engine;
        }

        private IRequestEngine RequestEngine { get; }

        [HttpGet]
        public async Task<IEnumerable<RequestViewModel>> GetRequests()
        {
            return await RequestEngine.GetRequests();
        }

        [HttpGet("{count:int}/{position:int}", Name = "GetRequestsByCount")]
        public async Task<IEnumerable<RequestViewModel>> GetRequests(int count, int position)
        {
            return await RequestEngine.GetRequests(count, position);
        }

        [HttpPost("movie")]
        public async Task<RequestEngineResult> RequestMovie([FromBody]SearchMovieViewModel movie)
        {
            return await RequestEngine.RequestMovie(movie);
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<RequestViewModel>> Search(string searchTerm)
        {
            return await RequestEngine.SearchRequest(searchTerm);
        }

        [HttpDelete("{requestId:int}")]
        public async Task DeleteRequest(int requestId)
        {
            await RequestEngine.RemoveRequest(requestId);
        }

        [HttpPost]
        public async Task<RequestViewModel> UpdateRequest([FromBody]RequestViewModel model)
        {
            return await RequestEngine.UpdateRequest(model);
        }
    }
}
