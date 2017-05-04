using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Hangfire;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IRequestServiceMain requestService, IPrincipal user, INotificationService notificationService) : base(user, requestService)
        {
            TvApi = tvApi;
            NotificationService = notificationService;
        }
        private INotificationService NotificationService { get; }
        private ITvMazeApi TvApi { get; }

        public async Task<RequestEngineResult> RequestTvShow(SearchTvShowViewModel tv)
        {

            var showInfo = await TvApi.ShowLookupByTheTvDbId(tv.Id);
            DateTime.TryParse(showInfo.premiered, out DateTime firstAir);
            
            // For some reason the poster path is always http
            var posterPath = showInfo.image?.medium.Replace("http:", "https:");
            var model = new TvRequestModel
            {
                Id = tv.Id,
                Type = RequestType.TvShow,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = posterPath,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,
                ImdbId = showInfo.externals?.imdb ?? string.Empty,
                TvDbId = tv.Id.ToString(),
                ProviderId = tv.Id,
                SeasonsNumbersRequested = tv.SeasonNumbersRequested,
                RequestAll = tv.RequestAll
            };


            var existingRequest = await TvRequestService.CheckRequestAsync(model.Id);
            if (existingRequest != null)
            {
                return await AddExistingRequest(model, existingRequest);
            }

            // This is a new request
            return await AddRequest(model);
        }

        public async Task<IEnumerable<TvRequestModel>> GetTvRequests(int count, int position)
        {
            var allRequests = await TvRequestService.GetAllAsync(count, position);
            return allRequests;
        }
        public async Task<IEnumerable<TvRequestModel>> SearchTvRequest(string search)
        {
            var allRequests = await TvRequestService.GetAllAsync();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            return results;
        }
        public async Task<TvRequestModel> UpdateTvRequest(TvRequestModel request)
        {
            var allRequests = await TvRequestService.GetAllAsync();
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

            var model = TvRequestService.UpdateRequest(results);
            return model;
        }

        public async Task RemoveTvRequest(int requestId)
        {
            await TvRequestService.DeleteRequestAsync(requestId);
        }

        private async Task<RequestEngineResult> AddExistingRequest(TvRequestModel newRequest, TvRequestModel existingRequest)
        {
            var episodeDifference = new List<EpisodesModel>();
            if (existingRequest.HasChildRequests)
            {
                // Let's check if this has already been requested as a child!
                foreach (var children in existingRequest.ChildRequests)
                {
                    var difference = GetListDifferences(children.Episodes, newRequest.Episodes).ToList();
                    if (difference.Any())
                    {
                        episodeDifference = difference;
                    }
                }
            }

            if (episodeDifference.Any())
            {
                // This is where there are some episodes that have been requested, but this list contains the 'new' requests
                newRequest.Episodes = episodeDifference;
            }

            existingRequest.ChildRequests.Add(newRequest);

            TvRequestService.UpdateRequest(existingRequest);

            if (ShouldAutoApprove(RequestType.TvShow))
            {
                // TODO Auto Approval Code
            }
            return await AddRequest(newRequest);
        }

        private IEnumerable<EpisodesModel> GetListDifferences(IEnumerable<EpisodesModel> existing, IEnumerable<EpisodesModel> request)
        {
            var newRequest = request
                .Select(r =>
                    new EpisodesModel
                    {
                        SeasonNumber = r.SeasonNumber,
                        EpisodeNumber = r.EpisodeNumber
                    }).ToList();

            return newRequest.Except(existing);
        }

        private async Task<RequestEngineResult> AddRequest(TvRequestModel model)
        {
            await TvRequestService.AddRequestAsync(model);

            if (ShouldSendNotification(model.Type))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type,
                    ImgSrc = model.PosterPath
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

            return new RequestEngineResult { RequestAdded = true };
        }


    }
}