using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
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
            if (obj.Type == RequestType.Movie)
            {
                var movieRequests = Movie.GetRequest(obj.Id);
                if (movieRequests != null) // Do we already have a request for this?
                {

                    obj.Requested = true;
                    obj.Approved = movieRequests.Approved;
                    obj.Available = movieRequests.Available;

                    return Task.FromResult(Success());
                }
                return Task.FromResult(Success());
            }
            else
            {
                //var tvRequests = Tv.GetRequest(obj.Id);
                //if (tvRequests != null) // Do we already have a request for this?
                //{

                //    obj.Requested = true;
                //    obj.Approved = tvRequests.ChildRequests.Any(x => x.Approved);
                //    obj.Available = tvRequests.ChildRequests.Any(x => x.Available);

                //    return Task.FromResult(Success());
                //}

                var request = (SearchTvShowViewModel) obj;
                var tvRequests = Tv.GetRequest(obj.Id);
                if (tvRequests != null) // Do we already have a request for this?
                {

                    request.Requested = true;
                    request.Approved = tvRequests.ChildRequests.Any(x => x.Approved);

                    // Let's modify the seasonsrequested to reflect what we have requested...
                    foreach (var season in request.SeasonRequests)
                    {
                        foreach (var existingRequestChildRequest in tvRequests.ChildRequests)
                        {
                            // Find the existing request season
                            var existingSeason =
                                existingRequestChildRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == season.SeasonNumber);
                            if (existingSeason == null) continue;

                            foreach (var ep in existingSeason.Episodes)
                            {
                                // Find the episode from what we are searching
                                var episodeSearching = season.Episodes.FirstOrDefault(x => x.EpisodeNumber == ep.EpisodeNumber);
                                if (episodeSearching == null)
                                {
                                    continue;
                                }
                                episodeSearching.Requested = true;
                                episodeSearching.Available = ep.Available;
                                episodeSearching.Approved = ep.Season.ChildRequest.Approved;
                            }
                        }
                    }
                }

                if (request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.All(e => e.Approved)))
                {
                    request.FullyAvailable = true;
                }


                return Task.FromResult(Success());
            }
        }
    }
}