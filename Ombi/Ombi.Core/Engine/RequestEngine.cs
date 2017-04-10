using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Store.Entities;
using Ombi.TheMovieDbApi;
using Ombi.Helpers;

namespace Ombi.Core.Engine
{
    public class RequestEngine : IRequestEngine
    {
        public RequestEngine(IMovieDbApi movieApi, IRequestService requestService)
        {
            MovieApi = movieApi;
            RequestService = requestService;
        }
        private IMovieDbApi MovieApi { get; }
        private IRequestService RequestService { get; }
        public async Task<RequestEngineResult> RequestMovie(SearchMovieViewModel model)
        {
            var movieInfo = await MovieApi.GetMovieInformation(model.Id);
            if (movieInfo == null)
            {
                return new RequestEngineResult
                {
                    RequestAdded = false,
                    Message = "There was an issue adding this movie!"
                };
                //Response.AsJson(new JsonResponseModel
                //{
                //    Result = false,
                //    Message = "There was an issue adding this movie!"
                //});
            }
            var fullMovieName =
                $"{movieInfo.title}{(!string.IsNullOrEmpty(movieInfo.release_date) ? $" ({DateTime.Parse(movieInfo.release_date).Year})" : string.Empty)}";

            var existingRequest = await RequestService.CheckRequestAsync(model.Id);
            if (existingRequest != null)
            {
                // check if the current user is already marked as a requester for this movie, if not, add them
                //if (!existingRequest.UserHasRequested(Username))
                //{
                //    existingRequest.RequestedUsers.Add(Username);
                //    await RequestService.UpdateRequestAsync(existingRequest);
                //}

                return new RequestEngineResult
                {
                    RequestAdded = true,

                };
                //Response.AsJson(new JsonResponseModel
                //{
                //    Result = true,
                //    Message =
                //        Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests)
                //            ? $"{fullMovieName} {Ombi.UI.Resources.UI.Search_SuccessfullyAdded}"
                //            : $"{fullMovieName} {Resources.UI.Search_AlreadyRequested}"
                //});
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
                ProviderId = movieInfo.id,
                Type = RequestType.Movie,
                Overview = movieInfo.overview,
                ImdbId = movieInfo.imdb_id,
                PosterPath = movieInfo.poster_path,
                Title = movieInfo.title,
                ReleaseDate = !string.IsNullOrEmpty(movieInfo.release_date) ? DateTime.Parse(movieInfo.release_date) : DateTime.MinValue,
                Status = movieInfo.status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                //RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,
            };

            try
            {
                if (ShouldAutoApprove(RequestType.Movie))
                {
                    //    model.Approved = true;

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

                //await NotificationService.Publish(new NotificationModel
                //{
                //    DateTime = DateTime.Now,
                //    User = Username,
                //    RequestType = RequestType.Movie,
                //    Title = model.Title,
                //    NotificationType = NotificationType.ItemAddedToFaultQueue
                //});

                //return Response.AsJson(new JsonResponseModel
                //{
                //    Result = true,
                //    Message = $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}"
                //});
            }

            return null;
        }

        public bool ShouldAutoApprove(RequestType requestType)
        {
            //var admin = Security.HasPermissions(Context.CurrentUser, Permissions.Administrator);
            //// if the user is an admin, they go ahead and allow auto-approval
            //if (admin) return true;

            //// check by request type if the category requires approval or not
            //switch (requestType)
            //{
            //    case RequestType.Movie:
            //        return Security.HasPermissions(User, Permissions.AutoApproveMovie);
            //    case RequestType.TvShow:
            //        return Security.HasPermissions(User, Permissions.AutoApproveTv);
            //    case RequestType.Album:
            //        return Security.HasPermissions(User, Permissions.AutoApproveAlbum);
            //    default:
            //        return false;
            return false;
        }

        private async Task<RequestEngineResult> AddRequest(RequestModel model, /*PlexRequestSettings settings,*/ string message)
        {
            await RequestService.AddRequestAsync(model);

            //if (ShouldSendNotification(model.Type, settings))
            //{
            //    var notificationModel = new NotificationModel
            //    {
            //        Title = model.Title,
            //        User = Username,
            //        DateTime = DateTime.Now,
            //        NotificationType = NotificationType.NewRequest,
            //        RequestType = model.Type,
            //        ImgSrc = model.Type == RequestType.Movie ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}" : model.PosterPath
            //    };
            //    await NotificationService.Publish(notificationModel);
            //}

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

        public async Task<IEnumerable<RequestViewModel>> GetRequests()
        {
            var allRequests = await RequestService.GetAllAsync();
            var viewModel = MapToVm(allRequests);
            return viewModel;
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


        private IEnumerable<RequestViewModel> MapToVm(IEnumerable<RequestModel> model)
        {
            return model.Select(movie => new RequestViewModel
            {
                ProviderId = movie.ProviderId,
                Type = movie.Type,
                Status = movie.Status,
                ImdbId = movie.ImdbId,
                Id = movie.Id,
                PosterPath = movie.PosterPath,
                ReleaseDate = movie.ReleaseDate,
                RequestedDate = movie.RequestedDate,
                Released = DateTime.Now > movie.ReleaseDate,
                Approved = movie.Available || movie.Approved,
                Title = movie.Title,
                Overview = movie.Overview,
                RequestedUsers = movie.AllUsers.ToArray(),
                ReleaseYear = movie.ReleaseDate.Year.ToString(),
                Available = movie.Available,
                Admin = false,
                IssueId = movie.IssueId,
                Denied = movie.Denied,
                DeniedReason = movie.DeniedReason,
                //Qualities = qualities.ToArray(),
                //HasRootFolders = rootFolders.Any(),
                //RootFolders = rootFolders.ToArray(),
                //CurrentRootPath = radarr.Enabled ? GetRootPath(movie.RootFolderSelected, radarr).Result : null
            }).ToList();
        }
    }
}