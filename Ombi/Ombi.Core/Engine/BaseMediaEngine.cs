using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Claims;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Requests.Models;
using Ombi.Helpers;

namespace Ombi.Core.Engine
{
    public abstract class BaseMediaEngine : BaseEngine
    {
        protected BaseMediaEngine(IPrincipal identity, IRequestServiceMain requestService) : base(identity)
        {
            RequestService = requestService;
        }

        protected IRequestServiceMain RequestService { get; }
        protected IRequestService<MovieRequestModel> MovieRequestService => RequestService.MovieRequestService;
        protected IRequestService<TvRequestModel> TvRequestService => RequestService.TvRequestService;

        private long _cacheTime = 0;
        private Dictionary<int, MovieRequestModel> _dbMovies;
        private Dictionary<int, TvRequestModel> _dbTv;
        protected async Task<Dictionary<int, MovieRequestModel>> GetMovieRequests()
        {
            long now = DateTime.Now.Ticks;
            if (_dbMovies == null || (now - _cacheTime) > 10000)
            {
                var allResults = await MovieRequestService.GetAllAsync();

                var distinctResults = allResults.DistinctBy(x => x.ProviderId);
                _dbMovies = distinctResults.ToDictionary(x => x.ProviderId);
                _cacheTime = now;
            }
            return _dbMovies;
        }

        protected async Task<Dictionary<int, TvRequestModel>> GetTvRequests()
        {
            long now = DateTime.Now.Ticks;
            if (_dbTv == null || (now - _cacheTime) > 10000)
            {
                var allResults = await TvRequestService.GetAllAsync();

                var distinctResults = allResults.DistinctBy(x => x.ProviderId);
                _dbTv = distinctResults.ToDictionary(x => x.ProviderId);
                _cacheTime = now;
            }
            return _dbTv;
        }
    }
}