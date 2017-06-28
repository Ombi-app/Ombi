using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules.Search
{
    public class ExistingRequestRule : BaseSearchRule, IRequestRules<SearchViewModel>
    {
        public ExistingRequestRule(IMovieRequestRepository movie, ITvRequestRepository tv)
        {
            Movie = movie;
            Tv = tv;
        }

        private IMovieRequestRepository Movie { get; }
        private ITvRequestRepository Tv { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            var movieRequests = Movie.Get();
            var existing = await movieRequests.FirstOrDefaultAsync(x => x.TheMovieDbId == obj.Id);
            if (existing != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = existing.Approved;
                obj.Available = existing.Available;

                return Success();
            }
            
            var tvRequests = Tv.Get();
            var tv = await tvRequests.FirstOrDefaultAsync(x => x.TvDbId == obj.Id);
            if (tv != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = tv.ChildRequests.Any(x => x.Approved);
                obj.Available = tv.ChildRequests.Any(x => x.Available);

                return Success();
            }
            return Success();
        }
    }
}