using System;
using AutoMapper;
using Ombi.Api.TvMaze;
using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Core.Models.UI;
using Ombi.Core.Rule;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Senders;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Core.Models;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IMovieDbApi movApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IRuleEvaluator rule, OmbiUserManager manager,
            ITvSender sender, IRepository<RequestLog> rl, ISettingsService<OmbiSettings> settings, ICacheService cache,
            IRepository<RequestSubscription> sub) : base(user, requestService, rule, manager, cache, settings, sub)
        {
            TvApi = tvApi;
            MovieDbApi = movApi;
            NotificationHelper = helper;
            TvSender = sender;
            _requestLog = rl;
        }

        private INotificationHelper NotificationHelper { get; }
        private ITvMazeApi TvApi { get; }
        private IMovieDbApi MovieDbApi { get; }
        private ITvSender TvSender { get; }
        private readonly IRepository<RequestLog> _requestLog;

        public async Task<RequestEngineResult> RequestTvShow(TvRequestViewModel tv)
        {
            var user = await GetUser();
            var canRequestOnBehalf = false;

            if (tv.RequestOnBehalf.HasValue())
            {
                canRequestOnBehalf = await UserManager.IsInRoleAsync(user, OmbiRoles.PowerUser) || await UserManager.IsInRoleAsync(user, OmbiRoles.Admin);

                if (!canRequestOnBehalf)
                {
                    return new RequestEngineResult
                    {
                        Result = false,
                        Message = "You do not have the correct permissions to request on behalf of users!",
                        ErrorMessage = $"You do not have the correct permissions to request on behalf of users!"
                    };
                }
            }

            var tvBuilder = new TvShowRequestBuilder(TvApi, MovieDbApi);
            (await tvBuilder
                .GetShowInfo(tv.TvDbId))
                .CreateTvList(tv)
                .CreateChild(tv, canRequestOnBehalf ? tv.RequestOnBehalf : user.Id);

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

            var existingRequest = await TvRepository.Get().FirstOrDefaultAsync(x => x.TvDbId == tv.TvDbId);
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
                // This was a TVDBID for the request rules to run
                tvBuilder.ChildRequest.Id = 0;
                if (!tvBuilder.ChildRequest.SeasonRequests.Any())
                {
                    // Looks like we have removed them all! They were all duplicates...
                    return new RequestEngineResult
                    {
                        Result = false,
                        ErrorMessage = "This has already been requested"
                    };
                }
                return await AddExistingRequest(tvBuilder.ChildRequest, existingRequest, tv.RequestOnBehalf);
            }

            // This is a new request
            var newRequest = tvBuilder.CreateNewRequest(tv);
            return await AddRequest(newRequest.NewRequest, tv.RequestOnBehalf);
        }

        public async Task<RequestsViewModel<TvRequests>> GetRequests(int count, int position, OrderFilterModel type)
        {
            var shouldHide = await HideFromOtherUsers();
            List<TvRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.Get(shouldHide.UserId)
                    .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.SeasonRequests)
                    .ThenInclude(x => x.Episodes)
                    .OrderByDescending(x => x.ChildRequests.Select(y => y.RequestedDate).FirstOrDefault())
                    .Skip(position).Take(count).ToListAsync();

                // Filter out children

                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                allRequests = await TvRepository.Get()
                    .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.SeasonRequests)
                    .ThenInclude(x => x.Episodes)
                    .OrderByDescending(x => x.ChildRequests.Select(y => y.RequestedDate).FirstOrDefault())
                    .Skip(position).Take(count).ToListAsync();

            }
            await CheckForSubscription(shouldHide, allRequests);

            return new RequestsViewModel<TvRequests>
            {
                Collection = allRequests
            };
        }

        public async Task<RequestsViewModel<TvRequests>> GetRequestsLite(int count, int position, OrderFilterModel type)
        {
            var shouldHide = await HideFromOtherUsers();
            List<TvRequests> allRequests = null;
            if (shouldHide.Hide)
            {
                var tv = TvRepository.GetLite(shouldHide.UserId);
                if (tv.Any() && tv.Select(x => x.ChildRequests).Any())
                {
                    allRequests = await tv.OrderByDescending(x => x.ChildRequests.Select(y => y.RequestedDate).FirstOrDefault()).Skip(position).Take(count).ToListAsync();
                }

                // Filter out children
                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                var tv = TvRepository.GetLite();
                if (tv.Any() && tv.Select(x => x.ChildRequests).Any())
                {
                    allRequests = await tv.OrderByDescending(x => x.ChildRequests.Select(y => y.RequestedDate).FirstOrDefault()).Skip(position).Take(count).ToListAsync();
                }
            }
            if (allRequests == null)
            {
                return new RequestsViewModel<TvRequests>();
            }

            await CheckForSubscription(shouldHide, allRequests);

            return new RequestsViewModel<TvRequests>
            {
                Collection = allRequests
            };
        }
        public async Task<IEnumerable<TvRequests>> GetRequests()
        {
            var shouldHide = await HideFromOtherUsers();
            List<TvRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.Get(shouldHide.UserId).ToListAsync();

                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                allRequests = await TvRepository.Get().ToListAsync();
            }

            await CheckForSubscription(shouldHide, allRequests);
            return allRequests;
        }


        public async Task<RequestsViewModel<ChildRequests>> GetRequests(int count, int position, string sortProperty, string sortOrder)
        {
            var shouldHide = await HideFromOtherUsers();
            List<ChildRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.GetChild(shouldHide.UserId).ToListAsync();

                // Filter out children

                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                allRequests = await TvRepository.GetChild().ToListAsync();

            }

            if (allRequests == null)
            {
                return new RequestsViewModel<ChildRequests>();
            }

            var total = allRequests.Count;


            var prop = TypeDescriptor.GetProperties(typeof(ChildRequests)).Find(sortProperty, true);

            if (sortProperty.Contains('.'))
            {
                // This is a navigation property currently not supported
                prop = TypeDescriptor.GetProperties(typeof(ChildRequests)).Find("Title", true);
                //var properties = sortProperty.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
                //var firstProp = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find(properties[0], true);
                //var propType = firstProp.PropertyType;
                //var secondProp = TypeDescriptor.GetProperties(propType).Find(properties[1], true);
            }
            allRequests = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase)
                ? allRequests.OrderBy(x => prop.GetValue(x)).ToList()
                : allRequests.OrderByDescending(x => prop.GetValue(x)).ToList();
            
            await CheckForSubscription(shouldHide, allRequests);

            // Make sure we do not show duplicate child requests
            allRequests = allRequests.DistinctBy(x => x.ParentRequest.Title).ToList();

            allRequests = allRequests.Skip(position).Take(count).ToList();

            return new RequestsViewModel<ChildRequests>
            {
                Collection = allRequests,
                Total = total,
            };
        }

         public async Task<RequestsViewModel<ChildRequests>> GetRequests(int count, int position, string sortProperty, string sortOrder, RequestStatus status)
        {
            var shouldHide = await HideFromOtherUsers();
            List<ChildRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.GetChild(shouldHide.UserId).ToListAsync();

                // Filter out children

                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                allRequests = await TvRepository.GetChild().ToListAsync();

            }

            switch (status)
            {
                case RequestStatus.PendingApproval:
                    allRequests = allRequests.Where(x => !x.Approved && !x.Available && (!x.Denied.HasValue || !x.Denied.Value)).ToList();
                    break;
                case RequestStatus.ProcessingRequest:
                    allRequests = allRequests.Where(x => x.Approved && !x.Available && (!x.Denied.HasValue || !x.Denied.Value)).ToList();
                    break;
                case RequestStatus.Available:
                    allRequests = allRequests.Where(x => x.Available && (!x.Denied.HasValue || !x.Denied.Value)).ToList();
                    break;
                case RequestStatus.Denied:
                    allRequests = allRequests.Where(x => x.Denied.HasValue  && x.Denied.Value).ToList();
                    break;
                default:
                    break;
            }

            if (allRequests == null)
            {
                return new RequestsViewModel<ChildRequests>();
            }

            var total = allRequests.Count;


            var prop = TypeDescriptor.GetProperties(typeof(ChildRequests)).Find(sortProperty, true);

            if (sortProperty.Contains('.'))
            {
                // This is a navigation property currently not supported
                prop = TypeDescriptor.GetProperties(typeof(ChildRequests)).Find("Title", true);
                //var properties = sortProperty.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
                //var firstProp = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find(properties[0], true);
                //var propType = firstProp.PropertyType;
                //var secondProp = TypeDescriptor.GetProperties(propType).Find(properties[1], true);
            }
            allRequests = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase)
                ? allRequests.OrderBy(x => prop.GetValue(x)).ToList()
                : allRequests.OrderByDescending(x => prop.GetValue(x)).ToList();
            
            await CheckForSubscription(shouldHide, allRequests);

            // Make sure we do not show duplicate child requests
            allRequests = allRequests.DistinctBy(x => x.ParentRequest.Title).ToList();

            allRequests = allRequests.Skip(position).Take(count).ToList();

            return new RequestsViewModel<ChildRequests>
            {
                Collection = allRequests,
                Total = total,
            };
        }
        public async Task<RequestsViewModel<ChildRequests>> GetUnavailableRequests(int count, int position, string sortProperty, string sortOrder)
        {
            var shouldHide = await HideFromOtherUsers();
            List<ChildRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.GetChild(shouldHide.UserId).Where(x => !x.Available && x.Approved).ToListAsync();

                // Filter out children

                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                allRequests = await TvRepository.GetChild().Where(x => !x.Available && x.Approved).ToListAsync();

            }

            if (allRequests == null)
            {
                return new RequestsViewModel<ChildRequests>();
            }

            var total = allRequests.Count;


            var prop = TypeDescriptor.GetProperties(typeof(ChildRequests)).Find(sortProperty, true);

            if (sortProperty.Contains('.'))
            {
                // This is a navigation property currently not supported
                prop = TypeDescriptor.GetProperties(typeof(ChildRequests)).Find("Title", true);
                //var properties = sortProperty.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
                //var firstProp = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find(properties[0], true);
                //var propType = firstProp.PropertyType;
                //var secondProp = TypeDescriptor.GetProperties(propType).Find(properties[1], true);
            }
            allRequests = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase)
                ? allRequests.OrderBy(x => prop.GetValue(x)).ToList()
                : allRequests.OrderByDescending(x => prop.GetValue(x)).ToList();
            await CheckForSubscription(shouldHide, allRequests);

            // Make sure we do not show duplicate child requests
            allRequests = allRequests.DistinctBy(x => x.ParentRequest.Title).ToList();
            allRequests = allRequests.Skip(position).Take(count).ToList();

            return new RequestsViewModel<ChildRequests>
            {
                Collection = allRequests,
                Total = total,
            };
        }


        public async Task<IEnumerable<TvRequests>> GetRequestsLite()
        {
            var shouldHide = await HideFromOtherUsers();
            List<TvRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.GetLite(shouldHide.UserId).ToListAsync();

                FilterChildren(allRequests, shouldHide);
            }
            else
            {
                allRequests = await TvRepository.GetLite().ToListAsync();
            }

            await CheckForSubscription(shouldHide, allRequests);
            return allRequests;
        }

        public async Task<TvRequests> GetTvRequest(int requestId)
        {
            var shouldHide = await HideFromOtherUsers();
            TvRequests request;
            if (shouldHide.Hide)
            {
                request = await TvRepository.Get(shouldHide.UserId).Where(x => x.Id == requestId).FirstOrDefaultAsync();

                FilterChildren(request, shouldHide);
            }
            else
            {
                request = await TvRepository.Get().Where(x => x.Id == requestId).FirstOrDefaultAsync();
            }

            await CheckForSubscription(shouldHide, new List<TvRequests>{request});
            return request;
        }

        private static void FilterChildren(IEnumerable<TvRequests> allRequests, HideResult shouldHide)
        {
            if (allRequests == null)
            {
                return;
            }
            // Filter out children
            foreach (var t in allRequests)
            {
                for (var j = 0; j < t.ChildRequests.Count; j++)
                {
                    FilterChildren(t, shouldHide);
                }
            }
        }

        private static void FilterChildren(TvRequests t, HideResult shouldHide)
        {
            // Filter out children
            FilterChildren(t.ChildRequests, shouldHide);
        }

        private static void FilterChildren(List<ChildRequests> t, HideResult shouldHide)
        {
            // Filter out children

            for (var j = 0; j < t.Count; j++)
            {
                var child = t[j];
                if (child.RequestedUserId != shouldHide.UserId)
                {
                    t.RemoveAt(j);
                    j--;
                }
            }
        }

        public async Task<IEnumerable<ChildRequests>> GetAllChldren(int tvId)
        {
            var shouldHide = await HideFromOtherUsers();
            List<ChildRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await TvRepository.GetChild(shouldHide.UserId).Include(x => x.SeasonRequests).Where(x => x.ParentRequestId == tvId).ToListAsync();
            }
            else
            {
                allRequests = await TvRepository.GetChild().Include(x => x.SeasonRequests).Where(x => x.ParentRequestId == tvId).ToListAsync();
            }

            await CheckForSubscription(shouldHide, allRequests);

            return allRequests;
        }

        public async Task<IEnumerable<TvRequests>> SearchTvRequest(string search)
        {
            var shouldHide = await HideFromOtherUsers();
            IQueryable<TvRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = TvRepository.Get(shouldHide.UserId);
            }
            else
            {
                allRequests = TvRepository.Get();
            }
            var results = await allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase)).ToListAsync();

            await CheckForSubscription(shouldHide, results);
            return results;
        }

        public async Task UpdateRootPath(int requestId, int rootPath)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.FirstOrDefaultAsync(x => x.Id == requestId);
            results.RootFolder = rootPath;

            await TvRepository.Update(results);
        }

        public async Task UpdateQualityProfile(int requestId, int profileId)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.FirstOrDefaultAsync(x => x.Id == requestId);
            results.QualityOverride = profileId;

            await TvRepository.Update(results);
        }

        public async Task<TvRequests> UpdateTvRequest(TvRequests request)
        {
            var allRequests = TvRepository.Get();
            var results = await allRequests.FirstOrDefaultAsync(x => x.Id == request.Id);

            results.TvDbId = request.TvDbId;
            results.ImdbId = request.ImdbId;
            results.Overview = request.Overview;
            results.PosterPath = PosterPathHelper.FixPosterPath(request.PosterPath);
            results.Background = PosterPathHelper.FixBackgroundPath(request.Background);
            results.QualityOverride = request.QualityOverride;
            results.RootFolder = request.RootFolder;

            await TvRepository.Update(results);
            return results;
        }

        public async Task<RequestEngineResult> ApproveChildRequest(int id)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == id);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Child Request does not exist"
                };
            }
            request.Approved = true;
            request.Denied = false;

            foreach (var s in request.SeasonRequests)
            {
                foreach (var ep in s.Episodes)
                {
                    ep.Approved = true;
                    ep.Requested = true;
                }
            }

            await TvRepository.UpdateChild(request);

            if (request.Approved)
            {
                await NotificationHelper.Notify(request, NotificationType.RequestApproved);
                // Autosend
                await TvSender.Send(request);
            }
            return new RequestEngineResult
            {
                Result = true
            };
        }

        public async Task<RequestEngineResult> DenyChildRequest(int requestId, string reason)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Child Request does not exist"
                };
            }
            request.Denied = true;
            request.DeniedReason = reason;
            await TvRepository.UpdateChild(request);
            await NotificationHelper.Notify(request, NotificationType.RequestDeclined);
            return new RequestEngineResult
            {
                Result = true
            };
        }

        public async Task<ChildRequests> UpdateChildRequest(ChildRequests request)
        {
            await TvRepository.UpdateChild(request);
            return request;
        }

        public async Task RemoveTvChild(int requestId)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);

            TvRepository.Db.ChildRequests.Remove(request);
            var all = TvRepository.Db.TvRequests.Include(x => x.ChildRequests);
            var parent = all.FirstOrDefault(x => x.Id == request.ParentRequestId);

            // Is this the only child? If so delete the parent
            if (parent.ChildRequests.Count <= 1)
            {
                // Delete the parent
                TvRepository.Db.TvRequests.Remove(parent);
            }

            await TvRepository.Db.SaveChangesAsync();
        }

        public async Task RemoveTvRequest(int requestId)
        {
            var request = await TvRepository.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            await TvRepository.Delete(request);
        }

        public async Task<bool> UserHasRequest(string userId)
        {
            return await TvRepository.GetChild().AnyAsync(x => x.RequestedUserId == userId);
        }

        public async Task<RequestEngineResult> MarkUnavailable(int modelId)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Child Request does not exist"
                };
            }
            request.Available = false;
            foreach (var season in request.SeasonRequests)
            {
                foreach (var e in season.Episodes)
                {
                    e.Available = false;
                }
            }
            await TvRepository.UpdateChild(request);
            return new RequestEngineResult
            {
                Result = true,
                Message = "Request is now unavailable",
            };
        }

        public async Task<RequestEngineResult> MarkAvailable(int modelId)
        {
            ChildRequests request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Child Request does not exist"
                };
            }
            request.Available = true;
            request.MarkedAsAvailable = DateTime.Now;
            foreach (var season in request.SeasonRequests)
            {
                foreach (var e in season.Episodes)
                {
                    e.Available = true;
                }
            }
            await TvRepository.UpdateChild(request);
            await NotificationHelper.Notify(request, NotificationType.RequestAvailable);
            return new RequestEngineResult
            {
                Result = true,
                Message = "Request is now available",
            };
        }

        public async Task<int> GetTotal()
        {
            var shouldHide = await HideFromOtherUsers();
            if (shouldHide.Hide)
            {
                return await TvRepository.Get(shouldHide.UserId).CountAsync();
            }
            else
            {
                return await TvRepository.Get().CountAsync();
            }
        }

        private async Task CheckForSubscription(HideResult shouldHide, List<TvRequests> x)
        {
            foreach (var tvRequest in x)
            {
                await CheckForSubscription(shouldHide, tvRequest.ChildRequests);
            }
        }

        private async Task CheckForSubscription(HideResult shouldHide, List<ChildRequests> childRequests)
        {
            var sub = _subscriptionRepository.GetAll();
            var childIds = childRequests.Select(x => x.Id);
            var relevantSubs = await sub.Where(s =>
                s.UserId == shouldHide.UserId && childIds.Contains(s.Id) && s.RequestType == RequestType.TvShow).ToListAsync();
            foreach (var x in childRequests)
            {
                if (shouldHide.UserId == x.RequestedUserId)
                {
                    x.ShowSubscribe = false;
                }
                else
                {
                    x.ShowSubscribe = true;
                    var result = relevantSubs.FirstOrDefault(s => s.RequestId == x.Id);
                    x.Subscribed = result != null;
                }
            }
        }

        private async Task<RequestEngineResult> AddExistingRequest(ChildRequests newRequest, TvRequests existingRequest, string requestOnBehalf)
        {
            // Add the child
            existingRequest.ChildRequests.Add(newRequest);

            await TvRepository.Update(existingRequest);

            return await AfterRequest(newRequest, requestOnBehalf);
        }

        private async Task<RequestEngineResult> AddRequest(TvRequests model, string requestOnBehalf)
        {
            await TvRepository.Add(model);
            // This is a new request so we should only have 1 child
            return await AfterRequest(model.ChildRequests.FirstOrDefault(), requestOnBehalf);
        }

        private static List<ChildRequests> SortEpisodes(List<ChildRequests> items)
        {
            foreach (var value in items)
            {
                foreach (var requests in value.SeasonRequests)
                {
                    requests.Episodes = requests.Episodes.OrderBy(x => x.EpisodeNumber).ToList();
                }
            }
            return items;
        }


        private async Task<RequestEngineResult> AfterRequest(ChildRequests model, string requestOnBehalf)
        {
            var sendRuleResult = await RunSpecificRule(model, SpecificRules.CanSendNotification);
            if (sendRuleResult.Success)
            {
                await NotificationHelper.NewRequest(model);
            }

            await _requestLog.Add(new RequestLog
            {
                UserId = requestOnBehalf.HasValue() ? requestOnBehalf : (await GetUser()).Id,
                RequestDate = DateTime.UtcNow,
                RequestId = model.Id,
                RequestType = RequestType.TvShow,
                EpisodeCount = model.SeasonRequests.Select(m => m.Episodes.Count).Sum(),
            });

            if (model.Approved)
            {
                // Autosend
                await NotificationHelper.Notify(model, NotificationType.RequestApproved);
                var result = await TvSender.Send(model);
                if (result.Success)
                {
                    return new RequestEngineResult { Result = true, RequestId = model.Id };
                }
                return new RequestEngineResult
                {
                    ErrorMessage = result.Message,
                    RequestId = model.Id
                };
            }

            return new RequestEngineResult { Result = true, RequestId = model.Id };
        }

        public async Task<RequestQuotaCountModel> GetRemainingRequests(OmbiUser user)
        {
            if (user == null)
            {
                user = await GetUser();

                // If user is still null after attempting to get the logged in user, return null.
                if (user == null)
                {
                    return null;
                }
            }

            int limit = user.EpisodeRequestLimit ?? 0;

            if (limit <= 0)
            {
                return new RequestQuotaCountModel()
                {
                    HasLimit = false,
                    Limit = 0,
                    Remaining = 0,
                    NextRequest = DateTime.Now,
                };
            }

            IQueryable<RequestLog> log = _requestLog.GetAll()
                                            .Where(x => x.UserId == user.Id
                                                && x.RequestType == RequestType.TvShow
                                                && x.RequestDate >= DateTime.UtcNow.AddDays(-7));

            // Needed, due to a bug which would cause all episode counts to be 0
            int zeroEpisodeCount = await log.Where(x => x.EpisodeCount == 0).Select(x => x.EpisodeCount).CountAsync();

            int episodeCount = await log.Where(x => x.EpisodeCount != 0).Select(x => x.EpisodeCount).SumAsync();

            int count = limit - (zeroEpisodeCount + episodeCount);

            DateTime oldestRequestedAt = await log.OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();

            return new RequestQuotaCountModel()
            {
                HasLimit = true,
                Limit = limit,
                Remaining = count,
                NextRequest = DateTime.SpecifyKind(oldestRequestedAt.AddDays(7), DateTimeKind.Utc),
            };
        }

        public async Task<RequestEngineResult> UpdateAdvancedOptions(MediaAdvancedOptions options)
        {
            var request = await TvRepository.Find(options.RequestId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorMessage = "Request does not exist"
                };
            }

            request.QualityOverride = options.QualityOverride;
            request.RootFolder = options.RootPathOverride;

            await TvRepository.Update(request);

            return new RequestEngineResult
            {
                Result = true
            };
        }
    }
}