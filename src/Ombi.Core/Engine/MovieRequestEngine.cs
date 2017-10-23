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
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine
{
    public class MovieRequestEngine : BaseMediaEngine, IMovieRequestEngine
    {
        public MovieRequestEngine(IMovieDbApi movieApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IRuleEvaluator r, IMovieSender sender, ILogger<MovieRequestEngine> log,
            OmbiUserManager manager) : base(user, requestService, r, manager)
        {
            MovieApi = movieApi;
            NotificationHelper = helper;
            Sender = sender;
            Logger = log;
        }

        private IMovieDbApi MovieApi { get; }
        private INotificationHelper NotificationHelper { get; }
        private IMovieSender Sender { get; }
        private ILogger<MovieRequestEngine> Logger { get; }

        /// <summary>
        /// Requests the movie.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model)
        {
            var movieInfo = await MovieApi.GetMovieInformation(model.Id);
            if (movieInfo == null || movieInfo.Id == 0)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    Message = "There was an issue adding this movie!",
                    ErrorMessage = $"TheMovieDb didn't have any information for ID {model.Id}"
                };
            }
            var fullMovieName =
                $"{movieInfo.Title}{(!string.IsNullOrEmpty(movieInfo.ReleaseDate) ? $" ({DateTime.Parse(movieInfo.ReleaseDate).Year})" : string.Empty)}";

            var userDetails = await GetUser();

            var requestModel = new MovieRequests
            {
                TheMovieDbId = movieInfo.Id,
                RequestType = RequestType.Movie,
                Overview = movieInfo.Overview,
                ImdbId = movieInfo.ImdbId,
                PosterPath = PosterPathHelper.FixPosterPath(movieInfo.PosterPath),
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
                    ErrorMessage = ruleResults.FirstOrDefault(x => x.Message.HasValue()).Message
                };
            }

            if (requestModel.Approved) // The rules have auto approved this
            {
                var requestEngineResult = await AddMovieRequest(requestModel, fullMovieName);
                if (requestEngineResult.Result)
                {
                    var result = await ApproveMovie(requestModel);
                    if (result.IsError)
                    {
                        Logger.LogWarning("Tried auto sending movie but failed. Message: {0}", result.Message);
                        return new RequestEngineResult
                        {
                            Message = result.Message,
                            ErrorMessage = result.Message,
                            Result = false
                        };
                    }

                    return requestEngineResult;
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
            var allRequests = await MovieRepository.GetWithUser().Skip(position).Take(count).ToListAsync();
            allRequests.ForEach(x =>
            {
                x.PosterPath = PosterPathHelper.FixPosterPath(x.PosterPath);
            });
            return allRequests;
        }

        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> GetRequests()
        {
            var allRequests = await MovieRepository.GetWithUser().ToListAsync();
            return allRequests;
        }

        /// <summary>
        /// Searches the movie request.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search)
        {
            var allRequests = await MovieRepository.GetWithUser().ToListAsync();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            return results;
        }
        
        public async Task<RequestEngineResult> ApproveMovieById(int requestId)
        {
            var request = await MovieRepository.Find(requestId);
            return await ApproveMovie(request);
        }

        public async Task<RequestEngineResult> DenyMovieById(int modelId)
        {
            var request = await MovieRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }
            request.Denied = true;
            // We are denying a request
            NotificationHelper.Notify(request, NotificationType.RequestDeclined);
                await MovieRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request successfully deleted",
            };
        }

        /// <summary>
        /// This is the method that is triggered by pressing Approve on the requests page
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RequestEngineResult> ApproveMovie(MovieRequests request)
        {
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }
            request.Approved = true;
            request.Denied = false;
            await MovieRepository.Update(request);

            NotificationHelper.Notify(request, NotificationType.RequestApproved);

            if (request.Approved)
            {
                var result = await Sender.Send(request);
                if (result.Success && result.Sent)
                {
                    return new RequestEngineResult
                    {
                        Result = true
                    };
                }
                if (!result.Success)
                {
                    Logger.LogWarning("Tried auto sending movie but failed. Message: {0}", result.Message);
                    return new RequestEngineResult
                    {
                        Message = result.Message,
                        ErrorMessage = result.Message,
                        Result = false
                    };
                }
                // If there are no providers then it's successful but movie has not been sent
            }

            return new RequestEngineResult
            {
                Result = true
            };
        }

        /// <summary>
        /// Updates the movie request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<MovieRequests> UpdateMovieRequest(MovieRequests request)
        {
            var allRequests = await MovieRepository.GetWithUser().ToListAsync();
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
            results.QualityOverride = request.QualityOverride;
            results.RootPathOverride = request.RootPathOverride;

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
            var request = await MovieRepository.GetAll().FirstOrDefaultAsync(x => x.Id == requestId);
            await MovieRepository.Delete(request);
        }

        public async Task<bool> UserHasRequest(string userId)
        {
            return await MovieRepository.GetAll().AnyAsync(x => x.RequestedUserId == userId);
        }

        public async Task<RequestEngineResult> MarkUnavailable(int modelId)
        {
            var request = await MovieRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }
            request.Available = false;
            await MovieRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request is now unavailable",
                Result = true
            };
        }

        public async Task<RequestEngineResult> MarkAvailable(int modelId)
        {
            var request = await MovieRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }
            request.Available = true;
            NotificationHelper.Notify(request, NotificationType.RequestAvailable);
            await MovieRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request is now available",
                Result = true
            };
        }

        private async Task<RequestEngineResult> AddMovieRequest(MovieRequests model, string movieName)
        {
            await MovieRepository.Add(model);

            var result = await RunSpecificRule(model, SpecificRules.CanSendNotification);
            if (result.Success)
            {
                NotificationHelper.NewRequest(model);
            }

            return new RequestEngineResult { Result = true, Message = $"{movieName} has been successfully added!" };
        }

        public async Task<IEnumerable<MovieRequests>> GetApprovedRequests()
        {
            var allRequests = MovieRepository.GetWithUser();
            return await allRequests.Where(x => x.Approved && !x.Available).ToListAsync();
        }

        public async Task<IEnumerable<MovieRequests>> GetNewRequests()
        {
            var allRequests = MovieRepository.GetWithUser();
            return await allRequests.Where(x => !x.Approved && !x.Available).ToListAsync();
        }

        public async Task<IEnumerable<MovieRequests>> GetAvailableRequests()
        {
            var allRequests = MovieRepository.GetWithUser();
            return await allRequests.Where(x => !x.Approved && x.Available).ToListAsync();
        }
    }
}