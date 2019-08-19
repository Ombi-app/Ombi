using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Store.Entities;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine
{
    public class UserStatsEngine : IUserStatsEngine
    {
        public UserStatsEngine(IMovieRequestRepository movieRequest, ITvRequestRepository tvRequest)
        {
            _movieRequest = movieRequest;
            _tvRequest = tvRequest;
        }

        private readonly IMovieRequestRepository _movieRequest;
        private readonly ITvRequestRepository _tvRequest;

        public async Task<UserStatsSummary> GetSummary(SummaryRequest request)
        {
            // get all movie requests
            var movies = _movieRequest.GetWithUser();
            var filteredMovies = movies.Where(x => x.RequestedDate >= request.From && x.RequestedDate <= request.To);
            var tv = _tvRequest.GetLite();
            var children = tv.SelectMany(x =>
                x.ChildRequests.Where(c => c.RequestedDate >= request.From && c.RequestedDate <= request.To));
            
            var userMovie = filteredMovies.GroupBy(x => x.RequestedUserId).OrderBy(x => x.Key).FirstOrDefaultAsync();
            var userTv = children.GroupBy(x => x.RequestedUserId).OrderBy(x => x.Key).FirstOrDefaultAsync();

            var moviesCount = filteredMovies.CountAsync();
            var childrenCount = children.CountAsync();
            var availableMovies =
                filteredMovies.Select(x => x.MarkedAsAvailable >= request.From && x.MarkedAsAvailable <= request.To).CountAsync();
            var availableChildren = children.Where(c => c.MarkedAsAvailable >= request.From && c.MarkedAsAvailable <= request.To).CountAsync();
            
            return new UserStatsSummary
            {
                TotalMovieRequests = await moviesCount,
                TotalTvRequests = await childrenCount,
                CompletedRequestsTv = await availableChildren,
                CompletedRequestsMovies = await availableMovies,
                MostRequestedUserMovie = (await userMovie).FirstOrDefault()?.RequestedUser ?? new OmbiUser(),
                MostRequestedUserTv = (await userTv).FirstOrDefault()?.RequestedUser ?? new OmbiUser(),
            };
        }
    }

    public class SummaryRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class UserStatsSummary
    {
        public int TotalRequests => TotalTvRequests + TotalMovieRequests;
        public int TotalMovieRequests { get; set; }
        public int TotalTvRequests { get; set; }
        public int TotalIssues { get; set; }
        public int CompletedRequestsMovies { get; set; }
        public int CompletedRequestsTv { get; set; }
        public int CompletedRequests => CompletedRequestsMovies + CompletedRequestsTv;
        public OmbiUser MostRequestedUserMovie { get; set; }
        public OmbiUser MostRequestedUserTv { get; set; }

    }
}
