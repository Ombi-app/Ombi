using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Models;

namespace Ombi.Controllers
{
    [Authorize]
    public class RequestController : BaseV1ApiController
    {
        public RequestController(IMovieRequestEngine engine, ITvRequestEngine tvRequestEngine)
        {
            MovieRequestEngine = engine;
            TvRequestEngine = tvRequestEngine;
        }

        private IMovieRequestEngine MovieRequestEngine { get; }
        private ITvRequestEngine TvRequestEngine { get; }

        [HttpGet("movie/{count:int}/{position:int}")]
        public async Task<IEnumerable<MovieRequestModel>> GetRequests(int count, int position)
        {
            return await MovieRequestEngine.GetRequests(count, position);
        }

        [HttpGet("movie")]
        public async Task<IEnumerable<MovieRequestModel>> GetRequests()
        {
            return await MovieRequestEngine.GetRequests();
        }

        [HttpPost("movie")]
        public async Task<RequestEngineResult> RequestMovie([FromBody] SearchMovieViewModel movie)
        {
            return await MovieRequestEngine.RequestMovie(movie);
        }

        [HttpGet("movie/search/{searchTerm}")]
        public async Task<IEnumerable<MovieRequestModel>> Search(string searchTerm)
        {
            return await MovieRequestEngine.SearchMovieRequest(searchTerm);
        }

        [HttpDelete("movie/{requestId:int}")]
        public async Task DeleteRequest(int requestId)
        {
            await MovieRequestEngine.RemoveMovieRequest(requestId);
        }

        [HttpPut("movie")]
        public async Task<MovieRequestModel> UpdateRequest([FromBody] MovieRequestModel model)
        {
            return await MovieRequestEngine.UpdateMovieRequest(model);
        }

        [HttpGet("tv/{count:int}/{position:int}")]
        public async Task<IEnumerable<TvRequestModel>> GetTvRequests(int count, int position)
        {
            return await TvRequestEngine.GetRequests(count, position);
        }

        [HttpGet("tv")]
        public async Task<IEnumerable<TvRequestModel>> GetTvRequests()
        {
            return await TvRequestEngine.GetRequests();
        }

        [HttpPost("tv")]
        public async Task<RequestEngineResult> RequestTv([FromBody] SearchTvShowViewModel tv)
        {
            return await TvRequestEngine.RequestTvShow(tv);
        }

        [HttpGet("tv/search/{searchTerm}")]
        public async Task<IEnumerable<TvRequestModel>> SearchTv(string searchTerm)
        {
            return await TvRequestEngine.SearchTvRequest(searchTerm);
        }

        [HttpDelete("tv/{requestId:int}")]
        public async Task DeleteTvRequest(int requestId)
        {
            await TvRequestEngine.RemoveTvRequest(requestId);
        }

        [HttpPut("tv")]
        public async Task<TvRequestModel> UpdateRequest([FromBody] TvRequestModel model)
        {
            return await TvRequestEngine.UpdateTvRequest(model);
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public RequestCountModel GetCountOfRequests()
        {
            // Doesn't matter if we use the TvEngine or MovieEngine, this method is in the base class
            return TvRequestEngine.RequestCount();
        }

        [HttpGet("tv/grid")]
        public async Task<RequestGridModel<TvRequestModel>> GetTvRequestsGrid()
        {
            return await GetGrid(TvRequestEngine);
        }

        [HttpGet("movie/grid")]
        public async Task<RequestGridModel<MovieRequestModel>> GetMovieRequestsGrid()
        {
            return await GetGrid(MovieRequestEngine);
        }

        private async Task<RequestGridModel<T>> GetGrid<T>(IRequestEngine<T> engine) where T : BaseRequestModel
        {
            var allRequests = await engine.GetRequests();
            var r = allRequests.ToList();
            var model = new RequestGridModel<T>
            {
                Available = r.Where(x => x.Available && !x.Approved),
                Approved = r.Where(x => x.Approved && !x.Available),
                New = r.Where(x => !x.Available && !x.Approved)
            };
            return model;
        }
    }
}