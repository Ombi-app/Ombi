using AutoMapper;
using Hangfire;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
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
using Ombi.Core.Engine.Interfaces;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationService notificationService, IMapper map,
            IRuleEvaluator rule) : base(user, requestService, rule)
        {
            TvApi = tvApi;
            NotificationService = notificationService;
            Mapper = map;
        }

        private INotificationService NotificationService { get; }
        private ITvMazeApi TvApi { get; }
        private IMapper Mapper { get; }

        public async Task<RequestEngineResult> RequestTvShow(SearchTvShowViewModel tv)
        {
            var showInfo = await TvApi.ShowLookupByTheTvDbId(tv.Id);
            DateTime.TryParse(showInfo.premiered, out DateTime firstAir);

            // For some reason the poster path is always http
            var posterPath = showInfo.image?.medium.Replace("http:", "https:");

            var tvRequests = new List<SeasonRequestModel>();
            // Only have the TV requests we actually requested and not everything
            foreach (var season in tv.SeasonRequests)
            {
                for (int i = season.Episodes.Count - 1; i >= 0; i--)
                {
                    if (!season.Episodes[i].Requested)
                    {
                        season.Episodes.RemoveAt(i); // Remove the episode since it's not requested
                    }
                }

                if (season.Episodes.Any())
                {
                    tvRequests.Add(season);
                }
            }

            
            var childRequest = new ChildTvRequest
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
                RequestedUser =  Username,
                Issues = IssueState.None,
                ProviderId = tv.Id,
                RequestAll = tv.RequestAll,
                SeasonRequests = tvRequests
            };

            var model = new TvRequestModel
            {
                Id = tv.Id,
                Type = RequestType.TvShow,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = posterPath,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                Approved = false,
                ImdbId = showInfo.externals?.imdb ?? string.Empty,
                TvDbId = tv.Id.ToString(),
                ProviderId = tv.Id
            };

            model.ChildRequests.Add(childRequest);

            //if (childRequest.SeasonRequests.Any())
            //{
            //    var episodes = await TvApi.EpisodeLookup(showInfo.id);

            //    foreach (var e in episodes)
            //    {
            //        var season = childRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == e.season);
            //        season?.Episodes.Add(new EpisodesRequested
            //        {
            //            Url = e.url,
            //            Title = e.name,
            //            AirDate = DateTime.Parse(e.airstamp),
            //            EpisodeNumber = e.number
            //        });
            //    }
            //}

            if (tv.LatestSeason)
            {
                var latest = showInfo.Season.OrderBy(x => x.SeasonNumber).FirstOrDefault();
                foreach (var modelSeasonRequest in childRequest.SeasonRequests)
                {
                    if (modelSeasonRequest.SeasonNumber == latest.SeasonNumber)
                    {
                        foreach (var episodesRequested in modelSeasonRequest.Episodes)
                        {
                            episodesRequested.Requested = true;
                        }
                    }
                }
            }
            if (tv.FirstSeason)
            {
                var first = showInfo.Season.OrderByDescending(x => x.SeasonNumber).FirstOrDefault();
                foreach (var modelSeasonRequest in childRequest.SeasonRequests)
                {
                    if (modelSeasonRequest.SeasonNumber == first.SeasonNumber)
                    {
                        foreach (var episodesRequested in modelSeasonRequest.Episodes)
                        {
                            episodesRequested.Requested = true;
                        }
                    }
                }
            }

            var ruleResults = RunRequestRules(model).ToList();
            if (ruleResults.Any(x => !x.Success))
            {
                return new RequestEngineResult
                {
                    ErrorMessage = ruleResults.FirstOrDefault(x => !string.IsNullOrEmpty(x.Message)).Message
                };
            }

            var existingRequest = await TvRequestService.CheckRequestAsync(model.Id);
            if (existingRequest != null)
            {
                return await AddExistingRequest(model, existingRequest);
            }
            // This is a new request
            return await AddRequest(model);
        }

        public async Task<IEnumerable<TvRequestModel>> GetRequests(int count, int position)
        {
            var allRequests = await TvRequestService.GetAllAsync(count, position);
            return allRequests;
        }

        public async Task<IEnumerable<TvRequestModel>> GetRequests()
        {
            var allRequests = await TvRequestService.GetAllAsync();
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
            results = Mapper.Map<TvRequestModel>(request);

            // TODO need to check if we need to approve any child requests since they may have updated
            
            var model = TvRequestService.UpdateRequest(results);
            return model;
        }

        public async Task RemoveTvRequest(int requestId)
        {
            await TvRequestService.DeleteRequestAsync(requestId);
        }

        private async Task<RequestEngineResult> AddExistingRequest(TvRequestModel newRequest,
            TvRequestModel existingRequest)
        {
            var child = newRequest.ChildRequests.FirstOrDefault(); // There will only be 1
            var episodeDiff = new List<SeasonRequestModel>();
            foreach (var existingChild in existingRequest.ChildRequests)
            {
                var difference = GetListDifferences(existingChild.SeasonRequests, child.SeasonRequests).ToList();
                if (difference.Any())
                    episodeDiff = difference;
            }

            if (episodeDiff.Any())
                child.SeasonRequests = episodeDiff;

            existingRequest.ChildRequests.AddRange(newRequest.ChildRequests);
            TvRequestService.UpdateRequest(existingRequest);

            if (newRequest.Approved) // The auto approve rule
            {
                // TODO Auto Approval Code
            }
            return await AfterRequest(newRequest);
        }

        private IEnumerable<SeasonRequestModel> GetListDifferences(List<SeasonRequestModel> existing,
            List<SeasonRequestModel> request)
        {
            var requestsToRemove = new List<SeasonRequestModel>();
            foreach (var r in request)
            {
                // Do we have an existing season?
                var existingSeason = existing.FirstOrDefault(x => x.SeasonNumber == r.SeasonNumber);
                if (existingSeason == null)
                {
                    continue;
                }

                // Compare the episodes
                for (var i = r.Episodes.Count - 1; i >= 0; i--)
                {
                    var existingEpisode = existingSeason.Episodes.FirstOrDefault(x => x.EpisodeNumber == r.Episodes[i].EpisodeNumber);
                    if (existingEpisode == null)
                    {
                        // we are fine, we have not yet requested this
                    }
                    else
                    {
                        // We already have this request
                        r.Episodes.RemoveAt(i);
                    }
                }

                if (!r.Episodes.Any())
                {
                    requestsToRemove.Add(r);
                }
            }

            foreach (var remove in requestsToRemove)
            {
                request.Remove(remove);
            }
            return request;
        }

        private async Task<RequestEngineResult> AddRequest(TvRequestModel model)
        {
            await TvRequestService.AddRequestAsync(model);

            return await AfterRequest(model);
        }

        private async Task<RequestEngineResult> AfterRequest(TvRequestModel model)
        {
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

        public async Task<IEnumerable<TvRequestModel>> GetApprovedRequests()
        {
            var allRequests = await TvRequestService.GetAllAsync();
            return allRequests.Where(x => x.Approved && !x.Available);
        }

        public async Task<IEnumerable<TvRequestModel>> GetNewRequests()
        {
            var allRequests = await TvRequestService.GetAllAsync();
            return allRequests.Where(x => !x.Approved && !x.Available);
        }

        public async Task<IEnumerable<TvRequestModel>> GetAvailableRequests()
        {
            var allRequests = await TvRequestService.GetAllAsync();
            return allRequests.Where(x => x.Available);
        }
    }
}