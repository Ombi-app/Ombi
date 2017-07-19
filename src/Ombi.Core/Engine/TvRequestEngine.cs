using AutoMapper;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Rule;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IMapper map,
            IRuleEvaluator rule, UserManager<OmbiUser> manager,
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
        private UserManager<OmbiUser> UserManager { get; }
        private ITvSender TvSender {get;}

        public async Task<RequestEngineResult> RequestTvShow(SearchTvShowViewModel tv)
        {
            var user = await UserManager.GetUserAsync(new ClaimsPrincipal(User));

            var tvBuilder = new TvShowRequestBuilder(TvApi);
            (await tvBuilder
                .GetShowInfo(tv.Id))
                .CreateTvList(tv)
                .CreateChild(tv, user.Id);

            await tvBuilder.BuildEpisodes(tv);
           
            var ruleResults = await RunRequestRules(tvBuilder.ChildRequest);
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
                foreach (var existingSeason in existingRequest.ChildRequests)
                    foreach (var existing in existingSeason.SeasonRequests)
                    {
                        var newChild = tvBuilder.ChildRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == existing.SeasonNumber);
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
                                tvBuilder.ChildRequest.SeasonRequests.Remove(newChild);
                            }
                        }
                    }

                // Remove the ID since this is a new child
                tvBuilder.ChildRequest.Id = 0;
                return await AddExistingRequest(tvBuilder.ChildRequest, existingRequest);
            }
            // This is a new request

            var newRequest = tvBuilder.CreateNewRequest(tv);
            return await AddRequest(newRequest.NewRequest);
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

        public async Task<IEnumerable<ChildRequests>> GetAllChldren(int tvId)
        {
            return await TvRepository.GetChild().Where(x => x.ParentRequestId == tvId).ToListAsync();
        }
        
        public async Task<IEnumerable<TvRequests>> SearchTvRequest(string search)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase)).ToListAsync();
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

            return await AfterRequest(newRequest);
        }

        private async Task<RequestEngineResult> AddRequest(TvRequests model)
        {
            await TvRepository.Add(model);
            // This is a new request so we should only have 1 child
            return await AfterRequest(model.ChildRequests.FirstOrDefault());
        }

        private async Task<RequestEngineResult> AfterRequest(ChildRequests model)
        {
            var sendRuleResult = await RunSpecificRule(model, SpecificRules.CanSendNotification);
            if (sendRuleResult.Success)
            {
                NotificationHelper.NewRequest(model);
            }

            if(model.Approved)
            {
                // Autosend
                await TvSender.SendToSonarr(model);
            }

            return new RequestEngineResult { RequestAdded = true };
        }

        //public async Task<IEnumerable<TvRequests>> GetApprovedRequests()
        //{
        //    var allRequests = TvRepository.Get();
            
            
        //}

        //public async Task<IEnumerable<TvRequests>> GetNewRequests()
        //{
        //    //var allRequests = await TvRepository.GetAllAsync();
        //    //return allRequests.Where(x => !x.Approved && !x.Available);
        //    return null;
        //}

        //public async Task<IEnumerable<TvRequests>> GetAvailableRequests()
        //{
        //    //var allRequests = await TvRepository.GetAllAsync();
        //    //return allRequests.Where(x => x.Available);
        //    return null;
        //}
    }
}