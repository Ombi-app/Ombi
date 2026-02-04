using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Core.Engine;
using Ombi.Store.Repository.Requests;
using Ombi.Core.Services;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Rule.Rules.Request
{
    public class ExistingMovieRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        private readonly IFeatureService _featureService;

        public ExistingMovieRequestRule(IMovieRequestRepository movie, IFeatureService featureService)
        {
            Movie = movie;
            _featureService = featureService;
        }

        private IMovieRequestRepository Movie { get; }

        /// <summary>
        /// We check if the request exists, if it does then we don't want to re-request it.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            if (obj.RequestType == RequestType.Movie)
            {
                var movie = (MovieRequests) obj;
                var movieRequests = Movie.GetAll();
                var found = false;
                var existing = await movieRequests.FirstOrDefaultAsync(x => x.TheMovieDbId == movie.TheMovieDbId);
                if (existing != null) // Do we already have a request for this?
                {
                    found = await Check4KRequests(movie, existing);
                }

                if (!found && movie.ImdbId.HasValue())
                {
                   // Let's check imdbid
                   existing = await movieRequests.FirstOrDefaultAsync(x =>
                       x.ImdbId == movie.ImdbId);
                   if (existing != null)
                   {
                       found = await Check4KRequests(movie, existing);
                   }
                }
                if (found)
                {
                    return Fail(ErrorCode.AlreadyRequested, $"\"{obj.Title}\" has already been requested");
                }
            }
            return Success();
        }

        private async Task<bool> Check4KRequests(MovieRequests movie, MovieRequests existing)
        {
            var featureEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);
            
            // If requesting 4K and 4K feature is enabled, check if 4K request already exists
            if (movie.Is4kRequest && featureEnabled)
            {
               return existing.Has4KRequest;
            }
            
            // If requesting non-4K
            if (!movie.Is4kRequest)
            {
                // If 4K feature is disabled, any existing request (4K or non-4K) is a duplicate
                if (!featureEnabled)
                {
                    return existing.RequestedDate != default || existing.Has4KRequest;
                }
                // If 4K feature is enabled, only reject if non-4K request exists
                else
                {
                    return existing.RequestedDate != default;
                }
            }

            return false;
        }
    }
}