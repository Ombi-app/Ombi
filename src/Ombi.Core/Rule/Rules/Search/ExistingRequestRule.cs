using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Core.Rule.Interfaces;

namespace Ombi.Core.Rule.Rules.Search
{
    public class ExistingRequestRule : BaseSearchRule, IRequestRules<SearchViewModel>
    {
        public ExistingRequestRule(IRequestService<MovieRequestModel> movie, IRequestService<TvRequestModel> tv)
        {
            Movie = movie;
            Tv = tv;
        }

        private IRequestService<MovieRequestModel> Movie { get; }
        private IRequestService<TvRequestModel> Tv { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            var movieRequests = Movie.GetAllQueryable();
            var existing = await movieRequests.FirstOrDefaultAsync(x => x.ProviderId == obj.Id);
            if (existing != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = existing.Approved;
                obj.Available = existing.Available;

                return Success();
            }
            
            var tvRequests = Tv.GetAllQueryable();
            var movieExisting = await tvRequests.FirstOrDefaultAsync(x => x.ProviderId == obj.Id);
            if (movieExisting != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = movieExisting.Approved;
                obj.Available = movieExisting.Available;

                return Success();
            }
            return Success();
        }
    }
}