using System;
using AutoMapper;
using Ombi.Api.External.ExternalApis.TvMaze;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System.Collections.Generic;
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
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Core.Models;
using System.Threading;
using Microsoft.Extensions.Logging;
using Ombi.Notifications.Models;

namespace Ombi.Core.Engine
{
    public class TvRequestEngine : BaseMediaEngine, ITvRequestEngine
    {
        public TvRequestEngine(ITvMazeApi tvApi, IMovieDbApi movApi, IRequestServiceMain requestService, ICurrentUser user,
            INotificationHelper helper, IRuleEvaluator rule, OmbiUserManager manager, ILogger<TvRequestEngine> logger,
            ITvSender sender, IRepository<RequestLog> rl, ISettingsService<OmbiSettings> settings, ICacheService cache,
            IRepository<RequestSubscription> sub, IMediaCacheService mediaCacheService,
            IUserPlayedEpisodeRepository userPlayedEpisodeRepository,
            IQualityProfileSelectionService qualityProfileSelectionService,
            IRepository<RequestQueue> requestQueue,
            IMediaCleanupEngine mediaCleanupEngine = null) : base(user, requestService, rule, manager, cache, settings, sub)
        {
            TvApi = tvApi;
            MovieDbApi = movApi;
            NotificationHelper = helper;
            _logger = logger;
            TvSender = sender;
            _requestLog = rl;
            _mediaCacheService = mediaCacheService;
            _userPlayedEpisodeRepository = userPlayedEpisodeRepository;
            _qualityProfileSelectionService = qualityProfileSelectionService;
            _requestQueueRepository = requestQueue;
            _mediaCleanupEngine = mediaCleanupEngine;
        }

        private INotificationHelper NotificationHelper { get; }
        private ITvMazeApi TvApi { get; }
        private IMovieDbApi MovieDbApi { get; }
        private ITvSender TvSender { get; }

        private readonly ILogger<TvRequestEngine> _logger;
        private readonly IRepository<RequestLog> _requestLog;
        private readonly IMediaCacheService _mediaCacheService;
        private readonly IUserPlayedEpisodeRepository _userPlayedEpisodeRepository;
        private readonly IQualityProfileSelectionService _qualityProfileSelectionService;
        private readonly IRepository<RequestQueue> _requestQueueRepository;
        private readonly IMediaCleanupEngine _mediaCleanupEngine;

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

            var isAdmin = Username.Equals("API", StringComparison.CurrentCultureIgnoreCase) ||
                          await UserManager.IsInRoleAsync(user, OmbiRoles.PowerUser) ||
                          await UserManager.IsInRoleAsync(user, OmbiRoles.Admin);
            var canSelectQualityProfile = isAdmin || await UserManager.IsInRoleAsync(user, OmbiRoles.SelectQualityProfile);

