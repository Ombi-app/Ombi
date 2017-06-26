using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using Ombi.Core.Rules;
using Ombi.Helpers;
using Ombi.Notifications;
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
using Ombi.Core.Rule;
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

            var existingRequest = await MovieRepository.GetRequest(model.Id);
            if (existingRequest != null)
            {
                return new RequestEngineResult
                {
                    RequestAdded = false,
                    Message = $"{fullMovieName} has already been requested"
                };
            }

            // TODO
            //try
            //{
            //    var content = PlexContentRepository.GetAll();
            //    var movies = PlexChecker.GetPlexMovies(content);
            //    if (PlexChecker.IsMovieAvailable(movies.ToArray(), movieInfo.Title, movieInfo.ReleaseDate?.Year.ToString()))
            //    {
            //        return
            //            Response.AsJson(new JsonResponseModel
            //            {
            //                Result = false,
            //                Message = $"{fullMovieName} is already in Plex!"
            //            });
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log.Error(e);
            //    return
            //        Response.AsJson(new JsonResponseModel
            //        {
            //            Result = false,
            //            Message = string.Format(Resources.UI.Search_CouldNotCheckPlex, fullMovieName, GetMediaServerName())
            //        });
            //}

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

            var ruleResults = await RunRequestRules(requestModel);
            var results = ruleResults as RuleResult[] ?? ruleResults.ToArray();
            if (results.Any(x => !x.Success))
            {
                return new RequestEngineResult
                {
                    ErrorMessage = results.FirstOrDefault(x => !string.IsNullOrEmpty(x.Message)).Message
                };
            }

            if (requestModel.Approved) // The rules have auto approved this
            {
                var result = await Sender.Send(requestModel);
                if (result.Success && result.MovieSent)
                {
                    return await AddMovieRequest(requestModel, /*settings,*/
                        $"{fullMovieName} has been successfully added!");
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

            return await AddMovieRequest(requestModel, /*settings,*/
                $"{fullMovieName} has been successfully added!");
        }

        public async Task<IEnumerable<MovieRequests>> GetRequests(int count, int position)
        {
            var allRequests = await MovieRepository.Get().Skip(position).Take(count).ToListAsync();
            return allRequests;
        }

        public async Task<IEnumerable<MovieRequests>> GetRequests()
        {
            var allRequests = await MovieRepository.Get().ToListAsync();
            return allRequests;
        }

        public async Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search)
        {
            var allRequests = await MovieRepository.Get().ToListAsync();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            return results;
        }

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

        public async Task RemoveMovieRequest(int requestId)
        {
            var request = await MovieRepository.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            await MovieRepository.Delete(request);
        }

        private async Task<RequestEngineResult> AddMovieRequest(MovieRequests model, string message)
        {
            await MovieRepository.Add(model);

            if (ShouldSendNotification(RequestType.Movie))
            {
                NotificationHelper.NewRequest(model);
            }

            //var limit = await RequestLimitRepo.GetAllAsync();
            //var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == model.Type);
            //if (usersLimit == null)
            //{
            //    await RequestLimitRepo.InsertAsync(new RequestLimit
            //    {
            //        Username = Username,
            //        RequestType = model.Type,
            //        FirstRequestDate = DateTime.UtcNow,
            //        RequestCount = 1
            //    });
            //}
            //else
            //{
            //    usersLimit.RequestCount++;
            //    await RequestLimitRepo.UpdateAsync(usersLimit);
            //}

            return new RequestEngineResult { RequestAdded = true, Message = message };
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