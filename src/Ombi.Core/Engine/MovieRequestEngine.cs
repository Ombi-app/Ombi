using Hangfire;
using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using Ombi.Core.Rules;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Engine
{
    public class MovieRequestEngine : BaseMediaEngine, IMovieRequestEngine
    {
        public MovieRequestEngine(IMovieDbApi movieApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationService notificationService, IRuleEvaluator r) : base(user, requestService, r)
        {
            MovieApi = movieApi;
            NotificationService = notificationService;
        }

        private IMovieDbApi MovieApi { get; }
        private INotificationService NotificationService { get; }

        public async Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model)
        {
            var movieInfo = await MovieApi.GetMovieInformation(model.Id);
            if (movieInfo == null)
                return new RequestEngineResult
                {
                    RequestAdded = false,
                    Message = "There was an issue adding this movie!",
                    ErrorMessage = $"TheMovieDb didn't have any information for ID {model.Id}"
                };
            var fullMovieName =
                $"{movieInfo.Title}{(!string.IsNullOrEmpty(movieInfo.ReleaseDate) ? $" ({DateTime.Parse(movieInfo.ReleaseDate).Year})" : string.Empty)}";

            var existingRequest = await MovieRequestService.CheckRequestAsync(model.Id);
            if (existingRequest != null)
                return new RequestEngineResult
                {
                    RequestAdded = false,
                    Message = $"{fullMovieName} has already been requested"
                };

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

            var requestModel = new MovieRequestModel
            {
                ProviderId = movieInfo.Id,
                Type = RequestType.Movie,
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
                RequestedUsers = new List<string> {Username},
                Issues = IssueState.None
            };

            try
            {
                var ruleResults = RunRules(requestModel).ToList();
                if (ruleResults.Any(x => !x.Success))
                    return new RequestEngineResult
                    {
                        ErrorMessage = ruleResults.FirstOrDefault(x => !string.IsNullOrEmpty(x.Message)).Message
                    };

                if (requestModel.Approved) // The rules have auto approved this
                {
                    //    var result = await MovieSender.Send(model);
                    //    if (result.Result)
                    //    {
                    //        return await AddRequest(model, settings,
                    //            $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
                    //    }
                    //    if (result.Error)

                    //    {
                    //        return
                    //            Response.AsJson(new JsonResponseModel
                    //            {
                    //                Message = "Could not add movie, please contact your administrator",
                    //                Result = false
                    //            });
                    //    }
                    //    if (!result.MovieSendingEnabled)
                    //    {
                    //        return await AddRequest(model, settings, $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
                    //    }

                    //    return Response.AsJson(new JsonResponseModel
                    //    {
                    //        Result = false,
                    //        Message = Resources.UI.Search_CouchPotatoError
                    //    });
                }

                return await AddMovieRequest(requestModel, /*settings,*/
                    $"{fullMovieName} has been successfully added!");
            }
            catch (Exception e)
            {
                //Log.Fatal(e);
                //await FaultQueue.QueueItemAsync(model, movieInfo.Id.ToString(), RequestType.Movie, FaultType.RequestFault, e.Message);
                var notification = new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.Movie,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                };
                BackgroundJob.Enqueue(() => NotificationService.Publish(notification).Wait());

                //return Response.AsJson(new JsonResponseModel
                //{
                //    Result = true,
                //    Message = $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}"
                //});
            }

            return null;
        }

        public async Task<IEnumerable<MovieRequestModel>> GetMovieRequests(int count, int position)
        {
            var allRequests = await MovieRequestService.GetAllAsync(count, position);
            return allRequests;
        }

        public async Task<IEnumerable<MovieRequestModel>> SearchMovieRequest(string search)
        {
            var allRequests = await MovieRequestService.GetAllAsync();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            return results;
        }

        public async Task<MovieRequestModel> UpdateMovieRequest(MovieRequestModel request)
        {
            var allRequests = await MovieRequestService.GetAllAsync();
            var results = allRequests.FirstOrDefault(x => x.Id == request.Id);

            results.Approved = request.Approved;
            results.Available = request.Available;
            results.Denied = request.Denied;
            results.DeniedReason = request.DeniedReason;
            results.AdminNote = request.AdminNote;
            results.ImdbId = request.ImdbId;
            results.IssueId = request.IssueId;
            results.Issues = request.Issues;
            results.OtherMessage = request.OtherMessage;
            results.Overview = request.Overview;
            results.PosterPath = request.PosterPath;
            results.RequestedUsers = request.RequestedUsers?.ToList() ?? new List<string>();

            var model = MovieRequestService.UpdateRequest(results);
            return model;
        }

        public async Task RemoveMovieRequest(int requestId)
        {
            await MovieRequestService.DeleteRequestAsync(requestId);
        }

        private IEnumerable<EpisodesModel> GetListDifferences(IEnumerable<EpisodesModel> existing,
            IEnumerable<EpisodesModel> request)
        {
            var newRequest = request
                .Select(r =>
                    new EpisodesModel
                    {
                        SeasonNumber = r.SeasonNumber,
                        EpisodeNumber = r.EpisodeNumber
                    })
                .ToList();

            return newRequest.Except(existing);
        }

        private async Task<RequestEngineResult> AddMovieRequest(MovieRequestModel model, string message)
        {
            await MovieRequestService.AddRequestAsync(model);

            if (ShouldSendNotification(model.Type))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type,
                    ImgSrc = model.Type == RequestType.Movie
                        ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}"
                        : model.PosterPath
                };

                BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel).Wait());
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

            return new RequestEngineResult {RequestAdded = true};
        }
    }
}