using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Claims;
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
        protected BaseMediaEngine(IPrincipal identity, IRequestService service) : base(identity)
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

        public async Task<IEnumerable<RequestViewModel>> GetRequests()
        {
            var allRequests = await RequestService.GetAllAsync();
            var viewModel = MapToVm(allRequests);
            return viewModel;
        }


        protected IEnumerable<RequestViewModel> MapToVm(IEnumerable<RequestModel> model)
        {
            return model.Select(movie => new RequestViewModel
            {
                ProviderId = movie.ProviderId,
                Type = movie.Type,
                Status = movie.Status,
                ImdbId = movie.ImdbId,
                Id = movie.Id,
                PosterPath = movie.PosterPath,
                ReleaseDate = movie.ReleaseDate,
                RequestedDate = movie.RequestedDate,
                Released = DateTime.Now > movie.ReleaseDate,
                Approved = movie.Available || movie.Approved,
                Title = movie.Title,
                Overview = movie.Overview,
                RequestedUsers = movie.AllUsers.ToArray(),
                ReleaseYear = movie.ReleaseDate.Year.ToString(),
                Available = movie.Available,
                Admin = HasRole(OmbiClaims.Admin),
                IssueId = movie.IssueId,
                Denied = movie.Denied,
                DeniedReason = movie.DeniedReason,
                //Qualities = qualities.ToArray(),
                //HasRootFolders = rootFolders.Any(),
                //RootFolders = rootFolders.ToArray(),
                //CurrentRootPath = radarr.Enabled ? GetRootPath(movie.RootFolderSelected, radarr).Result : null
            }).ToList();
        }
    }
}