using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Requests.Tv;
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
            var movieRequests = await Movie.GetAllAsync();
            var existing = movieRequests.FirstOrDefault(x => x.ProviderId == obj.Id);
            if (existing != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = existing.Approved;
                obj.Available = existing.Available;

                return Success();
            }
            
            var tvRequests = await Tv.GetAllAsync();
            var tv = tvRequests.FirstOrDefault(x => x.ProviderId == obj.Id);
            if (tv != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = tv.Approved;
                obj.Available = tv.Available;

                return Success();
            }
            return Success();
        }
    }
}