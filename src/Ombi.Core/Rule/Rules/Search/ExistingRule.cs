using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2.Music;
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
            if (obj is SearchMovieViewModel movie)
            {
                var movieRequests = await Movie.GetRequestAsync(obj.Id);
                if (movieRequests != null) // Do we already have a request for this?
                {
                    // If the RequestDate is a min value, that means there's only a 4k request
                    movie.Requested = movieRequests.RequestedDate != DateTime.MinValue;
                    movie.RequestId = movieRequests.Id;
                    movie.Approved = movieRequests.Approved;
                    movie.Denied = movieRequests.Denied ?? false;
                    movie.DeniedReason = movieRequests.DeniedReason;
                    movie.Available = movieRequests.Available;
                    movie.Has4KRequest = movieRequests.Has4KRequest;
                    movie.RequestedDate4k = movieRequests.RequestedDate4k;
                    movie.Approved4K = movieRequests.Approved4K;
                    movie.Available4K = movieRequests.Available4K;
                    movie.Denied4K = movieRequests.Denied4K;
                    movie.DeniedReason4K = movieRequests.DeniedReason4K;
                    movie.MarkedAsApproved4K = movieRequests.MarkedAsApproved4K;
                    movie.MarkedAsAvailable4K = movieRequests.MarkedAsAvailable4K;
                    movie.MarkedAsDenied4K = movieRequests.MarkedAsDenied4K;

                    return Success();
                }
                return Success();
            }
            if (obj.Type == RequestType.TvShow)
            {
                var request = (SearchTvShowViewModel)obj;
                var tvRequests = Tv.GetRequest(obj.Id);
                if (tvRequests != null) // Do we already have a request for this?
                {
                    request.RequestId = tvRequests.Id;
                    request.Requested = true;
                    request.Approved = tvRequests.ChildRequests.Any(x => x.Approved);
                    request.Denied = tvRequests.ChildRequests.Any(x => x.Denied ?? false);
                    request.DeniedReason = tvRequests.ChildRequests.FirstOrDefault(x => x.Denied == true).DeniedReason;

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
                                episodeSearching.Denied = request.Denied;
                            }
                        }
                    }
                }

                if (request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.All(e => e.Available && e.AirDate > DateTime.MinValue)))
                {
                    request.FullyAvailable = true;
                }
                if (request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.Any(e => e.Available && e.AirDate > DateTime.MinValue  && e.AirDate <= DateTime.UtcNow)))
                {
                    request.PartlyAvailable = true;
                }

                if (request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.All(e => e.Denied ?? false)))
                {
                    request.FullyDenied = true;
                }

                var hasUnairedRequests = request.SeasonRequests.Any() && request.SeasonRequests.All(x => x.Episodes.Any(e => e.AirDate >= DateTime.UtcNow));

                if (request.FullyAvailable)
                {
                    request.PartlyAvailable = hasUnairedRequests;
                }

                return Success();
            }
            if (obj.Type == RequestType.Album)
            {
                if (obj is SearchAlbumViewModel album)
                {
                    var albumRequest = await Music.GetRequestAsync(album.ForeignAlbumId);
                    if (albumRequest != null) // Do we already have a request for this?
                    {
                        obj.Requested = true;
                        obj.RequestId = albumRequest.Id; 
                        obj.Denied = albumRequest.Denied;
                        obj.DeniedReason = albumRequest.DeniedReason;
                        obj.Approved = albumRequest.Approved;
                        obj.Available = albumRequest.Available;

                        return Success();
                    }
                }
                if (obj is ReleaseGroup release)
                {
                    var albumRequest = await Music.GetRequestAsync(release.Id);
                    if (albumRequest != null) // Do we already have a request for this?
                    {
                        obj.Requested = true;
                        obj.RequestId = albumRequest.Id;
                        obj.Approved = albumRequest.Approved;
                        obj.Available = albumRequest.Available;

                        return Success();
                    }
                }

                return Success();
            }
            return Success();
        }
    }
}