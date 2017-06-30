using AutoMapper;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rules;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Rule;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IMapper map,
            IRuleEvaluator rule, IUserIdentityManager manager,
            ITvSender sender) : base(user, requestService, rule)
        {
            TvApi = tvApi;
            NotificationHelper = helper;
            Mapper = map;
            UserManager = manager;
            TvSender = sender;
        }

        private INotificationHelper NotificationHelper { get; }
        private ITvMazeApi TvApi { get; }
        private IMapper Mapper { get; }
        private IUserIdentityManager UserManager { get; }
        private ITvSender TvSender {get;}

        public async Task<RequestEngineResult> RequestTvShow(SearchTvShowViewModel tv)
        {
            var showInfo = await TvApi.ShowLookupByTheTvDbId(tv.Id);
            DateTime.TryParse(showInfo.premiered, out DateTime firstAir);

            // For some reason the poster path is always http
            var posterPath = showInfo.image?.medium.Replace("http:", "https:");

            var tvRequests = new List<SeasonRequests>();
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

            if (tv.RequestAll)
            {
                var episodes = await TvApi.EpisodeLookup(showInfo.id);
                var seasonRequests = new List<SeasonRequests>();
                foreach (var ep in episodes)
                {
                    var episodesRequests = new List<EpisodeRequests>();
                    var season = childRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == ep.season);
                    if (season == null)
                    {
                        childRequest.SeasonRequests.Add(new SeasonRequests
                        {
                            Episodes = new List<EpisodeRequests>{
                                new EpisodeRequests
                                {
                                    EpisodeNumber = ep.number,
                                    AirDate = DateTime.Parse(ep.airdate),
                                    Title = ep.name,
                                    Url = ep.url
                                }
                            },
                            SeasonNumber = ep.season,
                        });
                    }
                    else
                    {
                        season.Episodes.Add(new EpisodeRequests
                        {
                            EpisodeNumber = ep.number,
                            AirDate = DateTime.Parse(ep.airdate),
                            Title = ep.name,
                            Url = ep.url
                        });
                    }
                }

            }
            else if (tv.LatestSeason)
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
            else if (tv.FirstSeason)
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
            else
            {
                // It's a custom request
                childRequest.SeasonRequests = tvRequests;
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
                // Remove requests we already have, we just want new ones
                var existingSeasons = existingRequest.ChildRequests.Select(x => x.SeasonRequests);
                foreach (var existingSeason in existingRequest.ChildRequests)
                    foreach (var existing in existingSeason.SeasonRequests)
                    {
                        var newChild = childRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == existing.SeasonNumber);
                        if (newChild != null)
                        {
                            // We have some requests in this season...
                            // Let's find the episodes.
                            foreach (var existingEp in existing.Episodes)
                            {
                                var duplicateEpisode = newChild.Episodes.FirstOrDefault(x => x.EpisodeNumber == existingEp.EpisodeNumber);
                                if (duplicateEpisode != null)
                                {
                                    // Remove it.
                                    newChild.Episodes.Remove(duplicateEpisode);
                                }
                            }
                            if (!newChild.Episodes.Any())
                            {
                                // We may have removed all episodes
                                childRequest.SeasonRequests.Remove(newChild);
                            }
                        }
                    }

                // Remove the ID since this is a new child
                childRequest.Id = 0;
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
                ChildRequests = new List<ChildRequests>(),
                TotalSeasons = tv.SeasonRequests.Count()
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

            if(model.Approved)
            {
                // Autosend
                TvSender.SendToSonarr(model,model.ParentRequest.TotalSeasons);
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