using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Requests.Models;
using Ombi.Core.Rules;
using Ombi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Engine
{
    public abstract class BaseMediaEngine : BaseEngine
    {
        private long _cacheTime;
        private Dictionary<int, MovieRequestModel> _dbMovies;
        private Dictionary<int, TvRequestModel> _dbTv;

        protected BaseMediaEngine(IPrincipal identity, IRequestServiceMain requestService,
            IRuleEvaluator rules) : base(identity, rules)
        {
            RequestService = requestService;
        }

        protected IRequestServiceMain RequestService { get; }
        protected IRequestService<MovieRequestModel> MovieRequestService => RequestService.MovieRequestService;
        protected IRequestService<TvRequestModel> TvRequestService => RequestService.TvRequestService;

        protected async Task<Dictionary<int, MovieRequestModel>> GetMovieRequests()
        {
            var now = DateTime.Now.Ticks;
            if (_dbMovies == null || now - _cacheTime > 10000)
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
            var now = DateTime.Now.Ticks;
            if (_dbTv == null || now - _cacheTime > 10000)
            {
                var allResults = await TvRequestService.GetAllAsync();

                var distinctResults = allResults.DistinctBy(x => x.ProviderId);
                _dbTv = distinctResults.ToDictionary(x => x.ProviderId);
                _cacheTime = now;
            }
            return _dbTv;
        }

        public RequestCountModel RequestCount()
        {
            var movieQuery = MovieRequestService.GetAllQueryable();
            var tvQuery = MovieRequestService.GetAllQueryable();

            var pendingMovies = movieQuery.Count(x => !x.Approved && !x.Available);
            var approvedMovies = movieQuery.Count(x => x.Approved && !x.Available);
            var availableMovies = movieQuery.Count(x => x.Available);

            var pendingTv = tvQuery.Count(x => !x.Approved && !x.Available);
            var approvedTv = tvQuery.Count(x => x.Approved && !x.Available);
            var availableTv = tvQuery.Count(x => x.Available);

            return new RequestCountModel
            {
                Approved = approvedTv + approvedMovies,
                Available = availableTv + availableMovies,
                Pending = pendingMovies + pendingTv
            };
        }
    }
}