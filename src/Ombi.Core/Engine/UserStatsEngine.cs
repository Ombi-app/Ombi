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
    public class UserStatsEngine
    {
        public UserStatsEngine(OmbiUserManager um, IMovieRequestRepository movieRequest, ITvRequestRepository tvRequest)
        {
            _userManager = um;
            _movieRequest = movieRequest;
            _tvRequest = tvRequest;
        }

        private readonly OmbiUserManager _userManager;
        private readonly IMovieRequestRepository _movieRequest;
        private readonly ITvRequestRepository _tvRequest;

        public async Task<UserStatsSummary> GetSummary(SummaryRequest request)
        {
            /* What do we want?
           
            This is Per week/month/all time (filter by date)

            1. Total Requests
            2. Total Movie Requests
            3. Total Tv Requests
            4. Total Issues (If enabled)
            5. Total Requests fufilled (now available)

            Then

           2. Most requested user Movie
           3. Most requested user tv

            Then

           1. 

            */

            // get all movie requests
            var movies = _movieRequest.GetWithUser();
            var filteredMovies = movies.Where(x => x.RequestedDate >= request.From && x.RequestedDate <= request.To);
            var tv = _tvRequest.GetLite();
            var children = tv.SelectMany(x =>
                x.ChildRequests.Where(c => c.RequestedDate >= request.From && c.RequestedDate <= request.To));

            var moviesCount = filteredMovies.CountAsync();
            var childrenCount = children.CountAsync();
            var availableMovies =
                movies.Select(x => x.MarkedAsAvailable >= request.From && x.MarkedAsAvailable <= request.To).CountAsync();
            var availableChildren = tv.SelectMany(x =>
                x.ChildRequests.Where(c => c.MarkedAsAvailable >= request.From && c.MarkedAsAvailable <= request.To)).CountAsync();

            var userMovie = filteredMovies.GroupBy(x => x.RequestedUserId).OrderBy(x => x.Key).FirstOrDefaultAsync();
            var userTv = children.GroupBy(x => x.RequestedUserId).OrderBy(x => x.Key).FirstOrDefaultAsync();


            return new UserStatsSummary
            {
                TotalMovieRequests = await moviesCount,
                TotalTvRequests = await childrenCount,
                CompletedRequestsTv = await availableChildren,
                CompletedRequestsMovies = await availableMovies,
                MostRequestedUserMovie = (await userMovie).FirstOrDefault().RequestedUser,
                MostRequestedUserTv = (await userTv).FirstOrDefault().RequestedUser,
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
        public int TotalRequests => TotalTvRequests + TotalTvRequests;
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
