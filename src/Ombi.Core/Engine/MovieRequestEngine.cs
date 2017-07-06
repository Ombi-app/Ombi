using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine
{
    public class MovieRequestEngine : BaseMediaEngine, IMovieRequestEngine
    {
        public MovieRequestEngine(IMovieDbApi movieApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IRuleEvaluator r, IMovieSender sender, ILogger<MovieRequestEngine> log, IUserIdentityManager manager) : base(user, requestService, r)
        {
            MovieApi = movieApi;
            NotificationHelper = helper;
            Sender = sender;
            Logger = log;
            UserManager = manager;
        }

        private IMovieDbApi MovieApi { get; }
        private INotificationHelper NotificationHelper { get; }
        private IMovieSender Sender { get; }
        private ILogger<MovieRequestEngine> Logger { get; }
        private IUserIdentityManager UserManager { get; }

        /// <summary>
        /// Requests the movie.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model)
        {
            var movieInfo = await MovieApi.GetMovieInformation(model.Id);
            if (movieInfo == null)
            {
                return new RequestEngineResult
                {
                    RequestAdded = false,
                    Message = "There was an issue adding this movie!",
                    ErrorMessage = $"TheMovieDb didn't have any information for ID {model.Id}"
                };
            }
            var fullMovieName =
                $"{movieInfo.Title}{(!string.IsNullOrEmpty(movieInfo.ReleaseDate) ? $" ({DateTime.Parse(movieInfo.ReleaseDate).Year})" : string.Empty)}";

            var userDetails = await UserManager.GetUser(User.Identity.Name);

            var requestModel = new MovieRequests
            {
                TheMovieDbId = movieInfo.Id,
                RequestType = RequestType.Movie,
                Overview = movieInfo.Overview,
                ImdbId = movieInfo.ImdbId,
                PosterPath = movieInfo.PosterPath,
                Title = movieInfo.Title,
                ReleaseDate = !string.IsNullOrEmpty(movieInfo.ReleaseDate)
                    ? DateTime.Parse(movieInfo.ReleaseDate)
                    : DateTime.MinValue,
                Status = movieInfo.Status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUserId = userDetails.Id,
            };

            var ruleResults = (await RunRequestRules(requestModel)).ToList();
            if (ruleResults.Any(x => !x.Success))
            {
                return new RequestEngineResult
                {
                    ErrorMessage = ruleResults.FirstOrDefault(x => !string.IsNullOrEmpty(x.Message)).Message
                };
            }

            if (requestModel.Approved) // The rules have auto approved this
            {
                var result = await Sender.Send(requestModel);
                if (result.Success && result.MovieSent)
                {
                    return await AddMovieRequest(requestModel, fullMovieName);
                }
                if (!result.Success)
                {
                    Logger.LogWarning("Tried auto sending movie but failed. Message: {0}", result.Message);
                    return new RequestEngineResult
                    {
                        Message = result.Message,
                        ErrorMessage = result.Message,
                        RequestAdded = false
                    };
                }
                // If there are no providers then it's successful but movie has not been sent
            }

            return await AddMovieRequest(requestModel, fullMovieName);
        }


        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> GetRequests(int count, int position)
        {
            var allRequests = await MovieRepository.Get().Skip(position).Take(count).ToListAsync();
            return allRequests;
        }

        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> GetRequests()
        {
            var allRequests = await MovieRepository.Get().ToListAsync();
            return allRequests;
        }

        /// <summary>
        /// Searches the movie request.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search)
        {
            var allRequests = await MovieRepository.Get().ToListAsync();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            return results;
        }

        /// <summary>
        /// Updates the movie request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<MovieRequests> UpdateMovieRequest(MovieRequests request)
        {
            var allRequests = await MovieRepository.Get().ToListAsync();
            var results = allRequests.FirstOrDefault(x => x.Id == request.Id);

            results.Approved = request.Approved;
            results.Available = request.Available;
            results.Denied = request.Denied;
            results.DeniedReason = request.DeniedReason;
            results.ImdbId = request.ImdbId;
            results.IssueId = request.IssueId;
            results.Issues = request.Issues;
            results.Overview = request.Overview;
            results.PosterPath = request.PosterPath;
            results.RequestedUser = request.RequestedUser;

            await MovieRepository.Update(results);
            return results;
        }

        /// <summary>
        /// Removes the movie request.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        public async Task RemoveMovieRequest(int requestId)
        {
            var request = await MovieRepository.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            await MovieRepository.Delete(request);
        }

        private async Task<RequestEngineResult> AddMovieRequest(MovieRequests model, string movieName)
        {
            await MovieRepository.Add(model);

            if (ShouldSendNotification(model))
            {
                NotificationHelper.NewRequest(model);
            }

            return new RequestEngineResult { RequestAdded = true, Message = $"{movieName} has been successfully added!" };
        }

        public async Task<IEnumerable<MovieRequests>> GetApprovedRequests()
        {
            var allRequests = MovieRepository.Get();
            return await allRequests.Where(x => x.Approved && !x.Available).ToListAsync();
        }

        public async Task<IEnumerable<MovieRequests>> GetNewRequests()
        {
            var allRequests = MovieRepository.Get();
            return await allRequests.Where(x => !x.Approved && !x.Available).ToListAsync();
        }

        public async Task<IEnumerable<MovieRequests>> GetAvailableRequests()
        {
            var allRequests = MovieRepository.Get();
            return await allRequests.Where(x => !x.Approved && x.Available).ToListAsync();
        }
    }
}