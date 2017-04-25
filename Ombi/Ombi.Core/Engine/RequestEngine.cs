using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Hangfire;
using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Store.Entities;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;

namespace Ombi.Core.Engine
{
    public class RequestEngine : BaseMediaEngine, IRequestEngine
    {
        public RequestEngine(IMovieDbApi movieApi, IRequestService requestService, IPrincipal user, INotificationService notificationService) : base(user, requestService)
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

            var existingRequest = await RequestService.CheckRequestAsync(model.Id);
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

            var requestModel = new RequestModel
            {
                ProviderId = movieInfo.Id,
                Type = RequestType.Movie,
                Overview = movieInfo.Overview,
                ImdbId = movieInfo.ImdbId,
                PosterPath = movieInfo.PosterPath,
                Title = movieInfo.Title,
                ReleaseDate = !string.IsNullOrEmpty(movieInfo.ReleaseDate) ? DateTime.Parse(movieInfo.ReleaseDate) : DateTime.MinValue,
                Status = movieInfo.Status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,
            };

            try
            {
                if (ShouldAutoApprove(RequestType.Movie))
                {
                    model.Approved = true;

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


                return await AddRequest(requestModel, /*settings,*/
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


        private async Task<RequestEngineResult> AddRequest(RequestModel model, string message)
        {
            await RequestService.AddRequestAsync(model);

            if (ShouldSendNotification(model.Type))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type,
                    ImgSrc = model.Type == RequestType.Movie ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}" : model.PosterPath
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

            return new RequestEngineResult{RequestAdded = true};
        }

        public async Task<IEnumerable<RequestViewModel>> GetRequests(int count, int position)
        {
            var allRequests = await RequestService.GetAllAsync(count, position);
            var viewModel = MapToVm(allRequests);
            return viewModel;
        }
        public async Task<IEnumerable<RequestViewModel>> SearchRequest(string search)
        {
            var allRequests = await RequestService.GetAllAsync();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            var viewModel = MapToVm(results);
            return viewModel;
        }
        public async Task<RequestViewModel> UpdateRequest(RequestViewModel request)
        {
            var allRequests = await RequestService.GetAllAsync();
            var results = allRequests.FirstOrDefault(x => x.Id == request.Id);

            results.Approved = request.Approved;
            results.Available = request.Available;
            results.Denied = request.Denied;
            results.DeniedReason = request.DeniedReason;
            //results.AdminNote = request.AdminNote;
            results.ImdbId = request.ImdbId;
            results.Episodes = request.Episodes?.ToList() ?? new List<EpisodesModel>();
            results.IssueId = request.IssueId;
            //results.Issues = request.Issues;
            //results.OtherMessage = request.OtherMessage;
            results.Overview = request.Overview;
            results.PosterPath = request.PosterPath;
            results.RequestedUsers = request.RequestedUsers?.ToList() ?? new List<string>();
            //results.RootFolderSelected = request.RootFolderSelected; 


            var model = RequestService.UpdateRequest(results);
            return MapToVm(new List<RequestModel>{model}).FirstOrDefault();
        }

        public async Task RemoveRequest(int requestId)
        {
            await RequestService.DeleteRequestAsync(requestId);
        }


       
    }
}