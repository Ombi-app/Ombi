using System;
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
        public ExistingRule(IMovieRequestRepository movie, ITvRequestRepository tv, IMusicRequestRepository music)
        {
            Movie = movie;
            Tv = tv;
            Music = music;
        }

        private IMovieRequestRepository Movie { get; }
        private IMusicRequestRepository Music { get; }
        private ITvRequestRepository Tv { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            if (obj.Type == RequestType.Movie)
            {
                var movieRequests = await Movie.GetRequestAsync(obj.Id);
                if (movieRequests != null) // Do we already have a request for this?
                {

                    obj.Requested = true;
                    obj.RequestId = movieRequests.Id;
                    obj.Approved = movieRequests.Approved;
                    obj.Available = movieRequests.Available;

                    return Success();
                }
                return Success();
            }
            if (obj.Type == RequestType.TvShow)
            {
                //var tvRequests = Tv.GetRequest(obj.Id);
                //if (tvRequests != null) // Do we already have a request for this?
                //{

                //    obj.Requested = true;
                //    obj.Approved = tvRequests.ChildRequests.Any(x => x.Approved);
                //    obj.Available = tvRequests.ChildRequests.Any(x => x.Available);

                //    return Task.FromResult(Success());
                //}

                var request = (SearchTvShowViewModel)obj;
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

                if (request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.All(e => e.Available && e.AirDate > DateTime.MinValue)))
                {
                    request.FullyAvailable = true;
                }
                if (request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.Any(e => e.Available && e.AirDate > DateTime.MinValue)))
                {
                    request.PartlyAvailable = true;
                }

                return Success();
            }
            if (obj.Type == RequestType.Album)
            {
                var album = (SearchAlbumViewModel) obj;
                var albumRequest = await Music.GetRequestAsync(album.ForeignAlbumId);
                if (albumRequest != null) // Do we already have a request for this?
                {
                    obj.Requested = true;
                    obj.RequestId = albumRequest.Id;
                    obj.Approved = albumRequest.Approved;
                    obj.Available = albumRequest.Available;

                    return Success();
                }
                return Success();
            }
            return Success();
        }
    }
}