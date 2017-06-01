using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;

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
            return await MovieRequestEngine.GetMovieRequests(count, position);
        }

        [HttpPost("movie")]
        public async Task<RequestEngineResult> RequestMovie([FromBody]SearchMovieViewModel movie)
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
        public async Task<MovieRequestModel> UpdateRequest([FromBody]MovieRequestModel model)
        {
            return await MovieRequestEngine.UpdateMovieRequest(model);
        }

        [HttpGet("tv/{count:int}/{position:int}")]
        public async Task<IEnumerable<TvRequestModel>> GetTvRequests(int count, int position)
        {
            try
            {
                return await TvRequestEngine.GetTvRequests(count, position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost("tv")]
        public async Task<RequestEngineResult> RequestTv([FromBody]SearchTvShowViewModel tv)
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
        public async Task<TvRequestModel> UpdateRequest([FromBody]TvRequestModel model)
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
    }
}
