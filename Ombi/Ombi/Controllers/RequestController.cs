using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;

namespace Ombi.Controllers
{
    [Authorize]
    public class RequestController : BaseV1ApiController
    {
        public RequestController(IRequestEngine engine)
        {
            RequestEngine = engine;
        }

        private IRequestEngine RequestEngine { get; }
        

        [HttpGet("movie/{count:int}/{position:int}", Name = "GetRequestsByCount")]
        public async Task<IEnumerable<MovieRequestModel>> GetRequests(int count, int position)
        {
            return await RequestEngine.GetMovieRequests(count, position);
        }

        [HttpPost("movie")]
        public async Task<RequestEngineResult> RequestMovie([FromBody]SearchMovieViewModel movie)
        {
            return await RequestEngine.RequestMovie(movie);
        }

        //[HttpPost("tv")]
        //public async Task<RequestEngineResult> RequestTv([FromBody]SearchTvShowViewModel tv)
        //{
        //    return await RequestEngine.RequestMovie();
        //}

        [HttpGet("movie/search/{searchTerm}")]
        public async Task<IEnumerable<MovieRequestModel>> Search(string searchTerm)
        {
            
            return await RequestEngine.SearchMovieRequest(searchTerm);
        }

        [HttpDelete("movie/{requestId:int}")]
        public async Task DeleteRequest(int requestId)
        {
            await RequestEngine.RemoveMovieRequest(requestId);
        }

        [HttpPut("movie")]
        public async Task<MovieRequestModel> UpdateRequest([FromBody]MovieRequestModel model)
        {
            return await RequestEngine.UpdateMovieRequest(model);
        }
    }
}
