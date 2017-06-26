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
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models.Requests.Tv;
using Ombi.Core.Rule;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IMapper map,
            IRuleEvaluator rule, IUserIdentityManager manager) : base(user, requestService, rule)
        {
            TvApi = tvApi;
            NotificationHelper = helper;
            Mapper = map;
            UserManager = manager;
        }

        private INotificationHelper NotificationHelper { get; }
        private ITvMazeApi TvApi { get; }
        private IMapper Mapper { get; }
        private IUserIdentityManager UserManager { get; }

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

            var user = await UserManager.GetUser(User.Identity.Name);
            var childRequest = new ChildRequests
            {
                Id = tv.Id,
                RequestType = RequestType.TvShow,
                //Overview = showInfo.summary.RemoveHtml(),
                //PosterPath = posterPath,
                //Title = showInfo.name,
                //ReleaseDate = firstAir,
                //Status = showInfo.status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUserId = user.Id,
                SeasonRequests = new List<SeasonRequests>()
            };

            if (tv.LatestSeason)
            {
                var episodes = await TvApi.EpisodeLookup(showInfo.id);
                var latest = episodes.OrderBy(x => x.season).FirstOrDefault();
                var episodesRequests = new List<EpisodeRequests>();
                foreach (var ep in episodes)
                {
                    episodesRequests.Add(new EpisodeRequests
                    {
                        EpisodeNumber = ep.number,
                        AirDate = DateTime.Parse(ep.airdate),
                        Title = ep.name,
                        Url = ep.url
                    });
                }
                childRequest.SeasonRequests.Add(new SeasonRequests
                {
                    Episodes = episodesRequests,
                    SeasonNumber = latest.season,
                });
            }
            
            if (tv.FirstSeason)
            {
                var episodes = await TvApi.EpisodeLookup(showInfo.id);
                var first = episodes.OrderByDescending(x => x.season).FirstOrDefault();
                var episodesRequests = new List<EpisodeRequests>();
                foreach (var ep in episodes)
                {
                    if (ep.season == first.season)
                    {
                        episodesRequests.Add(new EpisodeRequests
                        {
                            EpisodeNumber = ep.number,
                            AirDate = DateTime.Parse(ep.airdate),
                            Title = ep.name,
                            Url = ep.url
                        });
                    }
                }
                childRequest.SeasonRequests.Add(new SeasonRequests
                {
                    Episodes = episodesRequests,
                    SeasonNumber = first.season,
                });
            }

            var ruleResults = await RunRequestRules(childRequest);
            var results = ruleResults as RuleResult[] ?? ruleResults.ToArray();
            if (results.Any(x => !x.Success))
            {
                return new RequestEngineResult
                {
                    ErrorMessage = results.FirstOrDefault(x => !string.IsNullOrEmpty(x.Message)).Message
                };
            }

            var existingRequest = await TvRepository.Get().FirstOrDefaultAsync(x => x.TvDbId == tv.Id);
            if (existingRequest != null)
            {
                return await AddExistingRequest(childRequest, existingRequest);
            }
            // This is a new request

            var model = new TvRequests
            {
                Id = tv.Id,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = posterPath,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                ImdbId = showInfo.externals?.imdb ?? string.Empty,
                TvDbId = tv.Id,
                ChildRequests = new List<ChildRequests>()
            };
            model.ChildRequests.Add(childRequest);
            return await AddRequest(model);
        }

        public async Task<IEnumerable<TvRequests>> GetRequests(int count, int position)
        {
            var allRequests = await TvRepository.Get().Skip(position).Take(count).ToListAsync();
            return allRequests;
        }

        public async Task<IEnumerable<TvRequests>> GetRequests()
        {
            var allRequests = TvRepository.Get();
            return await allRequests.ToListAsync();
        }

        public async Task<IEnumerable<TvRequests>> SearchTvRequest(string search)
        {
            var allRequests = TvRepository.Get();
            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase));
            return results;
        }

        public async Task<TvRequests> UpdateTvRequest(TvRequests request)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.FirstOrDefaultAsync(x => x.Id == request.Id);
            results = Mapper.Map<TvRequests>(request);

            // TODO need to check if we need to approve any child requests since they may have updated
            
            await TvRepository.Update(results);
            return results;
        }

        public async Task RemoveTvRequest(int requestId)
        {
            var request = await TvRepository.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            await TvRepository.Delete(request);
        }

        private async Task<RequestEngineResult> AddExistingRequest(ChildRequests newRequest, TvRequests existingRequest)
        {
            // Add the child
            existingRequest.ChildRequests.Add(newRequest);

            await TvRepository.Update(existingRequest);

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

        private async Task<RequestEngineResult> AddRequest(TvRequests model)
        {
            await TvRepository.Add(model);
            // This is a new request so we should only have 1 child
            return await AfterRequest(model.ChildRequests.FirstOrDefault());
        }

        private Task<RequestEngineResult> AfterRequest(ChildRequests model)
        {
            if (ShouldSendNotification(RequestType.TvShow))
            {
                //NotificationHelper.NewRequest(model.ParentRequest);
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

            return Task.FromResult(new RequestEngineResult { RequestAdded = true });
        }

        public async Task<IEnumerable<TvRequests>> GetApprovedRequests()
        {
            //var allRequests = TvRepository.Get();
            //return await allRequests.Where(x => x.Approved && !x.Available).ToListAsync();
            return null;
        }

        public async Task<IEnumerable<TvRequests>> GetNewRequests()
        {
            //var allRequests = await TvRepository.GetAllAsync();
            //return allRequests.Where(x => !x.Approved && !x.Available);
            return null;
        }

        public async Task<IEnumerable<TvRequests>> GetAvailableRequests()
        {
            //var allRequests = await TvRepository.GetAllAsync();
            //return allRequests.Where(x => x.Available);
            return null;
        }
    }
}