            if ((tv.RootFolderOverride.HasValue || tv.LanguageProfile.HasValue) && !isAdmin)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissions,
                    Message = "You do not have the correct permissions to change advanced Sonarr options!",
                    ErrorMessage = "You do not have the correct permissions to change advanced Sonarr options!"
                };
            }

            if (tv.QualityPathOverride.HasValue && !canSelectQualityProfile)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissions,
                    Message = "You do not have the correct permissions to select a quality profile!",
                    ErrorMessage = "You do not have the correct permissions to select a quality profile!"
                };
            }

            if (tv.QualityPathOverride.HasValue && tv.QualityPathOverride.Value < 0)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissions,
                    Message = "The selected Sonarr quality profile is invalid.",
                    ErrorMessage = "The selected Sonarr quality profile is invalid."
                };
            }

            if (tv.QualityPathOverride.GetValueOrDefault() > 0 && !isAdmin)
            {
                try
                {
                    if (!await _qualityProfileSelectionService.IsValidSonarrProfile(tv.QualityPathOverride.Value))
                    {
                        return new RequestEngineResult
                        {
                            Result = false,
                            ErrorCode = ErrorCode.NoPermissions,
                            Message = "The selected Sonarr quality profile is no longer available.",
                            ErrorMessage = "The selected Sonarr quality profile is no longer available."
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not validate selected Sonarr quality profile {ProfileId}", tv.QualityPathOverride.Value);
                    return new RequestEngineResult
                    {
                        Result = false,
                        Message = "Ombi could not validate the selected Sonarr quality profile because Sonarr is unavailable.",
                        ErrorMessage = "Ombi could not validate the selected Sonarr quality profile because Sonarr is unavailable."
                    };
                }
            }

            var tvBuilder = new TvShowRequestBuilder(TvApi, MovieDbApi, _logger);
            (await tvBuilder
                .GetShowInfo(tv.TvDbId))
                .CreateTvList(tv)
                .CreateChild(tv, canRequestOnBehalf ? tv.RequestOnBehalf : user.Id);

            await tvBuilder.BuildEpisodes(tv);
            tvBuilder.ChildRequest.QualityOverride = tv.QualityPathOverride;

            var ruleResults = await RunRequestRules(tvBuilder.ChildRequest);
            var results = ruleResults as RuleResult[] ?? ruleResults.ToArray();
            var ruleResultInError = results.FirstOrDefault(x => !x.Success);
            if (ruleResultInError != null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = results.FirstOrDefault(x => !x.Success && !string.IsNullOrEmpty(x.Message))?.Message
                        ?? ruleResultInError.Message,
                    ErrorCode = ruleResultInError.ErrorCode
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

                if (!tvBuilder.ChildRequest.SeasonRequests.Any())
                {
                    // Looks like we have removed them all! They were all duplicates...
                    return new RequestEngineResult
                    {
                        Result = false,
                        ErrorCode = ErrorCode.AlreadyRequested,
                        ErrorMessage = "This has already been requested"
                    };
                }
                return await AddExistingRequest(tvBuilder.ChildRequest, existingRequest, tv.RequestOnBehalf, tv.RootFolderOverride.GetValueOrDefault(), tv.QualityPathOverride);
            }

            // This is a new request. Preserve the legacy API's request-time overrides too.
            var newRequest = tvBuilder.CreateNewRequest(tv);
            newRequest.NewRequest.RootFolder = tv.RootFolderOverride;
            newRequest.NewRequest.QualityOverride = tv.QualityPathOverride;
            newRequest.NewRequest.LanguageProfile = tv.LanguageProfile;
            return await AddRequest(newRequest.NewRequest, tv.RequestOnBehalf);
        }

        public async Task<RequestEngineResult> RequestTvShow(TvRequestViewModelV2 tv)
        {
            var user = await GetUser();
            var canRequestOnBehalf = tv.RequestOnBehalf.HasValue();

            var isAdmin = Username.Equals("API", StringComparison.CurrentCultureIgnoreCase) || await UserManager.IsInRoleAsync(user, OmbiRoles.PowerUser) || await UserManager.IsInRoleAsync(user, OmbiRoles.Admin);
            if (tv.RequestOnBehalf.HasValue() && !isAdmin)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissionsOnBehalf,
                    Message = "You do not have the correct permissions to request on behalf of users!",
                    ErrorMessage = $"You do not have the correct permissions to request on behalf of users!"
                };
            }

            var canSelectQualityProfile = isAdmin || await UserManager.IsInRoleAsync(user, OmbiRoles.SelectQualityProfile);

            if ((tv.RootFolderOverride.HasValue || tv.LanguageProfile.HasValue) && !isAdmin)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissions,
                    Message = "You do not have the correct permissions to change advanced Sonarr options!",
                    ErrorMessage = "You do not have the correct permissions to change advanced Sonarr options!"
                };
            }

            if (tv.QualityPathOverride.HasValue && !canSelectQualityProfile)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissions,
                    Message = "You do not have the correct permissions to select a quality profile!",
                    ErrorMessage = "You do not have the correct permissions to select a quality profile!"
                };
            }

            if (tv.QualityPathOverride.HasValue && tv.QualityPathOverride.Value < 0)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.NoPermissions,
                    Message = "The selected Sonarr quality profile is invalid.",
                    ErrorMessage = "The selected Sonarr quality profile is invalid."
                };
            }

            if (tv.QualityPathOverride.GetValueOrDefault() > 0 && !isAdmin)
            {
                try
                {
                    if (!await _qualityProfileSelectionService.IsValidSonarrProfile(tv.QualityPathOverride.Value))
                    {
                        return new RequestEngineResult
                        {
                            Result = false,
                            ErrorCode = ErrorCode.NoPermissions,
                            Message = "The selected Sonarr quality profile is no longer available.",
                            ErrorMessage = "The selected Sonarr quality profile is no longer available."
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not validate selected Sonarr quality profile {ProfileId}", tv.QualityPathOverride.Value);
                    return new RequestEngineResult
                    {
                        Result = false,
                        Message = "Ombi could not validate the selected Sonarr quality profile because Sonarr is unavailable.",
                        ErrorMessage = "Ombi could not validate the selected Sonarr quality profile because Sonarr is unavailable."
                    };
                }
            }

            var tvBuilder = new TvShowRequestBuilderV2(MovieDbApi);
            var showBuilder = await tvBuilder.GetShowInfo(tv.TheMovieDbId, tv.languageCode);
            if (showBuilder == null)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    Message = "TheMovieDb could not return TV show information. Please try again later.",
                    ErrorMessage = "TheMovieDb could not return TV show information. Please try again later."
                };
            }

            showBuilder
                .CreateTvList(tv)
                .CreateChild(tv, canRequestOnBehalf ? tv.RequestOnBehalf : user.Id, tv.Source);

            await tvBuilder.BuildEpisodes(tv);
            tvBuilder.ChildRequest.QualityOverride = tv.QualityPathOverride;

            var ruleResults = await RunRequestRules(tvBuilder.ChildRequest);
            var results = ruleResults as RuleResult[] ?? ruleResults.ToArray();
            var ruleResultInError = results.FirstOrDefault(x => !x.Success);
            if (ruleResultInError != null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = results.FirstOrDefault(x => !x.Success && !string.IsNullOrEmpty(x.Message))?.Message
                        ?? ruleResultInError.Message,
                    ErrorCode = ruleResultInError.ErrorCode
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

            var requestTvDbId = tvBuilder.ChildRequest.RequestTvDbId;
            var requestImdbId = tvBuilder.ChildRequest.RequestImdbId;

            // Prefer an exact TMDB parent. TVDB/IMDb aliases can identify the same anthology
            // parent while referring to different standalone TMDB seasons, so an alias is only
            // eligible for attachment when title/year metadata or an episode fingerprint proves
            // that the request belongs to that Ombi parent.
            var existingRequest = await TvRepository.Get()
                .FirstOrDefaultAsync(x => x.ExternalProviderId == tv.TheMovieDbId);
            var matchedByExactTmdb = existingRequest != null;

            TvRequestAliasIdentityMatch aliasMatch = null;
            if (existingRequest == null && (requestTvDbId > 0 || !string.IsNullOrEmpty(requestImdbId)))
            {
                var aliasCandidates = await TvRepository.Get()
                    .Where(x =>
                        (requestTvDbId > 0 && x.TvDbId == requestTvDbId) ||
                        (!string.IsNullOrEmpty(requestImdbId) && x.ImdbId == requestImdbId))
                    .ToListAsync();

                if (tvBuilder.ChildRequest.RequestExistingParentId > 0)
                {
                    existingRequest = aliasCandidates.FirstOrDefault(
                        x => x.Id == tvBuilder.ChildRequest.RequestExistingParentId);
                }

                if (existingRequest == null)
                {
                    aliasMatch = TvRequestSeasonIdentityMatcher.FindSafeAliasMatch(
                        tvBuilder.ChildRequest,
                        aliasCandidates);
                    existingRequest = aliasMatch?.Parent;
                }
            }

            if (existingRequest != null)
            {
                var existingSeasons = (existingRequest.ChildRequests ?? new List<ChildRequests>())
                    .Where(x => x?.SeasonRequests != null)
                    .SelectMany(x => x.SeasonRequests)
                    .ToList();
                var useLiteralSeasonNumbers = matchedByExactTmdb ||
                    TvRequestSeasonIdentityMatcher.SeriesMetadataMatches(tvBuilder.ChildRequest, existingRequest) ||
                    aliasMatch?.UseLiteralSeasonNumbers == true;

                // Remove requests we already have, we just want new ones. For alias-only matches,
                // map each season by fingerprint before comparing episode numbers so standalone S1
                // cannot be confused with an unrelated anthology S1.
                foreach (var newChild in tvBuilder.ChildRequest.SeasonRequests.ToList())
                {
                    int? targetSeasonNumber;
                    if (useLiteralSeasonNumbers)
                    {
                        targetSeasonNumber = newChild.SeasonNumber;
                    }
                    else if (tvBuilder.ChildRequest.RequestSeasonMappings.TryGetValue(
                                 newChild.SeasonNumber,
                                 out var hintedSeasonNumber))
                    {
                        targetSeasonNumber = hintedSeasonNumber;
                    }
                    else if (aliasMatch != null && aliasMatch.SeasonMappings.TryGetValue(
                                 newChild.SeasonNumber,
                                 out var aliasSeasonNumber))
                    {
                        targetSeasonNumber = aliasSeasonNumber;
                    }
                    else
                    {
                        targetSeasonNumber = TvRequestSeasonIdentityMatcher.FindSingleSeasonMatch(
                            newChild,
                            existingSeasons);
                    }

                    if (!targetSeasonNumber.HasValue)
                    {
                        continue;
                    }

                    var existingEpisodeNumbers = existingSeasons
                        .Where(x => x.SeasonNumber == targetSeasonNumber.Value)
                        .SelectMany(x => x.Episodes ?? new List<EpisodeRequests>())
                        .Select(x => x.EpisodeNumber)
                        .ToHashSet();

                    newChild.Episodes.RemoveAll(x => existingEpisodeNumbers.Contains(x.EpisodeNumber));
                    if (!newChild.Episodes.Any())
                    {
                        tvBuilder.ChildRequest.SeasonRequests.Remove(newChild);
                    }
                }

                if (!tvBuilder.ChildRequest.SeasonRequests.Any())
                {
                    // Looks like we have removed them all! They were all duplicates...
                    return new RequestEngineResult
                    {
                        Result = false,
                        ErrorCode = ErrorCode.AlreadyRequested,
                        ErrorMessage = "This has already been requested"
                    };
                }
                return await AddExistingRequest(tvBuilder.ChildRequest, existingRequest, tv.RequestOnBehalf, tv.RootFolderOverride.GetValueOrDefault(), tv.QualityPathOverride);
            }

            // This is a new request
            var newRequest = tvBuilder.CreateNewRequest(tv, tv.RootFolderOverride.GetValueOrDefault(), tv.QualityPathOverride.GetValueOrDefault(), tv.LanguageProfile.GetValueOrDefault());
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
            await FillAdditionalFields(shouldHide, allRequests);

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

            await FillAdditionalFields(shouldHide, allRequests);

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

            await FillAdditionalFields(shouldHide, allRequests);
            return allRequests;
        }

        public async Task<RequestsViewModel<ChildRequests>> GetRequests(int count, int position, string sortProperty, string sortOrder, string requestedByUserId = null)
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

            allRequests = FilterByRequestedUser(allRequests.AsQueryable(), requestedByUserId, shouldHide.IsAdmin).ToList();

            allRequests = ApplySortTv(allRequests, sortProperty, sortOrder);

            await FillAdditionalFields(shouldHide, allRequests);

            // Make sure we do not show duplicate child requests
            allRequests = allRequests.DistinctBy(x => x.ParentRequest.Title).ToList();

            var total = allRequests.Count;
            allRequests = allRequests.Skip(position).Take(count).ToList();

            return new RequestsViewModel<ChildRequests>
            {
                Collection = allRequests,
                Total = total,
            };
        }

        public async Task<RequestsViewModel<ChildRequests>> GetRequests(int count, int position, string sortProperty, string sortOrder, RequestStatus status, string requestedByUserId = null)
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

            allRequests = FilterByRequestedUser(allRequests.AsQueryable(), requestedByUserId, shouldHide.IsAdmin).ToList();

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

            allRequests = ApplySortTv(allRequests, sortProperty, sortOrder);

            await FillAdditionalFields(shouldHide, allRequests);

            // Make sure we do not show duplicate child requests
            allRequests = allRequests.DistinctBy(x => x.ParentRequest.Title).ToList();

            var total = allRequests.Count;
            allRequests = allRequests.Skip(position).Take(count).ToList();

            return new RequestsViewModel<ChildRequests>
            {
                Collection = allRequests,
                Total = total,
            };
        }

        public async Task<RequestsViewModel<ChildRequests>> GetUnavailableRequests(int count, int position, string sortProperty, string sortOrder, string requestedByUserId = null)
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

            allRequests = FilterByRequestedUser(allRequests.AsQueryable(), requestedByUserId, shouldHide.IsAdmin).ToList();

            allRequests = ApplySortTv(allRequests, sortProperty, sortOrder);

            await FillAdditionalFields(shouldHide, allRequests);

            // Make sure we do not show duplicate child requests
            allRequests = allRequests.DistinctBy(x => x.ParentRequest.Title).ToList();

            var total = allRequests.Count;
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

            await FillAdditionalFields(shouldHide, allRequests);
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

            await FillAdditionalFields(shouldHide, new List<TvRequests>{request});
            return request;
        }

        private static List<ChildRequests> ApplySortTv(List<ChildRequests> requests, string sortProperty, string sortOrder)
        {
            var asc = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase);
            return sortProperty.ToLowerInvariant() switch
            {
                "id" => asc ? requests.OrderBy(x => x.Id).ToList() : requests.OrderByDescending(x => x.Id).ToList(),
                "title" => asc ? requests.OrderBy(x => x.Title).ToList() : requests.OrderByDescending(x => x.Title).ToList(),
                _ => asc ? requests.OrderBy(x => x.RequestedDate).ToList() : requests.OrderByDescending(x => x.RequestedDate).ToList()
            };
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
                allRequests = await TvRepository.GetChild(shouldHide.UserId).Where(x => x.ParentRequestId == tvId).ToListAsync();
            }
            else
            {
                allRequests = await TvRepository.GetChild().Where(x => x.ParentRequestId == tvId).ToListAsync();
            }

            await FillAdditionalFields(shouldHide, allRequests);

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
            var results = (await allRequests.ToListAsync())
                .Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase))
                .ToList();

            await FillAdditionalFields(shouldHide, results);
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
                    ErrorCode = ErrorCode.ChildRequestDoesNotExist,
                    ErrorMessage = "Child Request does not exist"
                };
            }

            request.MarkedAsApproved = DateTime.Now;
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
            await _mediaCacheService.Purge();

            if (request.Approved)
            {
                var canNotify = await RunSpecificRule(request, SpecificRules.CanSendNotification, string.Empty);
                if (canNotify.Success)
                {
                    await NotificationHelper.Notify(request, NotificationType.RequestApproved);
                }
                // Autosend
                var sendResult = await TvSender.Send(request);
                if (sendResult.Success)
                {
                    await CompleteActiveTvRequestFailures(request.Id);
                }
            }
            return new RequestEngineResult
            {
                Result = true
            };
        }

        private async Task CompleteActiveTvRequestFailures(int requestId)
        {
            var activeFailures = await _requestQueueRepository.GetAll()
                .Where(x => x.RequestId == requestId &&
                    x.Type == RequestType.TvShow &&
                    !x.Completed.HasValue)
                .ToListAsync();

            if (activeFailures.Count == 0)
            {
                return;
            }

            var completedAt = DateTime.UtcNow;
            foreach (var failure in activeFailures)
            {
                failure.Completed = completedAt;
            }

            await _requestQueueRepository.SaveChangesAsync();
        }

        public async Task<RequestEngineResult> DenyChildRequest(int requestId, string reason)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorCode = ErrorCode.ChildRequestDoesNotExist,
                    ErrorMessage = "Child Request does not exist"
                };
            }
            request.Denied = true;
            request.DeniedReason = reason;
            await TvRepository.UpdateChild(request);
            await _mediaCacheService.Purge();
            await NotificationHelper.Notify(request, NotificationType.RequestDeclined);
            return new RequestEngineResult
            {
                Result = true
            };
        }

        public async Task<ChildRequests> UpdateChildRequest(ChildRequests request)
        {
            await TvRepository.UpdateChild(request);
            await _mediaCacheService.Purge();
            return request;
        }

        public async Task<RequestEngineResult> RemoveTvChild(int requestId)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);

            var result = await CheckCanManageRequest(request);
            if (result.IsError)
                return result;

            await NotificationHelper.Notify(new NotificationOptions
            {
                RequestId = 0,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.RequestDeleted,
                RequestType = RequestType.TvShow,
                Recipient = request.RequestedUser?.Email ?? string.Empty,
                UserId = request.RequestedUserId,
                Substitutes = new Dictionary<string, string>
                {
                    { NotificationSubstitues.Title, request.ParentRequest?.Title ?? string.Empty },
                    { NotificationSubstitues.RequestType, RequestType.TvShow.ToString() },
                }
            });

            var parent = await TvRepository.Get()
                .FirstOrDefaultAsync(x => x.Id == request.ParentRequestId);

            // If this is the only child, delete the complete request graph. Otherwise remove only
            // this child's season/episode graph explicitly. The repository also cleans up legacy
            // orphan rows left by older databases where cascading deletes were not enforced.
            if (parent != null && parent.ChildRequests.Count <= 1)
            {
                await TvRepository.DeleteRequest(parent);
                if (_mediaCleanupEngine != null)
                {
                    await _mediaCleanupEngine.CancelForDeletedMediaRequest(
                        RequestType.TvShow,
                        parent.Id,
                        parent.ExternalProviderId,
                        parent.TvDbId);
                }
            }
            else
            {
                await TvRepository.DeleteChild(request);
            }
            await _mediaCacheService.Purge();

            return new RequestEngineResult
            {
                Result = true,
            };
        }

        public async Task RemoveTvRequest(int requestId)
        {
            var request = await TvRepository.Get().FirstOrDefaultAsync(x => x.Id == requestId);

            var usersToNotify = request.ChildRequests
                .Where(x => !string.IsNullOrEmpty(x.RequestedUserId))
                .GroupBy(x => x.RequestedUserId)
                .Select(x => x.First());
            foreach (var userRequest in usersToNotify)
            {
                await NotificationHelper.Notify(new NotificationOptions
                {
                    RequestId = 0,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.RequestDeleted,
                    RequestType = RequestType.TvShow,
                    Recipient = userRequest.RequestedUser?.Email ?? string.Empty,
                    UserId = userRequest.RequestedUserId,
                    Substitutes = new Dictionary<string, string>
                    {
                        { NotificationSubstitues.Title, request.Title },
                        { NotificationSubstitues.RequestType, RequestType.TvShow.ToString() },
                    }
                });
            }

            await TvRepository.DeleteRequest(request);
            if (_mediaCleanupEngine != null)
            {
                await _mediaCleanupEngine.CancelForDeletedMediaRequest(
                    RequestType.TvShow,
                    request.Id,
                    request.ExternalProviderId,
                    request.TvDbId);
            }
            await _mediaCacheService.Purge();
        }

        public async Task<bool> UserHasRequest(string userId)
        {
            return await TvRepository.GetChild().AnyAsync(x => x.RequestedUserId == userId);
        }

        public async Task<RequestEngineResult> MarkUnavailable(int modelId, bool is4K)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorCode = ErrorCode.ChildRequestDoesNotExist,
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
            await _mediaCacheService.Purge();
            return new RequestEngineResult
            {
                Result = true,
                Message = "Request is now unavailable",
            };
        }

        public async Task<RequestEngineResult> MarkAvailable(int modelId, bool is4K)
        {
            ChildRequests request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorCode = ErrorCode.ChildRequestDoesNotExist,
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
            await _mediaCacheService.Purge();
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

        private async Task FillAdditionalFields(HideResult shouldHide, List<TvRequests> x)
        {
            foreach (var tvRequest in x)
            {
                await FillAdditionalFields(shouldHide, tvRequest.ChildRequests);
            }
        }

        private async Task FillAdditionalFields(HideResult shouldHide, List<ChildRequests> childRequests)
        {
            await CheckForSubscription(shouldHide, childRequests);
            CheckForPlayed(shouldHide, childRequests);
        }

        private async Task CheckForSubscription(HideResult shouldHide, List<ChildRequests> childRequests)
        {
            var sub = _subscriptionRepository.GetAll();
            var childIds = childRequests.Select(x => x.Id);
            var relevantSubs = await sub.Where(s =>
                s.UserId == shouldHide.UserId && childIds.Contains(s.RequestId) && s.RequestType == RequestType.TvShow).ToListAsync();
            foreach (var x in childRequests)
            {
                if (shouldHide.UserId == x.RequestedUserId)
                {
                    x.ShowSubscribe = false;
                }
                else
                {
                    if (!x.Available && (!x.Denied ?? true))
                    {
                        x.ShowSubscribe = true;
                    }
                    var result = relevantSubs.FirstOrDefault(s => s.RequestId == x.Id);
                    x.Subscribed = result != null;
                }
            }
        }

        private class EpisodeKey
        {
            public int SeasonNumber;
            public int EpisodeNumber;
        }

        private void CheckForPlayed(HideResult shouldHide, List<ChildRequests> childRequests)
        {
            foreach (var request in childRequests)
            {
                var requestedEpisodes = GetEpisodesKeys(request);
                var theMovieDbId = request.ParentRequest?.ExternalProviderId ?? 0;
                if (theMovieDbId <= 0)
                {
                    request.RequestedUserPlayedProgress = 0;
                    continue;
                }

                var playedEpisodes = _userPlayedEpisodeRepository
                    .GetAll()
                    .Where(x => x.TheMovieDbId == theMovieDbId && x.UserId == request.RequestedUserId)
                    .AsEnumerable()
                    .Join(requestedEpisodes,
                        played => new { played.SeasonNumber, played.EpisodeNumber },
                        requested => new { requested.SeasonNumber, requested.EpisodeNumber },
                        (played, requested) => new { played });

                var playedCount = playedEpisodes.Count();
                var toWatchCount = requestedEpisodes.Count();
                if (playedCount == 0 || toWatchCount == 0)
                {
                    request.RequestedUserPlayedProgress = 0;
                }
                else
                {
                    request.RequestedUserPlayedProgress = 100 * playedCount / toWatchCount;
                }
                
            }
        }

        private List<EpisodeKey> GetEpisodesKeys(ChildRequests request)
        {
            List<EpisodeKey> result = new List<EpisodeKey>();
            foreach(var season in request.SeasonRequests) 
            {
                foreach(var episode in season.Episodes) 
                {
                    result.Add(new EpisodeKey
                    {
                        SeasonNumber = season.SeasonNumber,
                        EpisodeNumber = episode.EpisodeNumber
                    });
                }
            }
            return result;
        }

        private async Task<RequestEngineResult> AddExistingRequest(ChildRequests newRequest, TvRequests existingRequest, string requestOnBehalf, int rootFolder, int? qualityProfile)
        {
            // Add the child
            existingRequest.ChildRequests.Add(newRequest);
            if (qualityProfile.HasValue)
            {
                // Zero explicitly clears a previous request-level override and restores the
                // normal Ombi/Sonarr profile behavior for future processing.
                existingRequest.QualityOverride = qualityProfile.Value;
            }
            if (rootFolder > 0)
            {
                existingRequest.RootFolder = rootFolder;
            }

            await TvRepository.Update(existingRequest);

            return await AfterRequest(newRequest, requestOnBehalf);
        }

        private async Task<RequestEngineResult> AddRequest(TvRequests model, string requestOnBehalf)
        {
            await TvRepository.Add(model);
            // This is a new request so we should only have 1 child
            return await AfterRequest(model.ChildRequests.FirstOrDefault(), requestOnBehalf);
        }

        public async Task<RequestEngineResult> ReProcessRequest(int requestId, bool is4K, CancellationToken cancellationToken)
        {
            var request = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.RequestDoesNotExist,
                    ErrorMessage = "Request does not exist"
                };
            }

            return await ProcessSendingShow(request);
        }


        private async Task<RequestEngineResult> AfterRequest(ChildRequests model, string requestOnBehalf)
        {
            var sendRuleResult = await RunSpecificRule(model, SpecificRules.CanSendNotification, requestOnBehalf);
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
            await _mediaCacheService.Purge();

            return await ProcessSendingShow(model);
        }

        private async Task<RequestEngineResult> ProcessSendingShow(ChildRequests model)
        {
            if (model.Approved)
            {
                // Autosend
                var canNotify = await RunSpecificRule(model, SpecificRules.CanSendNotification, string.Empty);
                if (canNotify.Success)
                {
                    await NotificationHelper.Notify(model, NotificationType.RequestApproved);
                }
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

       

        public async Task<RequestEngineResult> UpdateAdvancedOptions(MediaAdvancedOptions options)
        {
            var request = await TvRepository.Find(options.RequestId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    ErrorCode = ErrorCode.RequestDoesNotExist,
                    ErrorMessage = "Request does not exist"
                };
            }

            request.QualityOverride = options.QualityOverride;
            request.RootFolder = options.RootPathOverride;
            if (options.LanguageProfile > 0)
            {
                request.LanguageProfile = options.LanguageProfile;
            }

            await TvRepository.Update(request);

            return new RequestEngineResult
            {
                Result = true
            };
        }
    }
}
