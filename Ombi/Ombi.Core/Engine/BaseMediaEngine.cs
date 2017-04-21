using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models.Requests;
using Ombi.Core.Requests.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public abstract class BaseMediaEngine : BaseEngine
    {
        protected BaseMediaEngine(IUserIdentityManager identity, IRequestService service) : base(identity)
        {
            RequestService = service;
        }

        protected IRequestService RequestService { get; }

        private long _dbMovieCacheTime = 0;
        private Dictionary<int, RequestModel> _dbMovies;
        protected async Task<Dictionary<int, RequestModel>> GetRequests(RequestType type)
        {
            long now = DateTime.Now.Ticks;
            if (_dbMovies == null || (now - _dbMovieCacheTime) > 10000)
            {
                var allResults = await RequestService.GetAllAsync();
                allResults = allResults.Where(x => x.Type == type);

                var distinctResults = allResults.DistinctBy(x => x.ProviderId);
                _dbMovies = distinctResults.ToDictionary(x => x.ProviderId);
                _dbMovieCacheTime = now;
            }
            return _dbMovies;
        }
    }
}