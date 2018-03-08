using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Store.Entities;
using Microsoft.AspNetCore.Identity;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Engine
{
    public abstract class BaseMediaEngine : BaseEngine
    {
        private long _cacheTime;
        private Dictionary<int, MovieRequests> _dbMovies;
        private Dictionary<int, TvRequests> _dbTv;

        protected BaseMediaEngine(IPrincipal identity, IRequestServiceMain requestService,
            IRuleEvaluator rules, OmbiUserManager um, ICacheService cache, ISettingsService<OmbiSettings> ombiSettings) : base(identity, um, rules)
        {
            RequestService = requestService;
            Cache = cache;
            OmbiSettings = ombiSettings;
        }

        protected IRequestServiceMain RequestService { get; }
        protected IMovieRequestRepository MovieRepository => RequestService.MovieRequestService;
        protected ITvRequestRepository TvRepository => RequestService.TvRequestService;
        protected readonly ICacheService Cache;
        protected readonly ISettingsService<OmbiSettings> OmbiSettings;

        protected async Task<Dictionary<int, MovieRequests>> GetMovieRequests()
        {
            var now = DateTime.Now.Ticks;
            if (_dbMovies == null || now - _cacheTime > 10000)
            {
                var allResults = await MovieRepository.GetAll().ToListAsync();

                var distinctResults = allResults.DistinctBy(x => x.TheMovieDbId);
                _dbMovies = distinctResults.ToDictionary(x => x.TheMovieDbId);
                _cacheTime = now;
            }
            return _dbMovies;
        }

        protected async Task<Dictionary<int, TvRequests>> GetTvRequests()
        {
            var now = DateTime.Now.Ticks;
            if (_dbTv == null || now - _cacheTime > 10000)
            {
                var allResults = await TvRepository.Get().ToListAsync();

                var distinctResults = allResults.DistinctBy(x => x.TvDbId);
                _dbTv = distinctResults.ToDictionary(x => x.TvDbId);
                _cacheTime = now;
            }
            return _dbTv;
        }

        public RequestCountModel RequestCount()
        {
            var movieQuery = MovieRepository.GetAll();
            var tvQuery = TvRepository.Get();

            var pendingMovies = movieQuery.Count(x => !x.Approved && !x.Available);
            var approvedMovies = movieQuery.Count(x => x.Approved && !x.Available);
            var availableMovies = movieQuery.Count(x => x.Available);

            var pendingTv = 0;
            var approvedTv = 0;
                var availableTv = 0;
            foreach (var tv in tvQuery)
            {
                foreach (var child in tv.ChildRequests)
                {
                    if (!child.Approved && !child.Available)
                    {
                        pendingTv++;
                    }
                    if (child.Approved && !child.Available)
                    {
                        approvedTv++;
                    }
                    if (child.Available)
                    {
                        availableTv++;
                    }
                }
            }

            return new RequestCountModel
            {
                Approved = approvedTv + approvedMovies,
                Available = availableTv + availableMovies,
                Pending = pendingMovies + pendingTv
            };
        }

        protected async Task<HideResult> HideFromOtherUsers()
        {
            var settings = await Cache.GetOrAdd(CacheKeys.OmbiSettings, async () => await OmbiSettings.GetSettingsAsync());
            var result = new HideResult
            {
                Hide = settings.HideRequestsUsers
            };
            if (settings.HideRequestsUsers)
            {
                var user = await GetUser();
                result.UserId = user.Id;
            }
            return result;
        }

        public class HideResult
        {
            public bool Hide { get; set; }
            public string UserId { get; set; }
        }
    }
}