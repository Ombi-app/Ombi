using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class ExistingMovieRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        public ExistingMovieRequestRule(IMovieRequestRepository movie)
        {
            Movie = movie;
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
                    found = true;
                }

                if (!found && movie.ImdbId.HasValue())
                {
                   // Let's check imdbid
                   existing = await movieRequests.FirstOrDefaultAsync(x =>
                       x.ImdbId == movie.ImdbId);
                   if (existing != null)
                   {
                       found = true;
                   }
                }
                if(found)
                {
                    return Fail($"\"{obj.Title}\" has already been requested");
                }
            }
            return Success();
        }
    }
}