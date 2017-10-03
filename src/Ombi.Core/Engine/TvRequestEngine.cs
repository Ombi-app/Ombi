﻿using AutoMapper;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Core.Rule;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IMapper map,
            IRuleEvaluator rule, OmbiUserManager manager,
            ITvSender sender, IAuditRepository audit) : base(user, requestService, rule, manager)
        {
            TvApi = tvApi;
            NotificationHelper = helper;
            Mapper = map;
            TvSender = sender;
            Audit = audit;
        }

        private INotificationHelper NotificationHelper { get; }
        private ITvMazeApi TvApi { get; }
        private IMapper Mapper { get; }
        private ITvSender TvSender { get; }
        private IAuditRepository Audit { get; }

        public async Task<RequestEngineResult> RequestTvShow(SearchTvShowViewModel tv)
        {
            var user = await GetUser();

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

            // Check if we have auto approved the request, if we have then mark the episodes as approved
            if (tvBuilder.ChildRequest.Approved)
            {
                foreach (var seasons in tvBuilder.ChildRequest.SeasonRequests)
                {
                    foreach (var ep in seasons.Episodes)
                    {
                        ep.Approved = true;
                        ep.Requested = true;
                    }
                }
            }

            await Audit.Record(AuditType.Added, AuditArea.TvRequest, $"Added Request {tv.Title}", Username);

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
                if (!tvBuilder.ChildRequest.SeasonRequests.Any())
                {
                    // Looks like we have removed them all! They were all duplicates...
                    return new RequestEngineResult
                    {
                        RequestAdded = false,
                        ErrorMessage = "They have already been requestsed"
                    };
                }
                    return await AddExistingRequest(tvBuilder.ChildRequest, existingRequest);
            }

            // This is a new request
            var newRequest = tvBuilder.CreateNewRequest(tv);
            return await AddRequest(newRequest.NewRequest);
        }

        public async Task<IEnumerable<TvRequests>> GetRequests(int count, int position)
        {
            var allRequests = await TvRepository.Get()
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.SeasonRequests)
                    .ThenInclude(x => x.Episodes)
                .Skip(position).Take(count).ToListAsync();
            return allRequests;
        }

        public async Task<IEnumerable<TreeNode<TvRequests, List<ChildRequests>>>> GetRequestsTreeNode(int count, int position)
        {
            var allRequests = await TvRepository.Get()
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.SeasonRequests)
                    .ThenInclude(x=>x.Episodes)
                .Skip(position).Take(count).ToListAsync();
            return ParseIntoTreeNode(allRequests);
        }

        public async Task<IEnumerable<TvRequests>> GetRequests()
        {
            var allRequests = TvRepository.Get();
            return await allRequests.ToListAsync();
        }

        public async Task<IEnumerable<ChildRequests>> GetAllChldren(int tvId)
        {
            return await TvRepository.GetChild().Include(x => x.SeasonRequests).Where(x => x.ParentRequestId == tvId).ToListAsync();
        }

        public async Task<IEnumerable<TvRequests>> SearchTvRequest(string search)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase)).ToListAsync();
            return results;
        }

        public async Task<IEnumerable<TreeNode<TvRequests, List<ChildRequests>>>> SearchTvRequestTree(string search)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase)).ToListAsync();
            return ParseIntoTreeNode(results);
        }

        public async Task<TvRequests> UpdateTvRequest(TvRequests request)
        {
            await Audit.Record(AuditType.Updated, AuditArea.TvRequest, $"Updated Request {request.Title}", Username);
            var allRequests = TvRepository.Get();
            var results = await allRequests.FirstOrDefaultAsync(x => x.Id == request.Id);

            await TvRepository.Update(results);
            return results;
        }

        public async Task<RequestEngineResult> ApproveChildRequest(ChildRequests request)
        {
            if (request.Approved)
            {
                await Audit.Record(AuditType.Approved, AuditArea.TvRequest, $"Approved Request {request.Title}", Username);
                // Autosend
                await TvSender.SendToSonarr(request);
            }
            await TvRepository.UpdateChild(request);
            return new RequestEngineResult
            {
                RequestAdded = true
            };
        }

        public async Task<ChildRequests> UpdateChildRequest(ChildRequests request)
        {
            await Audit.Record(AuditType.Updated, AuditArea.TvRequest, $"Updated Request {request.Title}", Username);

            await TvRepository.UpdateChild(request);
            return request;
        }

        public async Task RemoveTvChild(int requestId)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);
            var all = TvRepository.Db.TvRequests.Include(x => x.ChildRequests);
            var parent = all.FirstOrDefault(x => x.Id == request.ParentRequestId);

            // Is this the only child? If so delete the parent
            if (parent.ChildRequests.Count <= 1)
            {
                // Delete the parent
                TvRepository.Db.TvRequests.Remove(parent);
            }
            await Audit.Record(AuditType.Deleted, AuditArea.TvRequest, $"Deleting Request {request.Title}", Username);

            TvRepository.Db.ChildRequests.Remove(request);
            await TvRepository.Db.SaveChangesAsync();
        }

        public async Task RemoveTvRequest(int requestId)
        {
            var request = await TvRepository.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            await Audit.Record(AuditType.Deleted, AuditArea.TvRequest, $"Deleting Request {request.Title}", Username);
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

        private static List<TreeNode<TvRequests, List<ChildRequests>>> ParseIntoTreeNode(IEnumerable<TvRequests> result)
        {
            var node = new List<TreeNode<TvRequests, List<ChildRequests>>>();

            foreach (var value in result)
            {
                node.Add(new TreeNode<TvRequests, List<ChildRequests>>
                {
                    Data = value,
                    Children = new List<TreeNode<List<ChildRequests>>>
                    {
                        new TreeNode<List<ChildRequests>>
                        {
                            Data = SortEpisodes(value.ChildRequests),
                            Leaf = true
                        }
                    }
                });
            }
            return node;
        }

        private static List<ChildRequests> SortEpisodes(List<ChildRequests> items)
        {
            foreach (var value in items)
            {
                foreach (var requests in value.SeasonRequests)
                {
                    requests.Episodes.OrderBy(x => x.EpisodeNumber);
                }
            }
            return items;
        }


        private async Task<RequestEngineResult> AfterRequest(ChildRequests model)
        {
            var sendRuleResult = await RunSpecificRule(model, SpecificRules.CanSendNotification);
            if (sendRuleResult.Success)
            {
                NotificationHelper.NewRequest(model);
            }

            if (model.Approved)
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