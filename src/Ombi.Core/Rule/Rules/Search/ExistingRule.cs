using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules.Search
{
    public class ExistingRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public ExistingRule(IMovieRequestRepository movie, ITvRequestRepository tv)
        {
            Movie = movie;
            Tv = tv;
        }

        private IMovieRequestRepository Movie { get; }
        private ITvRequestRepository Tv { get; }

        public Task<RuleResult> Execute(SearchViewModel obj)
        {
            var movieRequests = Movie.GetRequest(obj.Id);
            if (movieRequests != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = movieRequests.Approved;
                obj.Available = movieRequests.Available;

                return Task.FromResult(Success());
            }
            
            var tvRequests = Tv.GetRequest(obj.Id);
            if (tvRequests != null) // Do we already have a request for this?
            {

                obj.Requested = true;
                obj.Approved = tvRequests.ChildRequests.Any(x => x.Approved);
                obj.Available = tvRequests.ChildRequests.Any(x => x.Available);

                return Task.FromResult(Success());
            }
            return Task.FromResult(Success());
        }
    }
}