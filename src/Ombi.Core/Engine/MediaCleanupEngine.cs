using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.ExternalApis.Radarr;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Core.Models.MediaCleanup;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine
{
    public class MediaCleanupEngine : IMediaCleanupEngine
    {
        private static readonly SemaphoreSlim StateLock = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan[] RetryDelays =
        {
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(4),
            TimeSpan.FromHours(8),
            TimeSpan.FromHours(12),
            TimeSpan.FromHours(24)
        };

        // Quartz schedules jobs at whole-second precision, while DateTime.UtcNow includes
        // sub-second ticks. Keep cleanup deadlines at the same precision so a job that
        // fires at the displayed due second cannot miss an otherwise-due record and defer
        // deletion until the next scheduler pass.
        private static DateTime TruncateToSecond(DateTime value)
        {
            return value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
        }

        private static bool IsDeletionDue(DateTime scheduledForDeletionAt, DateTime now)
        {
            // Truncate both sides for backwards compatibility with records created by
            // older builds that persisted fractional seconds in ScheduledForDeletionAt.
            return TruncateToSecond(scheduledForDeletionAt) <= TruncateToSecond(now);
        }

        private static TimeSpan GetRetryDelay(int retryCount)
        {
            var index = Math.Min(Math.Max(retryCount - 1, 0), RetryDelays.Length - 1);
            return RetryDelays[index];
        }

        private readonly ISettingsService<MediaCleanupSettings> _settings;
        private readonly ISettingsService<MediaCleanupState> _state;
        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<Radarr4KSettings> _radarr4KSettings;
        private readonly ISettingsService<SonarrSettings> _sonarrSettings;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly IMovieRequestRepository _movieRequests;
        private readonly ITvRequestRepository _tvRequests;
        private readonly ICurrentUser _currentUser;
        private readonly OmbiUserManager _userManager;
        private readonly IRadarrV3Api _radarr;
        private readonly ISonarrV3Api _sonarr;
        private readonly IPlexApi _plex;
        private readonly IPlexContentRepository _plexContent;
        private readonly IExternalRepository<RadarrCache> _radarrCache;
        private readonly IExternalRepository<SonarrCache> _sonarrCache;
        private readonly IExternalRepository<SonarrEpisodeCache> _sonarrEpisodeCache;
        private readonly IMediaCacheService _mediaCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MediaCleanupEngine> _logger;

        public MediaCleanupEngine(
            ISettingsService<MediaCleanupSettings> settings,
            ISettingsService<MediaCleanupState> state,
            ISettingsService<RadarrSettings> radarrSettings,
            ISettingsService<Radarr4KSettings> radarr4KSettings,
            ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<PlexSettings> plexSettings,
            IMovieRequestRepository movieRequests,
            ITvRequestRepository tvRequests,
            ICurrentUser currentUser,
            OmbiUserManager userManager,
            IRadarrV3Api radarr,
            ISonarrV3Api sonarr,
            IPlexApi plex,
            IPlexContentRepository plexContent,
            IExternalRepository<RadarrCache> radarrCache,
            IExternalRepository<SonarrCache> sonarrCache,
            IExternalRepository<SonarrEpisodeCache> sonarrEpisodeCache,
            IMediaCacheService mediaCache,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MediaCleanupEngine> logger)
        {
            _settings = settings;
            _state = state;
            _radarrSettings = radarrSettings;
            _radarr4KSettings = radarr4KSettings;
            _sonarrSettings = sonarrSettings;
            _plexSettings = plexSettings;
            _movieRequests = movieRequests;
            _tvRequests = tvRequests;
            _currentUser = currentUser;
            _userManager = userManager;
            _radarr = radarr;
            _sonarr = sonarr;
            _plex = plex;
            _plexContent = plexContent;
            _radarrCache = radarrCache;
            _sonarrCache = sonarrCache;
            _sonarrEpisodeCache = sonarrEpisodeCache;
            _mediaCache = mediaCache;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<MediaCleanupOverview> GetOverview(
            RequestType? requestType = null,
            int? requestId = null,
            int? mediaId = null,
            bool includeMetrics = true,
            bool includeLastPlayed = true,
            CancellationToken cancellationToken = default)
        {
            var settings = await _settings.GetSettingsAsync();
            var user = await _currentUser.GetUser();
            if (user == null)
            {
                return new MediaCleanupOverview { Settings = settings };
            }

            var permissions = await GetPermissions(user);
            var result = new MediaCleanupOverview
            {
                Settings = settings,
                CanRequestRemoval = permissions.CanRequestRemoval,
                CanDeleteOwnMedia = permissions.CanDeleteOwnMedia,
                CanVote = permissions.CanVote,
                CanManage = permissions.CanManage
            };

            // Do not fan out to the request database, Radarr/Sonarr or Plex when Media Cleanup is
            // disabled or the caller cannot use it. This endpoint is public to authenticated users,
            // so the cheap authorization checks must happen before any expensive catalog work.
            if (!settings.Enabled || (!permissions.CanRequestRemoval && !permissions.CanVote && !permissions.CanManage))
            {
                return result;
            }

            var state = await LoadState();
            var activeRecords = state.Requests
                .Where(IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            var representedCleanupIds = new HashSet<string>(StringComparer.Ordinal);

            // Vote identities are moderation data. Only cleanup managers/admins receive
            // them; normal voters continue to see aggregate totals and their own vote.
            Dictionary<string, string> voterDisplayNames = null;
            if (permissions.CanManage)
            {
                var voterIds = activeRecords
                    .SelectMany(x => x.Votes ?? new List<MediaCleanupVoteRecord>())
                    .Select(x => x.UserId)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                if (voterIds.Count > 0)
                {
                    var voters = await _userManager.Users
                        .Where(x => voterIds.Contains(x.Id))
                        .Select(x => new { x.Id, x.Alias, x.UserName })
                        .ToListAsync(cancellationToken);

                    voterDisplayNames = voters.ToDictionary(
                        x => x.Id,
                        x => string.IsNullOrWhiteSpace(x.Alias) ? x.UserName : x.Alias);
                }
                else
                {
                    voterDisplayNames = new Dictionary<string, string>();
                }
            }

            var includeMovies = !requestType.HasValue || requestType == RequestType.Movie;
            var includeTv = !requestType.HasValue || requestType == RequestType.TvShow;
            var movieSizes = includeMetrics && includeMovies ? await GetMovieSizes() : new Dictionary<int, long>();
            var tvSizes = includeMetrics && includeTv ? await GetTvSizes() : new Dictionary<int, long>();
            var plexLookup = includeLastPlayed ? await GetPlexContentLookup() : null;
            var plexKeys = new Dictionary<(RequestType Type, int RequestId), string>();
            var now = DateTime.UtcNow;

            if (includeMovies)
            {
                var movieQuery = _movieRequests.GetWithUser().Where(x => x.Available);
                if (requestId.HasValue)
                {
                    movieQuery = movieQuery.Where(x => x.Id == requestId.Value);
                }
                else if (mediaId.HasValue)
                {
                    // Media details pages have a stable TMDB id even when Ombi does not
                    // expose the request id to the current user. Resolve the shared request
                    // by provider id so community cleanup is not tied to request ownership.
                    movieQuery = movieQuery.Where(x => x.TheMovieDbId == mediaId.Value);
                }

                var movies = await movieQuery.OrderBy(x => x.Title).ToListAsync(cancellationToken);
                foreach (var movie in movies)
                {
                    var cleanup = FindActiveForMedia(
                        activeRecords,
                        RequestType.Movie,
                        movie.Id,
                        movie.TheMovieDbId,
                        0);
                    TrackRepresentedCleanup(representedCleanupIds, cleanup);
                    var availableSince = movie.MarkedAsAvailable ?? (movie.RequestedDate == default ? (DateTime?)null : movie.RequestedDate);
                    var owned = movie.RequestedUserId == user.Id;
                    var ageEligible = IsAgeEligible(availableSince, settings.MinimumMediaAgeDays, now);
                    var stewardshipSince = GetCleanupStewardshipSince(
                        state,
                        RequestType.Movie,
                        movie.Id,
                        movie.TheMovieDbId,
                        0,
                        user.Id);
                    var canNominateUnderRestriction = CanNominateUnderRestriction(
                        settings,
                        permissions,
                        owned,
                        stewardshipSince.HasValue);
                    if (!CanSeeItem(cleanup, owned, settings, permissions, user.Id))
                    {
                        continue;
                    }

                    if (plexLookup != null)
                    {
                        var plexKey = plexLookup.FindMovie(movie.TheMovieDbId, movie.ImdbId);
                        if (!string.IsNullOrEmpty(plexKey))
                        {
                            plexKeys[(RequestType.Movie, movie.Id)] = plexKey;
                        }
                    }

                    result.Items.Add(new MediaCleanupItemViewModel
                    {
                        RequestType = RequestType.Movie,
                        RequestId = movie.Id,
                        Title = movie.Title,
                        PosterPath = movie.PosterPath,
                        Overview = movie.Overview,
                        ReleaseDate = movie.ReleaseDate == default ? (DateTime?)null : movie.ReleaseDate,
                        RequestedBy = GetRequesterDisplayName(movie.RequestedUser, movie.RequestedByAlias, permissions.CanManage),
                        OwnedByCurrentUser = owned,
                        IsCleanupSteward = stewardshipSince.HasValue,
                        StewardshipSince = stewardshipSince,
                        CanRequestOwnRemoval = cleanup == null && owned && CanUseOwnRemoval(settings, permissions),
                        CanNominate = cleanup == null && ageEligible && settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote
                            && canNominateUnderRestriction,
                        CanVote = cleanup != null && cleanup.Origin == MediaCleanupOrigin.Community && settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote && IsVoteable(cleanup),
                        CanManage = cleanup != null && permissions.CanManage,
                        CanCancel = cleanup != null && !cleanup.ExternalDeletionCompletedAt.HasValue && (permissions.CanManage || cleanup.RequestedByUserId == user.Id),
                        CommunityAgeEligible = ageEligible,
                        AvailableSince = availableSince,
                        SizeOnDisk = movieSizes.TryGetValue(movie.TheMovieDbId, out var movieSize) ? movieSize : 0,
                        Cleanup = ToViewModel(cleanup, user.Id, settings, voterDisplayNames)
                    });
                }
            }

            if (includeTv)
            {
                var tvQuery = _tvRequests.GetLite()
                    .Where(x => x.ChildRequests.Any() && x.ChildRequests.All(c => c.Available));
                if (requestId.HasValue)
                {
                    tvQuery = tvQuery.Where(x => x.Id == requestId.Value);
                }
                else if (mediaId.HasValue)
                {
                    // TV detail ids are TMDB ids in the current search model. Keep the
                    // legacy TVDB comparison as a fallback for older request records.
                    tvQuery = tvQuery.Where(x => x.ExternalProviderId == mediaId.Value || x.TvDbId == mediaId.Value);
                }

                var tvRequests = await tvQuery.OrderBy(x => x.Title).ToListAsync(cancellationToken);
                foreach (var tv in tvRequests)
                {
                    var cleanup = FindActiveForMedia(
                        activeRecords,
                        RequestType.TvShow,
                        tv.Id,
                        tv.ExternalProviderId,
                        tv.TvDbId);
                    TrackRepresentedCleanup(representedCleanupIds, cleanup);
                    var owners = tv.ChildRequests.Select(x => x.RequestedUserId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    var owned = owners.Count == 1 && owners[0] == user.Id;
                    var requestedByCurrentUser = owners.Contains(user.Id);
                    var availableSince = tv.ChildRequests
                        .Select(x => x.MarkedAsAvailable ?? (x.RequestedDate == default ? (DateTime?)null : x.RequestedDate))
                        .Where(x => x.HasValue)
                        .OrderByDescending(x => x.Value)
                        .FirstOrDefault();
                    var ageEligible = IsAgeEligible(availableSince, settings.MinimumMediaAgeDays, now);
                    var stewardshipSince = GetCleanupStewardshipSince(
                        state,
                        RequestType.TvShow,
                        tv.Id,
                        tv.ExternalProviderId,
                        tv.TvDbId,
                        user.Id);
                    var canNominateUnderRestriction = CanNominateUnderRestriction(
                        settings,
                        permissions,
                        requestedByCurrentUser,
                        stewardshipSince.HasValue);
                    var requestedBy = tv.ChildRequests
                        .Select(x => GetRequesterDisplayName(x.RequestedUser, x.RequestedByAlias, permissions.CanManage))
                        .Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    if (!CanSeeItem(cleanup, owned, settings, permissions, user.Id))
                    {
                        continue;
                    }

                    if (plexLookup != null)
                    {
                        var plexKey = plexLookup.FindSeries(tv.TvDbId, tv.ExternalProviderId, tv.ImdbId);
                        if (!string.IsNullOrEmpty(plexKey))
                        {
                            plexKeys[(RequestType.TvShow, tv.Id)] = plexKey;
                        }
                    }

                    result.Items.Add(new MediaCleanupItemViewModel
                    {
                        RequestType = RequestType.TvShow,
                        RequestId = tv.Id,
                        Title = tv.Title,
                        PosterPath = tv.PosterPath,
                        Overview = tv.Overview,
                        ReleaseDate = tv.ReleaseDate == default ? (DateTime?)null : tv.ReleaseDate,
                        RequestedBy = requestedBy.Count == 1 ? requestedBy[0] : requestedBy.Count > 1 ? "Multiple users" : string.Empty,
                        OwnedByCurrentUser = owned,
                        IsCleanupSteward = stewardshipSince.HasValue,
                        StewardshipSince = stewardshipSince,
                        CanRequestOwnRemoval = cleanup == null && owned && CanUseOwnRemoval(settings, permissions),
                        CanNominate = cleanup == null && ageEligible && settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote
                            && canNominateUnderRestriction,
                        CanVote = cleanup != null && cleanup.Origin == MediaCleanupOrigin.Community && settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote && IsVoteable(cleanup),
                        CanManage = cleanup != null && permissions.CanManage,
                        CanCancel = cleanup != null && !cleanup.ExternalDeletionCompletedAt.HasValue && (permissions.CanManage || cleanup.RequestedByUserId == user.Id),
                        CommunityAgeEligible = ageEligible,
                        AvailableSince = availableSince,
                        SizeOnDisk = tvSizes.TryGetValue(tv.TvDbId, out var tvSize) ? tvSize : 0,
                        Cleanup = ToViewModel(cleanup, user.Id, settings, voterDisplayNames)
                    });
                }

                // A partial TV cleanup can legitimately remove the last remaining Ombi request
                // row while the series itself (and other seasons) still exists in Sonarr. The
                // cleanup catalog is request-backed, so without a separate anchor the title would
                // disappear and could not be cleaned again. Re-use the latest completed partial
                // cleanup record as a media-catalog anchor while Sonarr still knows about the
                // series. This avoids keeping empty/fake request rows and stays compatible with
                // request-graph self-healing.
                var sonarrSeriesCache = await _sonarrCache.GetAll()
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                var sonarrTvDbIds = sonarrSeriesCache
                    .Where(x => x.TvDbId > 0)
                    .Select(x => x.TvDbId)
                    .ToHashSet();
                var sonarrMovieDbIds = sonarrSeriesCache
                    .Where(x => x.TheMovieDbId > 0)
                    .Select(x => x.TheMovieDbId)
                    .ToHashSet();

                var representedRequestIds = tvRequests.Select(x => x.Id).ToHashSet();
                var representedTvDbIds = tvRequests.Where(x => x.TvDbId > 0).Select(x => x.TvDbId).ToHashSet();
                var representedMovieDbIds = tvRequests.Where(x => x.ExternalProviderId > 0).Select(x => x.ExternalProviderId).ToHashSet();

                var residualRecords = state.Requests
                    .Where(x => x.RequestType == RequestType.TvShow && x.Status == MediaCleanupStatus.Completed)
                    .GroupBy(GetTvCleanupIdentityKey)
                    .Select(x => x.OrderByDescending(r => r.CompletedAt ?? r.CreatedAt).First())
                    .Where(IsPartialTvCleanup)
                    .Where(x =>
                        (x.TvDbId > 0 && sonarrTvDbIds.Contains(x.TvDbId)) ||
                        (x.TheMovieDbId > 0 && sonarrMovieDbIds.Contains(x.TheMovieDbId)))
                    .Where(x =>
                        !representedRequestIds.Contains(x.MediaRequestId) &&
                        !(x.TvDbId > 0 && representedTvDbIds.Contains(x.TvDbId)) &&
                        !(x.TheMovieDbId > 0 && representedMovieDbIds.Contains(x.TheMovieDbId)))
                    .Where(x => !requestId.HasValue || x.MediaRequestId == requestId.Value)
                    .Where(x => !mediaId.HasValue ||
                                (x.TheMovieDbId > 0 && x.TheMovieDbId == mediaId.Value) ||
                                (x.TvDbId > 0 && x.TvDbId == mediaId.Value))
                    .OrderBy(x => x.Title)
                    .ToList();

                var residualOwnerIds = residualRecords
                    .SelectMany(x => x.OwnerUserIds ?? new List<string>())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                var residualOwners = residualOwnerIds.Count == 0
                    ? new Dictionary<string, OmbiUser>()
                    : (await _userManager.Users
                        .Where(x => residualOwnerIds.Contains(x.Id))
                        .ToListAsync(cancellationToken))
                        .ToDictionary(x => x.Id);

                foreach (var record in residualRecords)
                {
                    var cleanup = FindActiveForMedia(
                        activeRecords,
                        RequestType.TvShow,
                        record.MediaRequestId,
                        record.TheMovieDbId,
                        record.TvDbId);
                    TrackRepresentedCleanup(representedCleanupIds, cleanup);
                    var owners = (record.OwnerUserIds ?? new List<string>())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                    var owned = owners.Count == 1 && owners[0] == user.Id;
                    var requestedByCurrentUser = owners.Contains(user.Id);
                    var ageEligible = IsAgeEligible(record.AvailableSince, settings.MinimumMediaAgeDays, now);
                    var stewardshipSince = GetCleanupStewardshipSince(
                        state,
                        RequestType.TvShow,
                        record.MediaRequestId,
                        record.TheMovieDbId,
                        record.TvDbId,
                        user.Id);
                    var canNominateUnderRestriction = CanNominateUnderRestriction(
                        settings,
                        permissions,
                        requestedByCurrentUser,
                        stewardshipSince.HasValue);
                    var requestedBy = owners
                        .Where(residualOwners.ContainsKey)
                        .Select(x => GetRequesterDisplayName(residualOwners[x], null, permissions.CanManage))
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();

                    if (!CanSeeItem(cleanup, owned, settings, permissions, user.Id))
                    {
                        continue;
                    }

                    if (plexLookup != null)
                    {
                        var plexKey = plexLookup.FindSeries(record.TvDbId, record.TheMovieDbId, null);
                        if (!string.IsNullOrEmpty(plexKey))
                        {
                            plexKeys[(RequestType.TvShow, record.MediaRequestId)] = plexKey;
                        }
                    }

                    result.Items.Add(new MediaCleanupItemViewModel
                    {
                        RequestType = RequestType.TvShow,
                        RequestId = record.MediaRequestId,
                        Title = record.Title,
                        PosterPath = record.PosterPath,
                        RequestedBy = requestedBy.Count == 1 ? requestedBy[0] : requestedBy.Count > 1 ? "Multiple users" : string.Empty,
                        OwnedByCurrentUser = owned,
                        IsCleanupSteward = stewardshipSince.HasValue,
                        StewardshipSince = stewardshipSince,
                        CanRequestOwnRemoval = cleanup == null && owned && CanUseOwnRemoval(settings, permissions),
                        CanNominate = cleanup == null && ageEligible && settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote
                            && canNominateUnderRestriction,
                        CanVote = cleanup != null && cleanup.Origin == MediaCleanupOrigin.Community && settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote && IsVoteable(cleanup),
                        CanManage = cleanup != null && permissions.CanManage,
                        CanCancel = cleanup != null && !cleanup.ExternalDeletionCompletedAt.HasValue && (permissions.CanManage || cleanup.RequestedByUserId == user.Id),
                        CommunityAgeEligible = ageEligible,
                        AvailableSince = record.AvailableSince,
                        SizeOnDisk = tvSizes.TryGetValue(record.TvDbId, out var residualTvSize) ? residualTvSize : 0,
                        Cleanup = ToViewModel(cleanup, user.Id, settings, voterDisplayNames)
                    });
                }
            }

            // Active cleanup workflows must never disappear just because the request-backed
            // catalog changes underneath them. Vote reminders are generated from the persisted
            // workflow state, so every active record that was not represented by a currently
            // eligible request (or a residual TV anchor) is surfaced directly from that state.
            // This keeps Voting, PendingAdminApproval and ScheduledForDeletion records visible
            // for their full lifecycle, including when availability/request rows temporarily
            // change or a request is re-created under a different id.
            var unrepresentedActive = activeRecords
                .Where(x => !string.IsNullOrEmpty(x.Id) && !representedCleanupIds.Contains(x.Id))
                .Where(x => (includeMovies && x.RequestType == RequestType.Movie) ||
                            (includeTv && x.RequestType == RequestType.TvShow))
                .Where(x => !requestId.HasValue || x.MediaRequestId == requestId.Value)
                .Where(x => !mediaId.HasValue ||
                            (x.RequestType == RequestType.Movie && x.TheMovieDbId == mediaId.Value) ||
                            (x.RequestType == RequestType.TvShow &&
                             (x.TheMovieDbId == mediaId.Value || x.TvDbId == mediaId.Value)))
                .OrderBy(x => x.Title)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            if (unrepresentedActive.Count > 0)
            {
                _logger.LogDebug(
                    "Media Cleanup is surfacing {Count} active workflow record(s) from persisted state because the request-backed catalog did not represent them.",
                    unrepresentedActive.Count);

                var fallbackOwnerIds = unrepresentedActive
                    .SelectMany(x => x.OwnerUserIds ?? new List<string>())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                var fallbackOwners = fallbackOwnerIds.Count == 0
                    ? new Dictionary<string, OmbiUser>()
                    : (await _userManager.Users
                        .Where(x => fallbackOwnerIds.Contains(x.Id))
                        .ToListAsync(cancellationToken))
                        .ToDictionary(x => x.Id);

                foreach (var record in unrepresentedActive)
                {
                    var owners = (record.OwnerUserIds ?? new List<string>())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                    var owned = owners.Count == 1 && owners[0] == user.Id;
                    if (!CanSeeItem(record, owned, settings, permissions, user.Id))
                    {
                        continue;
                    }

                    var requestedBy = owners
                        .Where(fallbackOwners.ContainsKey)
                        .Select(x => GetRequesterDisplayName(fallbackOwners[x], null, permissions.CanManage))
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                    var stewardshipSince = GetCleanupStewardshipSince(
                        state,
                        record.RequestType,
                        record.MediaRequestId,
                        record.TheMovieDbId,
                        record.TvDbId,
                        user.Id);
                    var ageEligible = IsAgeEligible(record.AvailableSince, settings.MinimumMediaAgeDays, now);

                    if (plexLookup != null)
                    {
                        var plexKey = record.RequestType == RequestType.Movie
                            ? plexLookup.FindMovie(record.TheMovieDbId, null)
                            : plexLookup.FindSeries(record.TvDbId, record.TheMovieDbId, null);
                        if (!string.IsNullOrEmpty(plexKey))
                        {
                            plexKeys[(record.RequestType, record.MediaRequestId)] = plexKey;
                        }
                    }

                    var sizeOnDisk = record.SizeOnDisk;
                    if (record.RequestType == RequestType.Movie &&
                        movieSizes.TryGetValue(record.TheMovieDbId, out var fallbackMovieSize))
                    {
                        sizeOnDisk = fallbackMovieSize;
                    }
                    else if (record.RequestType == RequestType.TvShow &&
                             tvSizes.TryGetValue(record.TvDbId, out var fallbackTvSize))
                    {
                        sizeOnDisk = fallbackTvSize;
                    }

                    result.Items.Add(new MediaCleanupItemViewModel
                    {
                        RequestType = record.RequestType,
                        RequestId = record.MediaRequestId,
                        Title = record.Title,
                        PosterPath = record.PosterPath,
                        RequestedBy = requestedBy.Count == 1 ? requestedBy[0] : requestedBy.Count > 1 ? "Multiple users" : string.Empty,
                        OwnedByCurrentUser = owned,
                        IsCleanupSteward = stewardshipSince.HasValue,
                        StewardshipSince = stewardshipSince,
                        // An active workflow already exists, so fallback rows only expose
                        // actions against that workflow. They must not start a second one.
                        CanRequestOwnRemoval = false,
                        CanNominate = false,
                        CanVote = record.Origin == MediaCleanupOrigin.Community &&
                                  settings.CommunityCleanup != CommunityCleanupMode.Off &&
                                  permissions.CanVote &&
                                  IsVoteable(record),
                        CanManage = permissions.CanManage,
                        CanCancel = !record.ExternalDeletionCompletedAt.HasValue && (permissions.CanManage || record.RequestedByUserId == user.Id),
                        CommunityAgeEligible = ageEligible,
                        AvailableSince = record.AvailableSince,
                        SizeOnDisk = sizeOnDisk,
                        Cleanup = ToViewModel(record, user.Id, settings, voterDisplayNames)
                    });
                }
            }

            if (includeLastPlayed)
            {
                await PopulateLastPlayed(result.Items, plexKeys, cancellationToken);
            }
            return result;
        }

        public async Task<MediaCleanupTvSelectionViewModel> GetTvSelection(int requestId)
        {
            var settings = await _settings.GetSettingsAsync();
            if (!settings.Enabled)
            {
                return new MediaCleanupTvSelectionViewModel { Result = false, Message = "Media Cleanup is disabled." };
            }

            var user = await _currentUser.GetUser();
            if (user == null)
            {
                return new MediaCleanupTvSelectionViewModel { Result = false, Message = "User could not be resolved." };
            }

            var permissions = await GetPermissions(user);
            if (!permissions.CanRequestRemoval && !permissions.CanVote && !permissions.CanManage)
            {
                return new MediaCleanupTvSelectionViewModel { Result = false, Message = "You do not have permission to use Media Cleanup." };
            }

            // Use the same visibility decision as the overview before contacting Sonarr. Knowing an
            // Ombi request id must not be enough to enumerate another user's episode/file metadata.
            var state = await LoadState();
            var activeRecords = state.Requests.Where(IsActive).ToList();
            var tv = await _tvRequests.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            MediaCleanupRecord residualRecord = null;
            MediaCleanupRecord cleanup;
            bool owned;
            var usableRequest = tv != null && tv.ChildRequests != null && tv.ChildRequests.Any() && tv.ChildRequests.All(x => x.Available);

            if (usableRequest)
            {
                cleanup = FindActiveForMedia(
                    activeRecords,
                    RequestType.TvShow,
                    tv.Id,
                    tv.ExternalProviderId,
                    tv.TvDbId);
                var owners = tv.ChildRequests
                    .Select(x => x.RequestedUserId)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                owned = owners.Count == 1 && owners[0] == user.Id;
            }
            else
            {
                residualRecord = FindResidualTvCatalogRecord(state, requestId);
                if (residualRecord == null)
                {
                    return new MediaCleanupTvSelectionViewModel { Result = false, Message = "The requested TV cleanup selection is not available." };
                }

                cleanup = FindActiveForMedia(
                    activeRecords,
                    RequestType.TvShow,
                    residualRecord.MediaRequestId,
                    residualRecord.TheMovieDbId,
                    residualRecord.TvDbId);
                var owners = (residualRecord.OwnerUserIds ?? new List<string>())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                owned = owners.Count == 1 && owners[0] == user.Id;
            }

            if (!CanSeeItem(cleanup, owned, settings, permissions, user.Id))
            {
                return new MediaCleanupTvSelectionViewModel { Result = false, Message = "The requested TV cleanup selection is not available." };
            }

            if (!usableRequest && !await HasResidualTvSeries(residualRecord))
            {
                return new MediaCleanupTvSelectionViewModel { Result = false, Message = "The available Ombi TV request or residual Sonarr series could not be found." };
            }

            var title = usableRequest ? tv.Title : residualRecord.Title;
            var tvDbId = usableRequest ? tv.TvDbId : residualRecord.TvDbId;

            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();
            if (!sonarrSettings.Enabled)
            {
                return new MediaCleanupTvSelectionViewModel
                {
                    Result = false,
                    RequestId = requestId,
                    Title = title,
                    DeleteFilesEnabled = settings.DeleteFiles,
                    Message = "Sonarr is not enabled, so episode-level cleanup is unavailable."
                };
            }

            try
            {
                var series = (await _sonarr.GetSeries(sonarrSettings.ApiKey, sonarrSettings.FullUri))
                    .FirstOrDefault(x => x.tvdbId == tvDbId);
                if (series == null)
                {
                    return new MediaCleanupTvSelectionViewModel
                    {
                        Result = false,
                        RequestId = requestId,
                        Title = title,
                        DeleteFilesEnabled = settings.DeleteFiles,
                        Message = "This series could not be found in Sonarr."
                    };
                }

                var episodes = (await _sonarr.GetEpisodes(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri)).ToList();
                var episodeFiles = (await _sonarr.GetEpisodeFiles(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri)).ToList();
                var fileById = episodeFiles.GroupBy(x => x.id).ToDictionary(x => x.Key, x => x.First());
                // The UI only needs a stable grouping key so multi-episode files are selected and
                // sized once. Do not expose Sonarr's internal EpisodeFileId to the browser.
                var fileGroupById = episodeFiles
                    .Where(x => x.id > 0)
                    .Select(x => x.id)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select((fileId, index) => new { fileId, groupId = index + 1 })
                    .ToDictionary(x => x.fileId, x => x.groupId);

                var result = new MediaCleanupTvSelectionViewModel
                {
                    Result = true,
                    RequestId = requestId,
                    Title = title,
                    DeleteFilesEnabled = settings.DeleteFiles,
                    SizeOnDisk = episodeFiles.GroupBy(x => x.id).Sum(x => x.First().size)
                };

                result.Seasons = episodes
                    .Where(x => x.seasonNumber >= 0)
                    .GroupBy(x => x.seasonNumber)
                    .OrderBy(x => x.Key)
                    .Select(season =>
                    {
                        var seasonEpisodes = season.OrderBy(x => x.episodeNumber).ToList();
                        var seasonFileIds = seasonEpisodes
                            .Where(x => x.hasFile && x.episodeFileId > 0)
                            .Select(x => x.episodeFileId)
                            .Distinct()
                            .ToList();

                        return new MediaCleanupTvSeasonViewModel
                        {
                            SeasonNumber = season.Key,
                            SizeOnDisk = seasonFileIds.Sum(fileId => fileById.TryGetValue(fileId, out var file) ? file.size : 0),
                            Episodes = seasonEpisodes.Select(episode => new MediaCleanupTvEpisodeViewModel
                            {
                                SeasonNumber = episode.seasonNumber,
                                EpisodeNumber = episode.episodeNumber,
                                Title = episode.title,
                                AirDateUtc = episode.airDateUtc == default ? (DateTime?)null : episode.airDateUtc,
                                HasFile = episode.hasFile && episode.episodeFileId > 0,
                                FileGroupId = episode.episodeFileId > 0 && fileGroupById.TryGetValue(episode.episodeFileId, out var groupId) ? groupId : 0,
                                SizeOnDisk = episode.episodeFileId > 0 && fileById.TryGetValue(episode.episodeFileId, out var file) ? file.size : 0
                            }).ToList()
                        };
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load Sonarr episodes for Media Cleanup request {RequestId}", requestId);
                return new MediaCleanupTvSelectionViewModel
                {
                    Result = false,
                    RequestId = requestId,
                    Title = title,
                    DeleteFilesEnabled = settings.DeleteFiles,
                    Message = "Ombi could not load this series' episodes from Sonarr."
                };
            }
        }

        public async Task<MediaCleanupActionResult> RequestOwnRemoval(RequestType requestType, int requestId, MediaCleanupSelection selection = null)
        {
            await StateLock.WaitAsync();
            try
            {
                var settings = await _settings.GetSettingsAsync();
                if (settings.OwnRequestRemoval == OwnRequestRemovalMode.Off)
                {
                    return Fail("Removing your own media is disabled by the administrator.");
                }

                var user = await _currentUser.GetUser();
                if (user == null)
                {
                    return Fail("User could not be resolved.");
                }

                var permissions = await GetPermissions(user);
                if (!permissions.CanRequestRemoval)
                {
                    return Fail("You do not have permission to request media removal.");
                }

                if (settings.OwnRequestRemoval == OwnRequestRemovalMode.ImmediateDeletion && !permissions.CanDeleteOwnMedia)
                {
                    return Fail("Immediate deletion requires the DeleteOwnMedia role.");
                }

                var target = await ResolveTarget(requestType, requestId);
                if (target == null || !target.Available)
                {
                    return Fail("The available Ombi request could not be found.");
                }

                if (target.OwnerUserIds.Count != 1 || target.OwnerUserIds[0] != user.Id)
                {
                    return Fail(requestType == RequestType.TvShow
                        ? "A TV series can only be removed as your own request when all Ombi requests for the series belong to you. Use community cleanup when multiple users requested it."
                        : "You can only remove media that you requested.");
                }

                var tvSelection = await ResolveTvCleanupSelection(target, selection);
                if (!string.IsNullOrEmpty(tvSelection.Error))
                {
                    return Fail(tvSelection.Error);
                }
                if (tvSelection.Episodes.Count > 0 && !settings.DeleteFiles)
                {
                    return Fail("Specific TV episode cleanup requires Delete Files to be enabled in Media Cleanup settings. You can still remove the entire series.");
                }

                var state = await LoadState();
                if (FindActive(state, target) != null)
                {
                    return Fail("This title already has an active cleanup request.");
                }

                var record = CreateRecord(target, user.Id, MediaCleanupOrigin.OwnRequest);
                ApplyTvSelection(record, tvSelection);
                if (settings.OwnRequestRemoval == OwnRequestRemovalMode.RequestRemoval)
                {
                    record.Status = MediaCleanupStatus.PendingAdminApproval;
                    state.Requests.Add(record);
                    if (!await TrySaveState(state, "submitting an own-request removal for administrator approval"))
                    {
                        state.Requests.Remove(record);
                        return PersistenceFailure(record.Id);
                    }
                    QueueManagersPendingApprovalNotification(record, settings);
                    return Success("Removal request submitted for administrator approval.", record.Id);
                }

                var deletionPlanError = await TrySnapshotDeletionPlan(record, settings);
                if (!string.IsNullOrEmpty(deletionPlanError))
                {
                    return Fail(deletionPlanError);
                }

                record.Status = MediaCleanupStatus.ScheduledForDeletion;
                record.ScheduledForDeletionAt = TruncateToSecond(DateTime.UtcNow);
                state.Requests.Add(record);

                // Immediate deletion must be durable before the first destructive *arr call.
                // Persist through a fresh scope so a process crash after Sonarr/Radarr accepts
                // the delete cannot erase the cleanup record that authorized the operation.
                try
                {
                    await SaveStateCheckpoint(state);
                }
                catch (Exception ex)
                {
                    state.Requests.Remove(record);
                    _logger.LogError(
                        ex,
                        "Could not persist immediate media cleanup request for {RequestType} '{Title}' ({CleanupId}); no external deletion was attempted",
                        record.RequestType,
                        record.Title,
                        record.Id);
                    return Fail("Media removal could not start because the cleanup request could not be persisted. No media was deleted.");
                }

                // ExecuteDeletion persists a second checkpoint after the destructive operation,
                // then this scoped state is saved once with the final/retryable reconciliation state.
                await ExecuteDeletion(record, settings, state);
                if (!await TrySaveState(state, "persisting the final state of an immediate media removal"))
                {
                    if (record.ExternalDeletionCompletedAt.HasValue)
                    {
                        return Fail(
                            "Media was removed from the external service, but Ombi could not persist the final cleanup state. " +
                            "The recovery checkpoint is durable and Ombi will retry reconciliation automatically.",
                            record.Id);
                    }

                    return Fail(
                        "Media cleanup could not persist its latest state. The durable cleanup request remains available for recovery and may retry automatically.",
                        record.Id);
                }
                if (record.Status == MediaCleanupStatus.Completed)
                {
                    return Success("Media was removed successfully.", record.Id);
                }
                if (record.ExternalDeletionCompletedAt.HasValue)
                {
                    return Success("Media was removed successfully. Ombi reconciliation is still pending and will retry automatically.", record.Id);
                }
                return Fail(record.FailureReason ?? "Media deletion failed.", record.Id);
            }
            finally
            {
                StateLock.Release();
            }
        }

        public async Task<MediaCleanupActionResult> Nominate(RequestType requestType, int requestId, MediaCleanupSelection selection = null)
        {
            await StateLock.WaitAsync();
            try
            {
                var settings = await _settings.GetSettingsAsync();
                if (settings.CommunityCleanup == CommunityCleanupMode.Off)
                {
                    return Fail("Community cleanup voting is disabled.");
                }

                var user = await _currentUser.GetUser();
                if (user == null)
                {
                    return Fail("User could not be resolved.");
                }

                var permissions = await GetPermissions(user);
                if (!permissions.CanVote)
                {
                    return Fail("You do not have permission to participate in media cleanup voting.");
                }

                var target = await ResolveTarget(requestType, requestId);
                if (target == null || !target.Available)
                {
                    return Fail("The available Ombi request could not be found.");
                }

                var state = await LoadState();
                var requestedByCurrentUser = target.OwnerUserIds?.Contains(user.Id) == true;
                var isCleanupSteward = GetCleanupStewardshipSince(
                    state,
                    target.RequestType,
                    target.RequestId,
                    target.TheMovieDbId,
                    target.TvDbId,
                    user.Id).HasValue;
                if (!CanNominateUnderRestriction(
                        settings,
                        permissions,
                        requestedByCurrentUser,
                        isCleanupSteward))
                {
                    return Fail("You can only nominate media that you requested or currently have cleanup stewardship for from a previous Keep vote.");
                }

                if (!IsAgeEligible(target.AvailableSince, settings.MinimumMediaAgeDays, DateTime.UtcNow))
                {
                    return Fail($"This title must be available for at least {settings.MinimumMediaAgeDays} days before community cleanup can be started.");
                }

                var tvSelection = await ResolveTvCleanupSelection(target, selection);
                if (!string.IsNullOrEmpty(tvSelection.Error))
                {
                    return Fail(tvSelection.Error);
                }
                if (tvSelection.Episodes.Count > 0 && !settings.DeleteFiles)
                {
                    return Fail("Specific TV episode cleanup requires Delete Files to be enabled in Media Cleanup settings. You can still nominate the entire series.");
                }

                var existing = FindActive(state, target);
                if (existing != null)
                {
                    return Fail("This title already has an active cleanup vote.", existing.Id);
                }

                var record = CreateRecord(target, user.Id, MediaCleanupOrigin.Community);
                ApplyTvSelection(record, tvSelection);
                record.Status = MediaCleanupStatus.Voting;
                record.VotingEndsAt = DateTime.UtcNow.AddDays(Math.Max(1, settings.VotingPeriodDays));
                record.Votes.Add(new MediaCleanupVoteRecord
                {
                    UserId = user.Id,
                    Vote = MediaCleanupVoteType.Delete,
                    Date = DateTime.UtcNow
                });
                await EvaluateCommunityAndSnapshotDeletionPlan(record, settings, DateTime.UtcNow);
                state.Requests.Add(record);
                if (!await TrySaveState(state, "starting a community cleanup vote"))
                {
                    state.Requests.Remove(record);
                    return PersistenceFailure(record.Id);
                }
                if (record.Status == MediaCleanupStatus.PendingAdminApproval)
                {
                    QueueManagersPendingApprovalNotification(record, settings);
                }

                return Success("Cleanup vote started. Your delete vote was recorded.", record.Id);
            }
            finally
            {
                StateLock.Release();
            }
        }

        public async Task<MediaCleanupActionResult> Vote(string cleanupRequestId, MediaCleanupVoteType vote)
        {
            await StateLock.WaitAsync();
            try
            {
                var settings = await _settings.GetSettingsAsync();
                if (settings.CommunityCleanup == CommunityCleanupMode.Off)
                {
                    return Fail("Community cleanup voting is disabled.");
                }

                var user = await _currentUser.GetUser();
                if (user == null)
                {
                    return Fail("User could not be resolved.");
                }

                var permissions = await GetPermissions(user);
                if (!permissions.CanVote)
                {
                    return Fail("You do not have permission to vote on media cleanup.");
                }

                var state = await LoadState();
                var record = state.Requests.FirstOrDefault(x => x.Id == cleanupRequestId);
                if (record == null || record.Origin != MediaCleanupOrigin.Community)
                {
                    return Fail("This cleanup request is not open for voting.");
                }

                var statusBeforeDeadlineEvaluation = record.Status;
                await EvaluateCommunityAndSnapshotDeletionPlan(record, settings, DateTime.UtcNow);
                if (!IsVoteable(record))
                {
                    if (!await TrySaveState(state, "persisting a cleanup vote deadline transition"))
                    {
                        return PersistenceFailure(record.Id);
                    }
                    if (statusBeforeDeadlineEvaluation != MediaCleanupStatus.PendingAdminApproval &&
                        record.Status == MediaCleanupStatus.PendingAdminApproval)
                    {
                        QueueManagersPendingApprovalNotification(record, settings);
                    }
                    return Fail("This cleanup request is no longer open for voting.", record.Id);
                }

                var previousStatus = record.Status;
                var existing = record.Votes.FirstOrDefault(x => x.UserId == user.Id);
                if (existing == null)
                {
                    record.Votes.Add(new MediaCleanupVoteRecord { UserId = user.Id, Vote = vote, Date = DateTime.UtcNow });
                }
                else
                {
                    existing.Vote = vote;
                    existing.Date = DateTime.UtcNow;
                }

                await EvaluateCommunityAndSnapshotDeletionPlan(record, settings, DateTime.UtcNow);
                if (!await TrySaveState(state, "recording a media cleanup vote"))
                {
                    return PersistenceFailure(record.Id);
                }
                if (previousStatus != MediaCleanupStatus.PendingAdminApproval && record.Status == MediaCleanupStatus.PendingAdminApproval)
                {
                    QueueManagersPendingApprovalNotification(record, settings);
                }
                return Success(vote == MediaCleanupVoteType.Delete ? "Delete vote recorded." : "Keep vote recorded.", record.Id);
            }
            finally
            {
                StateLock.Release();
            }
        }

        public async Task<MediaCleanupActionResult> Approve(string cleanupRequestId)
        {
            await StateLock.WaitAsync();
            try
            {
                var user = await _currentUser.GetUser();
                if (user == null || !(await GetPermissions(user)).CanManage)
                {
                    return Fail("You do not have permission to manage media cleanup.");
                }

                var settings = await _settings.GetSettingsAsync();
                var state = await LoadState();
                var record = state.Requests.FirstOrDefault(x => x.Id == cleanupRequestId);
                if (record == null || record.Status != MediaCleanupStatus.PendingAdminApproval)
                {
                    return Fail("This cleanup request is not awaiting administrator approval.");
                }

                if (!IsOriginEnabled(record, settings))
                {
                    return Fail("This cleanup system is currently disabled in Media Cleanup settings.");
                }

                var deletionPlanError = await TrySnapshotDeletionPlan(record, settings);
                if (!string.IsNullOrEmpty(deletionPlanError))
                {
                    return Fail(deletionPlanError, record.Id);
                }

                record.ApprovedByUserId = user.Id;
                record.Status = MediaCleanupStatus.ScheduledForDeletion;
                record.ScheduledForDeletionAt = TruncateToSecond(DateTime.UtcNow).AddDays(Math.Max(0, settings.GracePeriodDays));
                ResetRetryState(record);
                if (!await TrySaveState(state, "approving and scheduling a media cleanup"))
                {
                    return PersistenceFailure(record.Id);
                }
                return Success("Cleanup approved and scheduled for deletion.", record.Id);
            }
            finally
            {
                StateLock.Release();
            }
        }

        public async Task<MediaCleanupActionResult> Reject(string cleanupRequestId)
        {
            return await SetTerminalState(cleanupRequestId, MediaCleanupStatus.Rejected, "Cleanup request rejected.", true);
        }

        public async Task<MediaCleanupActionResult> Cancel(string cleanupRequestId)
        {
            await StateLock.WaitAsync();
            try
            {
                var user = await _currentUser.GetUser();
                if (user == null)
                {
                    return Fail("User could not be resolved.");
                }

                var state = await LoadState();
                var record = state.Requests.FirstOrDefault(x => x.Id == cleanupRequestId);
                if (record == null || !IsActive(record))
                {
                    return Fail("This cleanup request is no longer active.");
                }

                var canManage = (await GetPermissions(user)).CanManage;
                if (!canManage && record.RequestedByUserId != user.Id)
                {
                    return Fail("Only the user who started the cleanup request or a cleanup manager can cancel it.");
                }

                if (record.ExternalDeletionCompletedAt.HasValue)
                {
                    return Fail("The media has already been deleted from the external service. Ombi reconciliation is still pending and cannot be cancelled.", record.Id);
                }

                record.Status = MediaCleanupStatus.Cancelled;
                record.ScheduledForDeletionAt = null;
                if (!await TrySaveState(state, "cancelling a media cleanup request"))
                {
                    return PersistenceFailure(record.Id);
                }
                return Success("Cleanup request cancelled.", record.Id);
            }
            finally
            {
                StateLock.Release();
            }
        }

        public async Task CancelForDeletedMediaRequest(
            RequestType requestType,
            int requestId,
            int theMovieDbId = 0,
            int tvDbId = 0)
        {
            // Request deletion is authoritative. If the underlying Ombi request is explicitly
            // removed, any cleanup workflow for that request must stop as well so voters are
            // never reminded about, or asked to act on, a request that no longer exists.
            await StateLock.WaitAsync();
            try
            {
                var state = await LoadState();
                var matches = state.Requests
                    .Where(x => x != null &&
                                IsActive(x) &&
                                !x.ExternalDeletionCompletedAt.HasValue &&
                                IsSameCleanupMedia(x, requestType, requestId, theMovieDbId, tvDbId))
                    .ToList();

                if (matches.Count == 0)
                {
                    return;
                }

                foreach (var record in matches)
                {
                    record.Status = MediaCleanupStatus.Cancelled;
                    record.ScheduledForDeletionAt = null;
                    ResetRetryState(record);
                }

                if (!await TrySaveState(state, "cancelling cleanup workflows for a deleted Ombi request"))
                {
                    _logger.LogError(
                        "Could not persist cancellation of {Count} Media Cleanup workflow(s) for deleted Ombi {RequestType} request {RequestId}. " +
                        "The underlying request is already deleted; any stale cleanup will be blocked by deletion revalidation.",
                        matches.Count,
                        requestType,
                        requestId);
                    return;
                }

                _logger.LogInformation(
                    "Cancelled {Count} active Media Cleanup workflow(s) because the underlying Ombi {RequestType} request was deleted. RequestId={RequestId}, TMDB={TmdbId}, TVDB={TvdbId}",
                    matches.Count,
                    requestType,
                    requestId,
                    theMovieDbId,
                    tvDbId);
            }
            finally
            {
                StateLock.Release();
            }
        }

        public async Task ProcessPending()
        {
            await StateLock.WaitAsync();
            try
            {
                var settings = await _settings.GetSettingsAsync();
                var state = await LoadState();
                var now = DateTime.UtcNow;
                var changed = false;
                var newlyPendingApproval = new List<MediaCleanupRecord>();

                foreach (var record in state.Requests.Where(IsActive).ToList())
                {
                    if (record.Origin == MediaCleanupOrigin.Community)
                    {
                        var before = record.Status;
                        var beforeScheduled = record.ScheduledForDeletionAt;
                        await EvaluateCommunityAndSnapshotDeletionPlan(record, settings, now);
                        changed |= before != record.Status || beforeScheduled != record.ScheduledForDeletionAt;
                        if (before != MediaCleanupStatus.PendingAdminApproval && record.Status == MediaCleanupStatus.PendingAdminApproval)
                        {
                            newlyPendingApproval.Add(record);
                        }
                    }

                    var deletionDueAt = record.NextRetryAt ?? record.ScheduledForDeletionAt;
                    if (record.Status == MediaCleanupStatus.ScheduledForDeletion &&
                        deletionDueAt.HasValue &&
                        IsDeletionDue(deletionDueAt.Value, now) &&
                        (record.ExternalDeletionCompletedAt.HasValue || IsOriginEnabled(record, settings)))
                    {
                        await ExecuteDeletion(record, settings, state);
                        changed = true;
                    }
                }

                if (changed && !await TrySaveState(state, "persisting scheduled Media Cleanup processing"))
                {
                    // Do not emit notifications for transitions that were not durably saved.
                    // A later scheduler pass will reload the last durable state and evaluate it again.
                    return;
                }

                foreach (var record in newlyPendingApproval)
                {
                    QueueManagersPendingApprovalNotification(record, settings);
                }
            }
            finally
            {
                StateLock.Release();
            }
        }

        private async Task<MediaCleanupActionResult> SetTerminalState(string id, MediaCleanupStatus status, string message, bool managerOnly)
        {
            await StateLock.WaitAsync();
            try
            {
                var user = await _currentUser.GetUser();
                if (user == null)
                {
                    return Fail("User could not be resolved.");
                }

                if (managerOnly && !(await GetPermissions(user)).CanManage)
                {
                    return Fail("You do not have permission to manage media cleanup.");
                }

                var state = await LoadState();
                var record = state.Requests.FirstOrDefault(x => x.Id == id);
                if (record == null || !IsActive(record))
                {
                    return Fail("This cleanup request is no longer active.");
                }

                if (record.ExternalDeletionCompletedAt.HasValue)
                {
                    return Fail("The media has already been deleted from the external service. Ombi reconciliation is still pending and cannot be rejected.", record.Id);
                }

                record.Status = status;
                record.ScheduledForDeletionAt = null;
                ResetRetryState(record);
                if (!await TrySaveState(state, $"setting media cleanup state to {status}"))
                {
                    return PersistenceFailure(record.Id);
                }
                return Success(message, record.Id);
            }
            finally
            {
                StateLock.Release();
            }
        }

        private void QueueManagersPendingApprovalNotification(MediaCleanupRecord record, MediaCleanupSettings cleanupSettings)
        {
            if (!cleanupSettings.NotifyManagersOnPendingApproval)
            {
                return;
            }

            // Approval email is a secondary notification. Do not hold the user's cleanup
            // HTTP request open while SMTP connects/sends. Use a fresh DI scope so the
            // background work never touches request-scoped services after they are disposed.
            var cleanupId = record.Id;
            var title = record.Title;
            var requestedByUserId = record.RequestedByUserId;
            var origin = record.Origin;
            var requestType = record.RequestType;
            var scopeLabel = BuildCleanupScopeLabel(record);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailSettingsService = scope.ServiceProvider.GetRequiredService<ISettingsService<EmailNotificationSettings>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<OmbiUserManager>();
                    var emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();

                    var emailSettings = await emailSettingsService.GetSettingsAsync();
                    if (emailSettings == null || !emailSettings.Enabled)
                    {
                        return;
                    }

                    var recipients = new List<OmbiUser>();
                    recipients.AddRange(await userManager.GetUsersInRoleAsync(OmbiRoles.Admin));
                    recipients.AddRange(await userManager.GetUsersInRoleAsync(OmbiRoles.PowerUser));
                    recipients.AddRange(await userManager.GetUsersInRoleAsync(OmbiRoles.ManageMediaCleanup));

                    var requester = string.IsNullOrEmpty(requestedByUserId)
                        ? null
                        : await userManager.FindByIdAsync(requestedByUserId);
                    var requesterName = requester?.UserAlias ?? "An Ombi user";
                    var encodedTitle = System.Net.WebUtility.HtmlEncode(title ?? "Media");
                    var encodedRequester = System.Net.WebUtility.HtmlEncode(requesterName);
                    var encodedScope = System.Net.WebUtility.HtmlEncode(scopeLabel ?? string.Empty);
                    var scopeHtml = requestType == RequestType.TvShow
                        ? $"<p>Cleanup scope: <strong>{encodedScope}</strong></p>"
                        : string.Empty;
                    var scopePlain = requestType == RequestType.TvShow
                        ? $" Cleanup scope: {scopeLabel}."
                        : string.Empty;
                    var source = origin == MediaCleanupOrigin.OwnRequest
                        ? $"{encodedRequester} requested removal of this title."
                        : "A community cleanup vote reached the configured threshold and now requires approval.";
                    var plainSource = origin == MediaCleanupOrigin.OwnRequest
                        ? $"{requesterName} requested removal of this title."
                        : "A community cleanup vote reached the configured threshold and now requires approval.";

                    foreach (var recipient in recipients
                        .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Email))
                        .GroupBy(x => x.Id)
                        .Select(x => x.First()))
                    {
                        await emailProvider.SendAdHoc(new NotificationMessage
                        {
                            To = recipient.Email,
                            Subject = $"Media cleanup approval required: {title}",
                            Message = $"<p><strong>{encodedTitle}</strong> is waiting for Media Cleanup approval.</p>{scopeHtml}<p>{source}</p><p>Open Ombi and go to <strong>Media Cleanup</strong> to approve or reject it.</p>",
                            Other =
                            {
                                ["PlainTextBody"] = $"{title} is waiting for Media Cleanup approval.{scopePlain} {plainSource} Open Ombi and go to Media Cleanup to approve or reject it."
                            }
                        }, emailSettings);
                    }
                }
                catch (Exception ex)
                {
                    // Approval workflow must not fail merely because SMTP is unavailable.
                    _logger.LogWarning(ex, "Could not send Media Cleanup approval notification for {Title} ({CleanupId})", title, cleanupId);
                }
            });
        }

        private async Task ExecuteDeletion(MediaCleanupRecord record, MediaCleanupSettings settings, MediaCleanupState state)
        {
            // The external delete is destructive while everything after it is reconciliation.
            // Persist a checkpoint between those phases so a DB/cache failure or application
            // restart cannot cause the destructive *arr operation to be issued a second time.
            if (!record.ExternalDeletionCompletedAt.HasValue)
            {
                try
                {
                    if (record.DeletionPlan == null)
                    {
                        throw new MediaCleanupTerminalException(
                            "This cleanup was authorized before destructive settings and destination snapshotting was available. " +
                            "Create/approve a new cleanup request before deleting media.");
                    }

                    // Re-check Ombi's request/ownership state immediately before the first
                    // destructive *arr call. Authorization may shrink while it waits, but a new
                    // request/owner or a changed target must never inherit an older approval.
                    await RevalidateDeletionAuthorization(record);

                    var externalDeleted = record.RequestType == RequestType.Movie
                        ? await DeleteMovie(record)
                        : record.RequestType == RequestType.TvShow
                            ? await DeleteTv(record)
                            : throw new MediaCleanupTerminalException($"Unsupported cleanup request type: {record.RequestType}.");

                    if (!externalDeleted)
                    {
                        // A previous transient attempt may have reached *arr successfully even if
                        // Ombi never received the response. If a whole-title retry now confirms the
                        // title is absent, treat the destructive phase as complete rather than
                        // getting permanently stuck on an ambiguous timeout/network failure.
                        if (record.RetryCount > 0 && !IsPartialTvCleanup(record))
                        {
                            _logger.LogWarning(
                                "Media cleanup retry could no longer find {RequestType} '{Title}' ({CleanupId}) in *arr after {RetryCount} transient failure(s); treating the external deletion as already completed",
                                record.RequestType,
                                record.Title,
                                record.Id,
                                record.RetryCount);
                        }
                        else
                        {
                            throw new MediaCleanupTerminalException(record.RequestType == RequestType.Movie
                                ? "The movie could not be found in an enabled Radarr instance."
                                : "The series or selected episodes could not be found in the enabled Sonarr instance.");
                        }
                    }

                    record.ExternalDeletionCompletedAt = DateTime.UtcNow;
                    ResetRetryState(record);

                    // Use a fresh scope for the checkpoint. SettingsService attaches a new
                    // GlobalSettings entity each time it saves, so saving twice through the same
                    // scoped SettingsContext can trigger EF's duplicate-tracking exception.
                    await SaveStateCheckpoint(state);
                    _logger.LogInformation(
                        "Media cleanup external deletion completed for {RequestType} '{Title}' ({CleanupId}); starting Ombi reconciliation",
                        record.RequestType,
                        record.Title,
                        record.Id);
                }
                catch (MediaCleanupTerminalException ex)
                {
                    MarkTerminalFailure(record, ex, "external deletion");
                    return;
                }
                catch (HttpRequestException ex) when (IsPermanentExternalHttpFailure(ex))
                {
                    MarkTerminalFailure(record, ex, "external deletion");
                    return;
                }
                catch (UriFormatException ex)
                {
                    MarkTerminalFailure(record, ex, "external deletion configuration");
                    return;
                }
                catch (ArgumentException ex)
                {
                    MarkTerminalFailure(record, ex, "external deletion configuration");
                    return;
                }
                catch (Exception ex)
                {
                    var phase = record.ExternalDeletionCompletedAt.HasValue
                        ? "external deletion recovery checkpoint"
                        : "external deletion";
                    ScheduleRetry(record, ex, phase);
                    return;
                }
            }
            else
            {
                _logger.LogInformation(
                    "Resuming Ombi reconciliation for {RequestType} '{Title}' ({CleanupId}); external deletion completed at {ExternalDeletionCompletedAt}",
                    record.RequestType,
                    record.Title,
                    record.Id,
                    record.ExternalDeletionCompletedAt.Value);
            }

            try
            {
                if (record.RequestType == RequestType.Movie)
                {
                    // The details page determines Requested by provider id, not by the cleanup
                    // record's single request id. Remove every Ombi request row for this movie so
                    // historical/duplicate rows cannot leave the title stuck as Requested.
                    var movieRequests = await _movieRequests.GetAll()
                        .Where(x => x.TheMovieDbId == record.TheMovieDbId || x.Id == record.MediaRequestId)
                        .ToListAsync();
                    if (movieRequests.Count > 0)
                    {
                        await _movieRequests.DeleteRange(movieRequests);
                    }
                }
                else if (IsPartialTvCleanup(record))
                {
                    await RemoveSelectedTvRequests(record);
                }
                else
                {
                    // Whole-series cleanup keeps the existing behavior: remove every Ombi request
                    // parent for the provider identity so ExistingRule cannot leave the title stuck
                    // as Requested after Sonarr removes the series.
                    var tvRequests = await _tvRequests.Get()
                        .Where(x => x.Id == record.MediaRequestId ||
                                    (record.TheMovieDbId > 0 && x.ExternalProviderId == record.TheMovieDbId) ||
                                    (record.TvDbId > 0 && x.TvDbId == record.TvDbId))
                        .ToListAsync();
                    if (tvRequests.Count > 0)
                    {
                        await _tvRequests.DeleteRange(tvRequests);
                    }
                }

                // Plex availability is backed by Ombi's external content cache, not a live Plex lookup.
                // A partial Plex sync is additive and can leave a deleted title marked Available, so
                // remove the matching cached row immediately after a successful cleanup.
                await RemovePlexAvailabilityCache(record);
                await RemoveArrSearchCache(record);
                await _mediaCache.Purge();
                record.Status = MediaCleanupStatus.Completed;
                record.CompletedAt = DateTime.UtcNow;
                record.ScheduledForDeletionAt = null;
                ResetRetryState(record);
                _logger.LogInformation("Media cleanup removed {RequestType} '{Title}' ({CleanupId})", record.RequestType, record.Title, record.Id);
            }
            catch (Exception ex)
            {
                // The destructive operation is already complete, so reconciliation is safe to
                // retry indefinitely. Back off to avoid hammering a temporarily unavailable DB or
                // service, but never turn an already-deleted title into a terminal cleanup failure.
                ScheduleRetry(record, ex, "Ombi reconciliation");
            }
        }

        private static bool IsPermanentExternalHttpFailure(HttpRequestException exception)
        {
            if (!exception.StatusCode.HasValue)
            {
                // Transport failures such as DNS errors, connection resets, and timeouts do not
                // carry an HTTP response status and remain retryable.
                return false;
            }

            var status = exception.StatusCode.Value;
            if (status == HttpStatusCode.RequestTimeout || status == HttpStatusCode.TooManyRequests)
            {
                return false;
            }

            var numericStatus = (int)status;
            if (numericStatus >= 500 && numericStatus <= 599)
            {
                return false;
            }

            // A concrete non-success response outside the retryable classes needs a request,
            // credential, permission, or configuration change before another attempt can help.
            return numericStatus >= 300 && numericStatus <= 499;
        }

        private void ScheduleRetry(MediaCleanupRecord record, Exception ex, string phase)
        {
            var now = DateTime.UtcNow;
            if (record.RetryCount < int.MaxValue)
            {
                record.RetryCount++;
            }

            var delay = GetRetryDelay(record.RetryCount);
            record.Status = MediaCleanupStatus.ScheduledForDeletion;
            record.LastFailureAt = now;
            record.NextRetryAt = TruncateToSecond(now.Add(delay));
            record.FailureReason = $"{phase} failed and will retry automatically: {ex.Message}";

            _logger.LogWarning(ex,
                "Media cleanup {Phase} failed for {RequestType} '{Title}' ({CleanupId}); retry {RetryCount} is scheduled for {NextRetryAt}",
                phase,
                record.RequestType,
                record.Title,
                record.Id,
                record.RetryCount,
                record.NextRetryAt);
        }

        private void MarkTerminalFailure(MediaCleanupRecord record, Exception ex, string phase)
        {
            record.Status = MediaCleanupStatus.Failed;
            record.LastFailureAt = DateTime.UtcNow;
            record.NextRetryAt = null;
            record.ScheduledForDeletionAt = null;
            record.FailureReason = ex.Message;
            _logger.LogError(ex,
                "Media cleanup {Phase} failed permanently for {RequestType} '{Title}' ({CleanupId})",
                phase,
                record.RequestType,
                record.Title,
                record.Id);
        }

        private static void ResetRetryState(MediaCleanupRecord record)
        {
            record.RetryCount = 0;
            record.LastFailureAt = null;
            record.NextRetryAt = null;
            record.FailureReason = null;
        }

        private async Task RemoveSelectedTvRequests(MediaCleanupRecord record)
        {
            // SelectedEpisodes is the frozen authorization boundary for partial TV cleanup.
            // SelectedSeasons is derived metadata describing what was complete at selection time;
            // it must never authorize episodes that appeared after the cleanup was approved.
            var selected = record.SelectedEpisodes
                .Select(x => (x.SeasonNumber, x.EpisodeNumber))
                .ToHashSet();
            var selectedSeasonNumbers = selected.Select(x => x.SeasonNumber)
                .Distinct()
                .ToList();
            if (selected.Count == 0)
            {
                return;
            }

            // Resolve every Ombi parent that represents this series, but only prune request-graph
            // nodes that are actually touched by the selected cleanup. The previous implementation
            // walked the whole graph and removed any pre-existing empty season/child it happened to
            // encounter, which could erase unrelated historical requests for other seasons.
            var parentIds = await _tvRequests.GetLite()
                .Where(x => x.Id == record.MediaRequestId ||
                            (record.TheMovieDbId > 0 && x.ExternalProviderId == record.TheMovieDbId) ||
                            (record.TvDbId > 0 && x.TvDbId == record.TvDbId))
                .Select(x => x.Id)
                .Distinct()
                .ToListAsync();
            if (parentIds.Count == 0)
            {
                return;
            }

            var touchedSeasons = await _tvRequests.Db.Set<SeasonRequests>()
                .Include(x => x.Episodes)
                .Where(x => parentIds.Contains(x.ChildRequest.ParentRequestId) &&
                            selectedSeasonNumbers.Contains(x.SeasonNumber))
                .ToListAsync();

            var touchedSeasonIds = touchedSeasons.Select(x => x.Id).Distinct().ToList();
            var touchedChildIds = touchedSeasons.Select(x => x.ChildRequestId).Distinct().ToList();
            var touchedParentIds = touchedChildIds.Count == 0
                ? new List<int>()
                : await _tvRequests.Db.ChildRequests
                    .Where(x => touchedChildIds.Contains(x.Id))
                    .Select(x => x.ParentRequestId)
                    .Distinct()
                    .ToListAsync();
            var removedEpisodes = 0;

            foreach (var season in touchedSeasons)
            {
                var episodes = season.Episodes
                    .Where(x => selected.Contains((season.SeasonNumber, x.EpisodeNumber)))
                    .ToList();
                if (episodes.Count == 0)
                {
                    continue;
                }

                _tvRequests.Db.EpisodeRequests.RemoveRange(episodes);
                removedEpisodes += episodes.Count;
            }

            if (removedEpisodes > 0)
            {
                await _tvRequests.Save();
            }

            // Collapse only nodes made empty by the rows removed above. Untouched legacy or
            // historical request nodes are deliberately left alone here; global request repair is
            // handled by the dedicated maintenance path, not by a partial media cleanup.
            var emptyTouchedSeasons = touchedSeasonIds.Count == 0
                ? new List<SeasonRequests>()
                : await _tvRequests.Db.Set<SeasonRequests>()
                    .Where(x => touchedSeasonIds.Contains(x.Id) &&
                                !_tvRequests.Db.EpisodeRequests.Any(e => e.SeasonId == x.Id))
                    .ToListAsync();
            var removedSeasons = emptyTouchedSeasons.Count;
            if (removedSeasons > 0)
            {
                _tvRequests.Db.Set<SeasonRequests>().RemoveRange(emptyTouchedSeasons);
                await _tvRequests.Save();
            }

            var emptyTouchedChildren = touchedChildIds.Count == 0
                ? new List<ChildRequests>()
                : await _tvRequests.Db.ChildRequests
                    .Where(x => touchedChildIds.Contains(x.Id) &&
                                !_tvRequests.Db.Set<SeasonRequests>().Any(s => s.ChildRequestId == x.Id))
                    .ToListAsync();
            var removedChildren = emptyTouchedChildren.Count;
            if (removedChildren > 0)
            {
                _tvRequests.Db.ChildRequests.RemoveRange(emptyTouchedChildren);
                await _tvRequests.Save();
            }

            var emptyTouchedParents = touchedParentIds.Count == 0
                ? new List<TvRequests>()
                : await _tvRequests.Db.TvRequests
                    .Where(x => touchedParentIds.Contains(x.Id) &&
                                !_tvRequests.Db.ChildRequests.Any(c => c.ParentRequestId == x.Id))
                    .ToListAsync();
            var removedParents = emptyTouchedParents.Count;
            if (removedParents > 0)
            {
                _tvRequests.Db.TvRequests.RemoveRange(emptyTouchedParents);
                await _tvRequests.Save();
            }

            _logger.LogInformation(
                "Partial TV cleanup request reconciliation for '{Title}': Episodes={EpisodeCount}, Seasons={SeasonCount}, Children={ChildCount}, Parents={ParentCount}",
                record.Title,
                removedEpisodes,
                removedSeasons,
                removedChildren,
                removedParents);
        }

        private async Task RemoveArrSearchCache(MediaCleanupRecord record)
        {
            try
            {
                if (record.RequestType == RequestType.Movie)
                {
                    var matches = await _radarrCache.GetAll()
                        .Where(x => x.TheMovieDbId == record.TheMovieDbId)
                        .ToListAsync();
                    if (matches.Count > 0)
                    {
                        await _radarrCache.DeleteRange(matches);
                        _logger.LogInformation(
                            "Removed {Count} stale Radarr cache row(s) for '{Title}' (TMDB {TmdbId})",
                            matches.Count,
                            record.Title,
                            record.TheMovieDbId);
                    }
                    return;
                }

                if (record.RequestType == RequestType.TvShow)
                {
                    var episodeQuery = _sonarrEpisodeCache.GetAll()
                        .Where(x =>
                            (record.TvDbId > 0 && x.TvDbId == record.TvDbId) ||
                            (record.TheMovieDbId > 0 && x.MovieDbId == record.TheMovieDbId));

                    if (IsPartialTvCleanup(record))
                    {
                        var selected = record.SelectedEpisodes
                            .Select(x => (x.SeasonNumber, x.EpisodeNumber))
                            .ToHashSet();
                        var episodeMatches = await episodeQuery.ToListAsync();
                        episodeMatches = episodeMatches
                            .Where(x => selected.Contains((x.SeasonNumber, x.EpisodeNumber)))
                            .ToList();
                        if (episodeMatches.Count > 0)
                        {
                            await _sonarrEpisodeCache.DeleteRange(episodeMatches);
                            _logger.LogInformation(
                                "Removed {EpisodeCount} stale Sonarr episode cache row(s) for partial cleanup of '{Title}'",
                                episodeMatches.Count,
                                record.Title);
                        }
                        return;
                    }

                    var seriesMatches = await _sonarrCache.GetAll()
                        .Where(x =>
                            (record.TvDbId > 0 && x.TvDbId == record.TvDbId) ||
                            (record.TheMovieDbId > 0 && x.TheMovieDbId == record.TheMovieDbId))
                        .ToListAsync();
                    var allEpisodeMatches = await episodeQuery.ToListAsync();

                    if (allEpisodeMatches.Count > 0)
                    {
                        await _sonarrEpisodeCache.DeleteRange(allEpisodeMatches);
                    }
                    if (seriesMatches.Count > 0)
                    {
                        await _sonarrCache.DeleteRange(seriesMatches);
                    }

                    if (seriesMatches.Count > 0 || allEpisodeMatches.Count > 0)
                    {
                        _logger.LogInformation(
                            "Removed stale Sonarr cache for '{Title}': {SeriesCount} series row(s), {EpisodeCount} episode row(s)",
                            record.Title,
                            seriesMatches.Count,
                            allEpisodeMatches.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                // The destructive *arr operation and Ombi request removal have already succeeded.
                // A stale external cache should never turn that successful cleanup into a false failure.
                _logger.LogWarning(ex,
                    "Could not remove *arr search cache for {RequestType} '{Title}'",
                    record.RequestType,
                    record.Title);
            }
        }

        private async Task RemovePlexAvailabilityCache(MediaCleanupRecord record)
        {
            try
            {
                var plexSettings = await _plexSettings.GetSettingsAsync();
                if (plexSettings?.Enable != true || plexSettings.Servers == null || plexSettings.Servers.Count == 0)
                {
                    return;
                }

                // Plex rating keys and cached rows are not associated with a specific configured
                // Plex server in Ombi. Purging provider matches is therefore only unambiguous when
                // one Plex server is configured. A normal/full media database refresh remains the
                // safe reconciliation path for multi-server installations.
                if (plexSettings.Servers.Count != 1)
                {
                    _logger.LogWarning(
                        "Skipping direct Plex availability cache cleanup for {Title} because {ServerCount} Plex servers are configured",
                        record.Title,
                        plexSettings.Servers.Count);
                    return;
                }

                var mediaType = record.RequestType == RequestType.Movie ? MediaType.Movie : MediaType.Series;
                var tmdbId = record.TheMovieDbId > 0 ? record.TheMovieDbId.ToString() : null;
                var tvdbId = record.TvDbId > 0 ? record.TvDbId.ToString() : null;
                var requestId = record.MediaRequestId;

                var matches = _plexContent.GetWhereContentByCustom(x =>
                        x.Type == mediaType &&
                        ((tmdbId != null && x.TheMovieDbId == tmdbId) ||
                         (tvdbId != null && x.TvDbId == tvdbId) ||
                         x.RequestId == requestId))
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();

                foreach (var id in matches)
                {
                    // GetFirstContentByCustom includes seasons and episodes, which lets the
                    // repository remove the complete cached graph without leaving FK rows behind.
                    var content = await _plexContent.GetFirstContentByCustom(x => x.Id == id);
                    if (content == null)
                    {
                        continue;
                    }

                    if (IsPartialTvCleanup(record))
                    {
                        var selected = record.SelectedEpisodes
                            .Select(x => (x.SeasonNumber, x.EpisodeNumber))
                            .ToHashSet();
                        var episodes = content.Episodes?.OfType<PlexEpisode>()
                            .Where(x => selected.Contains((x.SeasonNumber, x.EpisodeNumber)))
                            .ToList() ?? new List<PlexEpisode>();
                        foreach (var episode in episodes)
                        {
                            await _plexContent.DeleteEpisode(episode);
                        }

                        if (episodes.Count > 0)
                        {
                            _logger.LogInformation(
                                "Removed {EpisodeCount} stale Plex episode cache row(s) for partial cleanup of '{Title}'",
                                episodes.Count,
                                record.Title);
                        }
                    }
                    else
                    {
                        await _plexContent.DeleteContent(content);
                        _logger.LogInformation(
                            "Removed stale Plex availability cache for {RequestType} '{Title}' (Plex content {PlexContentId})",
                            record.RequestType,
                            record.Title,
                            id);
                    }
                }
            }
            catch (Exception ex)
            {
                // The destructive *arr operation and Ombi request removal have already succeeded.
                // A cache-maintenance failure must not turn that successful cleanup into a false
                // failure response; a full media database refresh can reconcile it later.
                _logger.LogWarning(ex,
                    "Could not remove Plex availability cache for {RequestType} '{Title}'",
                    record.RequestType,
                    record.Title);
            }
        }

        private async Task<bool> DeleteMovie(MediaCleanupRecord record)
        {
            var plan = GetDeletionPlan(record);
            var targets = plan.Targets ?? new List<MediaCleanupExternalTarget>();
            if (targets.Count == 0 || targets.Any(x => x.Service == MediaCleanupExternalService.Sonarr))
            {
                throw new MediaCleanupTerminalException("The cleanup's frozen Radarr destination plan is invalid.");
            }

            // Resolve and validate every approved destination before deleting from any of them.
            // That avoids partially executing an old authorization when one endpoint has drifted.
            var approvedSettings = new List<RadarrSettings>();
            foreach (var target in targets)
            {
                RadarrSettings current;
                switch (target.Service)
                {
                    case MediaCleanupExternalService.Radarr:
                        current = await _radarrSettings.GetSettingsAsync();
                        break;
                    case MediaCleanupExternalService.Radarr4K:
                        current = await _radarr4KSettings.GetSettingsAsync();
                        break;
                    default:
                        throw new MediaCleanupTerminalException("The cleanup's frozen Radarr destination plan contains an unsupported target.");
                }

                ValidateExternalTarget(record, target, current.Enabled, current.FullUri);
                approvedSettings.Add(current);
            }

            var deleted = false;
            var deletedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var radarrSettings in approvedSettings)
            {
                deleted |= await DeleteMovieFromRadarr(record.TheMovieDbId, radarrSettings, plan, deletedKeys);
            }

            return deleted;
        }

        private async Task<bool> DeleteMovieFromRadarr(int tmdbId, RadarrSettings radarrSettings, MediaCleanupDeletionPlan plan, HashSet<string> deletedKeys)
        {
            var movies = await _radarr.GetMoviesForCleanup(radarrSettings.ApiKey, radarrSettings.FullUri);
            var matches = movies.Where(x => x.tmdbId == tmdbId).ToList();
            var deleted = false;
            foreach (var movie in matches)
            {
                var key = $"{NormalizeExternalEndpoint(radarrSettings.FullUri)}|{movie.id}";
                if (!deletedKeys.Add(key))
                {
                    continue;
                }
                var deleteSucceeded = await _radarr.DeleteMovie(movie.id, radarrSettings.ApiKey, radarrSettings.FullUri, plan.DeleteFiles, plan.AddImportExclusion);
                if (!deleteSucceeded)
                {
                    throw new MediaCleanupTerminalException($"Radarr rejected the delete request for movie id {movie.id} without an HTTP status.");
                }
                deleted = true;
            }
            return deleted;
        }

        private async Task<bool> DeleteTv(MediaCleanupRecord record)
        {
            var plan = GetDeletionPlan(record);
            var targets = plan.Targets ?? new List<MediaCleanupExternalTarget>();
            if (targets.Count != 1 || targets[0].Service != MediaCleanupExternalService.Sonarr)
            {
                throw new MediaCleanupTerminalException("The cleanup's frozen Sonarr destination plan is invalid.");
            }

            var target = targets[0];
            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();
            ValidateExternalTarget(record, target, sonarrSettings.Enabled, sonarrSettings.FullUri);

            var series = await _sonarr.GetSeriesForCleanup(sonarrSettings.ApiKey, sonarrSettings.FullUri);
            var match = series.FirstOrDefault(x => x.tvdbId == record.TvDbId);
            if (match == null)
            {
                return false;
            }

            if (IsPartialTvCleanup(record))
            {
                if (!plan.DeleteFiles)
                {
                    throw new MediaCleanupTerminalException(
                        "The frozen deletion plan is invalid because partial TV cleanup requires Delete Files.");
                }

                return await DeleteTvEpisodes(record, match, sonarrSettings);
            }

            var deleteSucceeded = await _sonarr.DeleteSeries(match.id, sonarrSettings.ApiKey, sonarrSettings.FullUri, plan.DeleteFiles, plan.AddImportExclusion);
            if (!deleteSucceeded)
            {
                throw new MediaCleanupTerminalException($"Sonarr rejected the delete request for series id {match.id} without an HTTP status.");
            }
            return true;
        }

        private async Task<bool> DeleteTvEpisodes(MediaCleanupRecord record, Ombi.Api.External.ExternalApis.Sonarr.Models.SonarrSeries series, SonarrSettings sonarrSettings)
        {
            // The exact episode keys captured when the cleanup was created are the authorization
            // boundary. SelectedSeasons is informational only and must not cause later downloads
            // in the same season to become implicitly authorized.
            var selectedKeys = record.SelectedEpisodes
                .Select(x => (x.SeasonNumber, x.EpisodeNumber))
                .ToHashSet();
            var episodes = (await _sonarr.GetEpisodesForCleanup(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri)).ToList();
            var selectedEpisodes = episodes
                .Where(x => selectedKeys.Contains((x.seasonNumber, x.episodeNumber)))
                .ToList();

            if (selectedEpisodes.Count == 0)
            {
                return false;
            }

            // A Sonarr episode file can cover multiple episode records (for example S01E01-E02).
            // The selection is expanded when it is created, but re-check here because the file
            // layout could have changed during an approval/voting/grace period. Never delete a
            // physical file if it now contains an episode that was outside the approved scope.
            var selectedFileIds = selectedEpisodes
                .Where(x => x.hasFile && x.episodeFileId > 0)
                .Select(x => x.episodeFileId)
                .Distinct()
                .ToHashSet();
            var collateralEpisodes = episodes
                .Where(x => x.hasFile &&
                            x.episodeFileId > 0 &&
                            selectedFileIds.Contains(x.episodeFileId) &&
                            !selectedKeys.Contains((x.seasonNumber, x.episodeNumber)))
                .OrderBy(x => x.seasonNumber)
                .ThenBy(x => x.episodeNumber)
                .ToList();
            if (collateralEpisodes.Count > 0)
            {
                var examples = string.Join(", ", collateralEpisodes
                    .Take(5)
                    .Select(x => $"S{x.seasonNumber:00}E{x.episodeNumber:00}"));
                throw new MediaCleanupTerminalException(
                    $"Sonarr's episode-file layout changed after this cleanup was created. " +
                    $"Deleting the approved files would also remove unselected episode(s): {examples}. " +
                    "Cancel this cleanup and create a new selection.");
            }

            // Unmonitor first so Sonarr does not immediately search for/re-download a file that
            // Media Cleanup just removed. The bulk monitor endpoint is already used elsewhere in Ombi.
            var episodeIds = selectedEpisodes.Select(x => x.id).Where(x => x > 0).Distinct().ToArray();
            if (episodeIds.Length > 0)
            {
                var monitorResult = await _sonarr.MonitorEpisodeForCleanup(episodeIds, false, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                if (monitorResult == null)
                {
                    throw new InvalidOperationException("Sonarr did not confirm the episode unmonitor request.");
                }
            }

            // Do not unmonitor the whole season for a partial cleanup. SelectedSeasons is inferred
            // from the files present at selection time, so treating it as a future-facing command
            // would silently expand the approved scope to episodes downloaded later.

            var fileIds = selectedFileIds.ToList();
            foreach (var fileId in fileIds)
            {
                var deleteSucceeded = await _sonarr.DeleteEpisodeFile(fileId, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                if (!deleteSucceeded)
                {
                    throw new MediaCleanupTerminalException($"Sonarr rejected the delete request for episode file id {fileId} without an HTTP status.");
                }
            }

            _logger.LogInformation(
                "Media cleanup removed {FileCount} Sonarr episode file(s) covering {EpisodeCount} episode(s) from '{Title}'",
                fileIds.Count,
                selectedEpisodes.Count,
                record.Title);
            return true;
        }

        private async Task<Dictionary<int, long>> GetMovieSizes()
        {
            var result = new Dictionary<int, long>();
            await AddRadarrSizes(await _radarrSettings.GetSettingsAsync(), result);
            await AddRadarrSizes(await _radarr4KSettings.GetSettingsAsync(), result);
            return result;
        }

        private async Task AddRadarrSizes(RadarrSettings settings, Dictionary<int, long> result)
        {
            if (!settings.Enabled)
            {
                return;
            }

            try
            {
                foreach (var movie in await _radarr.GetMovies(settings.ApiKey, settings.FullUri))
                {
                    result[movie.tmdbId] = (result.TryGetValue(movie.tmdbId, out var current) ? current : 0) + movie.sizeOnDisk;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load Radarr sizes for the media cleanup page");
            }
        }

        private async Task<Dictionary<int, long>> GetTvSizes()
        {
            var result = new Dictionary<int, long>();
            var settings = await _sonarrSettings.GetSettingsAsync();
            if (!settings.Enabled)
            {
                return result;
            }

            try
            {
                foreach (var series in await _sonarr.GetSeries(settings.ApiKey, settings.FullUri))
                {
                    // Sonarr v3/v4 returns the aggregate series size in the nested
                    // statistics object. Keep the root-level value as a fallback for
                    // older Sonarr versions/responses.
                    var sizeOnDisk = series.statistics?.sizeOnDisk > 0
                        ? series.statistics.sizeOnDisk
                        : series.sizeOnDisk;
                    result[series.tvdbId] = sizeOnDisk;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load Sonarr sizes for the media cleanup page");
            }

            return result;
        }

        private async Task<PlexContentLookup> GetPlexContentLookup()
        {
            try
            {
                var content = await _plexContent.GetAll().AsNoTracking().ToListAsync();
                return new PlexContentLookup(content);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load Plex content mappings for media cleanup play history");
                return new PlexContentLookup(Array.Empty<PlexServerContent>());
            }
        }

        private async Task PopulateLastPlayed(
            IEnumerable<MediaCleanupItemViewModel> items,
            IReadOnlyDictionary<(RequestType Type, int RequestId), string> plexKeys,
            CancellationToken cancellationToken)
        {
            if (plexKeys.Count == 0)
            {
                return;
            }

            var settings = await _plexSettings.GetSettingsAsync();
            var servers = settings?.Enable == true
                ? settings.Servers?.Where(x => x != null && !string.IsNullOrWhiteSpace(x.PlexAuthToken) && !string.IsNullOrWhiteSpace(x.Ip)).ToList()
                : null;

            // PlexServerContent currently does not record which Plex server supplied a rating key.
            // Querying more than one configured server could therefore match an unrelated ratingKey.
            if (servers == null || servers.Count != 1)
            {
                if (servers?.Count > 1)
                {
                    _logger.LogDebug("Media cleanup last-played lookup is skipped when multiple Plex servers are configured because cached Plex rating keys are not server-scoped");
                }
                return;
            }

            var server = servers[0];
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            using var concurrency = new SemaphoreSlim(6, 6);
            var tasks = items
                .Where(item => plexKeys.ContainsKey((item.RequestType, item.RequestId)))
                .Select(async item =>
                {
                    var entered = false;
                    try
                    {
                        await concurrency.WaitAsync(timeout.Token);
                        entered = true;
                        var key = plexKeys[(item.RequestType, item.RequestId)];
                        var history = await _plex.GetHistory(server.PlexAuthToken, server.FullUri, key, timeout.Token);

                        // A null/default payload is not proof that Plex successfully returned an
                        // empty history. Preserve Unknown unless a real MediaContainer was returned.
                        if (history?.MediaContainer == null)
                        {
                            _logger.LogDebug(
                                "Plex play history returned no usable response for cleanup item {Title}; leaving Last Played as Unknown",
                                item.Title);
                            return;
                        }

                        var latest = history.MediaContainer.Metadata?.FirstOrDefault();

                        // A successful response with an empty Metadata collection means Plex has
                        // positively reported no recorded play for this title.
                        item.LastPlayedKnown = true;
                        if (latest?.viewedAt > 0)
                        {
                            item.LastPlayedAt = DateTimeOffset.FromUnixTimeSeconds(latest.viewedAt.Value).UtcDateTime;
                        }
                    }
                    catch (OperationCanceledException) when (timeout.IsCancellationRequested)
                    {
                        // Leave LastPlayedKnown false. The cleanup page treats this as Unknown.
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        // A stale/missing Plex rating key is not equivalent to "never played".
                        // Keep the value Unknown without escalating an optional enrichment miss.
                        _logger.LogDebug(
                            "Plex play history was not found for cleanup item {Title}; leaving Last Played as Unknown",
                            item.Title);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not load Plex play history for cleanup item {Title}", item.Title);
                    }
                    finally
                    {
                        if (entered)
                        {
                            concurrency.Release();
                        }
                    }
                });

            await Task.WhenAll(tasks);
            if (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Media cleanup last-played lookup reached its 15 second time limit; unresolved titles will remain Unknown");
            }
        }

        private async Task<CleanupTarget> ResolveTarget(RequestType requestType, int requestId)
        {
            if (requestType == RequestType.Movie)
            {
                var movie = await _movieRequests.GetWithUser().FirstOrDefaultAsync(x => x.Id == requestId);
                if (movie == null)
                {
                    return null;
                }

                return new CleanupTarget
                {
                    RequestType = RequestType.Movie,
                    RequestId = movie.Id,
                    Title = movie.Title,
                    PosterPath = movie.PosterPath,
                    TheMovieDbId = movie.TheMovieDbId,
                    Available = movie.Available,
                    AvailableSince = movie.MarkedAsAvailable ?? (movie.RequestedDate == default ? (DateTime?)null : movie.RequestedDate),
                    OwnerUserIds = string.IsNullOrEmpty(movie.RequestedUserId) ? new List<string>() : new List<string> { movie.RequestedUserId }
                };
            }

            if (requestType == RequestType.TvShow)
            {
                var tv = await _tvRequests.GetLite().FirstOrDefaultAsync(x => x.Id == requestId);
                if (tv != null && tv.ChildRequests != null && tv.ChildRequests.Any())
                {
                    return new CleanupTarget
                    {
                        RequestType = RequestType.TvShow,
                        RequestId = tv.Id,
                        Title = tv.Title,
                        PosterPath = tv.PosterPath,
                        TheMovieDbId = tv.ExternalProviderId,
                        TvDbId = tv.TvDbId,
                        Available = tv.ChildRequests.All(x => x.Available),
                        AvailableSince = tv.ChildRequests
                            .Select(x => x.MarkedAsAvailable ?? (x.RequestedDate == default ? (DateTime?)null : x.RequestedDate))
                            .Where(x => x.HasValue)
                            .OrderByDescending(x => x.Value)
                            .FirstOrDefault(),
                        OwnerUserIds = tv.ChildRequests.Select(x => x.RequestedUserId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList()
                    };
                }

                var state = await LoadState();
                var residualRecord = FindResidualTvCatalogRecord(state, requestId);
                if (residualRecord == null || !await HasResidualTvSeries(residualRecord))
                {
                    return null;
                }

                return new CleanupTarget
                {
                    RequestType = RequestType.TvShow,
                    RequestId = residualRecord.MediaRequestId,
                    Title = residualRecord.Title,
                    PosterPath = residualRecord.PosterPath,
                    TheMovieDbId = residualRecord.TheMovieDbId,
                    TvDbId = residualRecord.TvDbId,
                    Available = true,
                    AvailableSince = residualRecord.AvailableSince,
                    OwnerUserIds = (residualRecord.OwnerUserIds ?? new List<string>()).ToList()
                };
            }

            return null;
        }

        private async Task<ResolvedTvCleanupSelection> ResolveTvCleanupSelection(CleanupTarget target, MediaCleanupSelection selection)
        {
            var result = new ResolvedTvCleanupSelection();
            if (target.RequestType != RequestType.TvShow || selection == null || selection.EntireSeries)
            {
                return result;
            }

            var requested = selection.Episodes ?? new List<MediaCleanupEpisodeSelection>();
            var requestedKeys = requested
                .Select(x => (x.SeasonNumber, x.EpisodeNumber))
                .Distinct()
                .ToHashSet();
            if (requestedKeys.Count == 0)
            {
                result.Error = "Select at least one TV episode, or choose Entire series.";
                return result;
            }

            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();
            if (!sonarrSettings.Enabled)
            {
                result.Error = "Sonarr is not enabled, so episode-level cleanup is unavailable.";
                return result;
            }

            try
            {
                var series = (await _sonarr.GetSeries(sonarrSettings.ApiKey, sonarrSettings.FullUri))
                    .FirstOrDefault(x => x.tvdbId == target.TvDbId);
                if (series == null)
                {
                    result.Error = "This series could not be found in Sonarr.";
                    return result;
                }

                var episodes = (await _sonarr.GetEpisodes(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri)).ToList();
                var episodeFiles = (await _sonarr.GetEpisodeFiles(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri)).ToList();
                var fileById = episodeFiles.GroupBy(x => x.id).ToDictionary(x => x.Key, x => x.First());

                var directlySelected = episodes
                    .Where(x => x.hasFile && x.episodeFileId > 0 && requestedKeys.Contains((x.seasonNumber, x.episodeNumber)))
                    .ToList();
                if (directlySelected.Count == 0)
                {
                    result.Error = "None of the selected episodes currently have a file in Sonarr.";
                    return result;
                }

                // Sonarr can represent more than one episode with one physical file (E01-E02).
                // Expand the requested keys by file id so the cleanup scope exactly matches what
                // deleting that file will remove.
                var selectedFileIds = directlySelected.Select(x => x.episodeFileId).Distinct().ToHashSet();
                var expanded = episodes
                    .Where(x => x.hasFile && x.episodeFileId > 0 && selectedFileIds.Contains(x.episodeFileId))
                    .GroupBy(x => (x.seasonNumber, x.episodeNumber))
                    .Select(x => x.First())
                    .OrderBy(x => x.seasonNumber)
                    .ThenBy(x => x.episodeNumber)
                    .ToList();

                result.Episodes = expanded.Select(x => new MediaCleanupEpisodeRecord
                {
                    SeasonNumber = x.seasonNumber,
                    EpisodeNumber = x.episodeNumber,
                    Title = x.title,
                    EpisodeFileId = x.episodeFileId,
                    SizeOnDisk = fileById.TryGetValue(x.episodeFileId, out var file) ? file.size : 0
                }).ToList();

                // Informational only: this records seasons for which every file-bearing episode was
                // selected at this moment. Destructive execution must use result.Episodes as the
                // frozen authorization boundary and must never authorize by SelectedSeasons.
                result.SelectedSeasons = episodes
                    .Where(x => x.hasFile && x.episodeFileId > 0)
                    .GroupBy(x => x.seasonNumber)
                    .Where(season => season.Select(x => x.episodeFileId).Distinct().All(selectedFileIds.Contains))
                    .Select(x => x.Key)
                    .OrderBy(x => x)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not validate the selected Sonarr episodes for '{Title}'", target.Title);
                result.Error = "Ombi could not validate the selected episodes with Sonarr.";
                return result;
            }
        }

        private static void ApplyTvSelection(MediaCleanupRecord record, ResolvedTvCleanupSelection selection)
        {
            if (record.RequestType != RequestType.TvShow || selection == null || selection.Episodes.Count == 0)
            {
                return;
            }

            record.SelectedEpisodes = selection.Episodes;
            record.SelectedSeasons = selection.SelectedSeasons;
            record.SizeOnDisk = selection.Episodes
                .GroupBy(x => x.EpisodeFileId)
                .Sum(x => x.First().SizeOnDisk);
        }

        private MediaCleanupRecord CreateRecord(CleanupTarget target, string userId, MediaCleanupOrigin origin)
        {
            return new MediaCleanupRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RequestType = target.RequestType,
                MediaRequestId = target.RequestId,
                Title = target.Title,
                PosterPath = target.PosterPath,
                TheMovieDbId = target.TheMovieDbId,
                TvDbId = target.TvDbId,
                AvailableSince = target.AvailableSince,
                RequestedByUserId = userId,
                OwnerUserIds = target.OwnerUserIds,
                Origin = origin,
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task<string> TrySnapshotDeletionPlan(MediaCleanupRecord record, MediaCleanupSettings settings)
        {
            try
            {
                record.DeletionPlan = await CreateDeletionPlan(record, settings);
                return null;
            }
            catch (MediaCleanupTerminalException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Could not snapshot destructive settings/destination for {RequestType} '{Title}' ({CleanupId})",
                    record.RequestType,
                    record.Title,
                    record.Id);
                return "Ombi could not freeze the destructive settings and external destination for this cleanup. No media was deleted.";
            }
        }

        private async Task<MediaCleanupDeletionPlan> CreateDeletionPlan(MediaCleanupRecord record, MediaCleanupSettings settings)
        {
            if (record.RequestType == RequestType.TvShow && IsPartialTvCleanup(record) && !settings.DeleteFiles)
            {
                throw new MediaCleanupTerminalException(
                    "Specific TV episode cleanup requires Delete Files to be enabled when deletion is authorized.");
            }

            var authorization = await CaptureAuthorizationSnapshot(record);
            var plan = new MediaCleanupDeletionPlan
            {
                DeleteFiles = settings.DeleteFiles,
                AddImportExclusion = settings.AddImportExclusion,
                AuthorizedAt = DateTime.UtcNow,
                Authorization = authorization
            };

            // Keep the record's owner display/audit field aligned with the state that was
            // actually authorized, rather than the potentially older nomination snapshot.
            record.OwnerUserIds = authorization.OwnerUserIds.ToList();

            if (record.RequestType == RequestType.Movie)
            {
                var regular = await _radarrSettings.GetSettingsAsync();
                if (regular.Enabled)
                {
                    plan.Targets.Add(new MediaCleanupExternalTarget
                    {
                        Service = MediaCleanupExternalService.Radarr,
                        Endpoint = NormalizeExternalEndpoint(regular.FullUri)
                    });
                }

                var fourK = await _radarr4KSettings.GetSettingsAsync();
                if (fourK.Enabled)
                {
                    plan.Targets.Add(new MediaCleanupExternalTarget
                    {
                        Service = MediaCleanupExternalService.Radarr4K,
                        Endpoint = NormalizeExternalEndpoint(fourK.FullUri)
                    });
                }

                if (plan.Targets.Count == 0)
                {
                    throw new MediaCleanupTerminalException("No Radarr instance is enabled, so this cleanup cannot be authorized for deletion.");
                }

                return plan;
            }

            if (record.RequestType == RequestType.TvShow)
            {
                var sonarr = await _sonarrSettings.GetSettingsAsync();
                if (!sonarr.Enabled)
                {
                    throw new MediaCleanupTerminalException("Sonarr is not enabled, so this cleanup cannot be authorized for deletion.");
                }

                plan.Targets.Add(new MediaCleanupExternalTarget
                {
                    Service = MediaCleanupExternalService.Sonarr,
                    Endpoint = NormalizeExternalEndpoint(sonarr.FullUri)
                });
                return plan;
            }

            throw new MediaCleanupTerminalException($"Unsupported cleanup request type: {record.RequestType}.");
        }

        private async Task EvaluateCommunityAndSnapshotDeletionPlan(MediaCleanupRecord record, MediaCleanupSettings settings, DateTime now)
        {
            var wasScheduled = record.Status == MediaCleanupStatus.ScheduledForDeletion;
            EvaluateCommunity(record, settings, now);

            if (!wasScheduled &&
                record.Status == MediaCleanupStatus.ScheduledForDeletion &&
                !record.ExternalDeletionCompletedAt.HasValue &&
                record.DeletionPlan == null)
            {
                var error = await TrySnapshotDeletionPlan(record, settings);
                if (!string.IsNullOrEmpty(error))
                {
                    MarkTerminalFailure(record, new MediaCleanupTerminalException(error), "deletion authorization");
                }
            }
        }

        private async Task<MediaCleanupAuthorizationSnapshot> CaptureAuthorizationSnapshot(MediaCleanupRecord record)
        {
            if (record.RequestType == RequestType.Movie)
            {
                var requests = await _movieRequests.GetAll()
                    .Where(x => x.Id == record.MediaRequestId ||
                                (record.TheMovieDbId > 0 && x.TheMovieDbId == record.TheMovieDbId))
                    .ToListAsync();

                var idConflict = requests.FirstOrDefault(x =>
                    x.Id == record.MediaRequestId &&
                    record.TheMovieDbId > 0 &&
                    x.TheMovieDbId != record.TheMovieDbId);
                if (idConflict != null)
                {
                    throw new MediaCleanupTerminalException(
                        "The Ombi movie request now points at a different provider identity. Create/approve a new cleanup request.");
                }

                var matching = requests
                    .Where(x => record.TheMovieDbId <= 0 || x.TheMovieDbId == record.TheMovieDbId)
                    .ToList();
                if (matching.Count == 0)
                {
                    throw new MediaCleanupTerminalException(
                        "The Ombi movie request no longer exists. Create/approve a new cleanup request before deleting media.");
                }
                if (!matching.Any(x => x.Available))
                {
                    throw new MediaCleanupTerminalException(
                        "The Ombi movie request is no longer marked available. Create/approve a new cleanup request before deleting media.");
                }

                return new MediaCleanupAuthorizationSnapshot
                {
                    OwnerUserIds = matching
                        .Select(x => x.RequestedUserId)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(x => x, StringComparer.Ordinal)
                        .ToList(),
                    RequestClaims = matching
                        .Select(x => $"movie:{x.Id}:{x.RequestedUserId ?? string.Empty}")
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(x => x, StringComparer.Ordinal)
                        .ToList()
                };
            }

            if (record.RequestType == RequestType.TvShow)
            {
                var parents = await _tvRequests.Get()
                    .Where(x => x.Id == record.MediaRequestId ||
                                (record.TvDbId > 0 && x.TvDbId == record.TvDbId) ||
                                (record.TheMovieDbId > 0 && x.ExternalProviderId == record.TheMovieDbId))
                    .ToListAsync();

                var idConflict = parents.FirstOrDefault(x =>
                    x.Id == record.MediaRequestId &&
                    ((record.TvDbId > 0 && x.TvDbId > 0 && x.TvDbId != record.TvDbId) ||
                     (record.TheMovieDbId > 0 && x.ExternalProviderId > 0 && x.ExternalProviderId != record.TheMovieDbId)));
                if (idConflict != null)
                {
                    throw new MediaCleanupTerminalException(
                        "The Ombi TV request now points at a different provider identity. Create/approve a new cleanup request.");
                }

                var matching = parents
                    .Where(x =>
                        (record.TvDbId > 0 && x.TvDbId == record.TvDbId) ||
                        (record.TheMovieDbId > 0 && x.ExternalProviderId == record.TheMovieDbId) ||
                        x.Id == record.MediaRequestId)
                    .ToList();
                var children = matching
                    .SelectMany(x => x.ChildRequests ?? new List<ChildRequests>())
                    .ToList();

                if (children.Count == 0)
                {
                    // After a completed partial cleanup Ombi can intentionally keep a residual
                    // Sonarr-backed catalog entry even when no request rows remain. Preserve that
                    // supported workflow, but only while the same residual series still exists.
                    var state = await LoadState();
                    var residual = FindResidualTvCatalogRecord(state, record.MediaRequestId);
                    if (residual != null &&
                        IsSameCleanupMedia(residual, record.RequestType, record.MediaRequestId, record.TheMovieDbId, record.TvDbId) &&
                        await HasResidualTvSeries(residual))
                    {
                        return new MediaCleanupAuthorizationSnapshot
                        {
                            ResidualTvCatalog = true,
                            OwnerUserIds = (record.OwnerUserIds ?? new List<string>())
                                .Where(x => !string.IsNullOrEmpty(x))
                                .Distinct(StringComparer.Ordinal)
                                .OrderBy(x => x, StringComparer.Ordinal)
                                .ToList()
                        };
                    }

                    throw new MediaCleanupTerminalException(
                        "The Ombi TV request no longer exists. Create/approve a new cleanup request before deleting media.");
                }

                if (children.Any(x => !x.Available))
                {
                    throw new MediaCleanupTerminalException(
                        "The Ombi TV request changed and is no longer fully available. Create/approve a new cleanup request before deleting media.");
                }

                var claims = new List<string>();
                foreach (var parent in matching)
                {
                    foreach (var child in parent.ChildRequests ?? new List<ChildRequests>())
                    {
                        claims.Add($"tv-child:{parent.Id}:{child.Id}:{child.RequestedUserId ?? string.Empty}");
                        foreach (var season in child.SeasonRequests ?? new List<SeasonRequests>())
                        {
                            foreach (var episode in season.Episodes ?? new List<EpisodeRequests>())
                            {
                                if (episode.Requested || episode.Approved)
                                {
                                    claims.Add($"tv-episode:{child.Id}:{season.SeasonNumber}:{episode.EpisodeNumber}");
                                }
                            }
                        }
                    }
                }

                return new MediaCleanupAuthorizationSnapshot
                {
                    OwnerUserIds = children
                        .Select(x => x.RequestedUserId)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(x => x, StringComparer.Ordinal)
                        .ToList(),
                    RequestClaims = claims
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(x => x, StringComparer.Ordinal)
                        .ToList()
                };
            }

            throw new MediaCleanupTerminalException($"Unsupported cleanup request type: {record.RequestType}.");
        }

        private async Task RevalidateDeletionAuthorization(MediaCleanupRecord record)
        {
            var plan = GetDeletionPlan(record);
            var approved = plan.Authorization;
            var current = await CaptureAuthorizationSnapshot(record);

            var approvedClaims = new HashSet<string>(approved.RequestClaims ?? new List<string>(), StringComparer.Ordinal);
            var currentClaims = new HashSet<string>(current.RequestClaims ?? new List<string>(), StringComparer.Ordinal);
            var approvedOwners = new HashSet<string>(approved.OwnerUserIds ?? new List<string>(), StringComparer.Ordinal);
            var currentOwners = new HashSet<string>(current.OwnerUserIds ?? new List<string>(), StringComparer.Ordinal);

            if (approved.ResidualTvCatalog != current.ResidualTvCatalog)
            {
                throw new MediaCleanupTerminalException(
                    "The Ombi request state changed after this cleanup was authorized. Create/approve a new cleanup request before deleting media.");
            }

            // Removing an owner/request narrows the authorization and is safe. Adding a claim or
            // owner expands or changes what the old approval would affect and requires reapproval.
            var addedClaims = currentClaims.Except(approvedClaims, StringComparer.Ordinal).ToList();
            var addedOwners = currentOwners.Except(approvedOwners, StringComparer.Ordinal).ToList();
            if (addedClaims.Count > 0 || addedOwners.Count > 0)
            {
                _logger.LogWarning(
                    "Media cleanup authorization became stale for {RequestType} '{Title}' ({CleanupId}); {AddedClaimCount} new request claim(s), {AddedOwnerCount} new owner(s)",
                    record.RequestType,
                    record.Title,
                    record.Id,
                    addedClaims.Count,
                    addedOwners.Count);
                throw new MediaCleanupTerminalException(
                    "The Ombi request or ownership state changed after this cleanup was authorized. Create/approve a new cleanup request before deleting media.");
            }

            if (!approved.ResidualTvCatalog && approvedClaims.Count > 0 && currentClaims.Count == 0)
            {
                throw new MediaCleanupTerminalException(
                    "The Ombi request that authorized this cleanup no longer exists. Create/approve a new cleanup request before deleting media.");
            }
        }

        private static MediaCleanupDeletionPlan GetDeletionPlan(MediaCleanupRecord record)
        {
            if (record.DeletionPlan == null ||
                record.DeletionPlan.Version != 2 ||
                record.DeletionPlan.Authorization == null)
            {
                throw new MediaCleanupTerminalException(
                    "This cleanup does not contain a supported frozen deletion plan and authorization snapshot. Create/approve a new cleanup request before deleting media.");
            }

            return record.DeletionPlan;
        }

        private void ValidateExternalTarget(MediaCleanupRecord record, MediaCleanupExternalTarget target, bool enabled, string currentEndpoint)
        {
            var approvedEndpoint = NormalizeExternalEndpoint(target.Endpoint);
            var normalizedCurrent = NormalizeExternalEndpoint(currentEndpoint);
            if (!enabled ||
                string.IsNullOrEmpty(approvedEndpoint) ||
                !string.Equals(approvedEndpoint, normalizedCurrent, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Media cleanup destination changed for {RequestType} '{Title}' ({CleanupId}). Service={Service}, ApprovedEndpoint={ApprovedEndpoint}, CurrentEndpoint={CurrentEndpoint}, Enabled={Enabled}",
                    record.RequestType,
                    record.Title,
                    record.Id,
                    target.Service,
                    approvedEndpoint,
                    normalizedCurrent,
                    enabled);
                throw new MediaCleanupTerminalException(
                    $"The configured {target.Service} destination changed after this cleanup was authorized. " +
                    "Create/approve a new cleanup request before deleting media.");
            }
        }

        private static string NormalizeExternalEndpoint(string endpoint)
        {
            return string.IsNullOrWhiteSpace(endpoint)
                ? string.Empty
                : endpoint.Trim().TrimEnd('/');
        }

        private void EvaluateCommunity(MediaCleanupRecord record, MediaCleanupSettings settings, DateTime now)
        {
            // Once external deletion has happened, voting/approval can no longer change the
            // outcome. The only valid transition is to finish Ombi reconciliation.
            if (record.ExternalDeletionCompletedAt.HasValue ||
                record.Origin != MediaCleanupOrigin.Community ||
                !IsActive(record))
            {
                return;
            }

            if (settings.CommunityCleanup == CommunityCleanupMode.Off)
            {
                return;
            }

            // The configured voting period is a minimum voting window, not merely a
            // deadline by which the threshold must be reached. Never advance a
            // community cleanup request to approval or deletion before VotingEndsAt,
            // even when the current votes already satisfy the threshold/margin.
            //
            // This also repairs requests that an older build advanced early: while
            // their original voting window is still open, put them back into Voting.
            if (record.VotingEndsAt.HasValue && record.VotingEndsAt.Value > now)
            {
                record.Status = MediaCleanupStatus.Voting;
                record.ScheduledForDeletionAt = null;
                record.ApprovedByUserId = null;
                return;
            }

            // Voting has ended. Evaluate the final vote snapshot exactly once the
            // configured window has elapsed. Votes can no longer change after this.
            var keepVotes = record.Votes.Count(x => x.Vote == MediaCleanupVoteType.Keep);
            var deleteVotes = record.Votes.Count(x => x.Vote == MediaCleanupVoteType.Delete);
            var requesterVeto = settings.RequesterCanVeto && record.Votes.Any(x =>
                x.Vote == MediaCleanupVoteType.Keep && record.OwnerUserIds.Contains(x.UserId));
            var anyKeepVoteVeto = settings.AnyKeepVotePreventsRemoval && keepVotes > 0;
            var thresholdMet = !requesterVeto &&
                               !anyKeepVoteVeto &&
                               deleteVotes >= Math.Max(1, settings.MinimumDeleteVotes) &&
                               deleteVotes - keepVotes >= Math.Max(0, settings.RequiredVoteMargin);

            if (!thresholdMet)
            {
                record.ApprovedByUserId = null;
                record.Status = MediaCleanupStatus.Rejected;
                record.ScheduledForDeletionAt = null;
                return;
            }

            if (settings.CommunityCleanup == CommunityCleanupMode.AutomaticAfterThreshold)
            {
                if (record.Status != MediaCleanupStatus.ScheduledForDeletion)
                {
                    record.Status = MediaCleanupStatus.ScheduledForDeletion;
                    record.ScheduledForDeletionAt = TruncateToSecond(now).AddDays(Math.Max(0, settings.GracePeriodDays));
                    ResetRetryState(record);
                }
                return;
            }

            if (settings.CommunityCleanup == CommunityCleanupMode.AdminApproval)
            {
                if (!string.IsNullOrEmpty(record.ApprovedByUserId))
                {
                    record.Status = MediaCleanupStatus.ScheduledForDeletion;
                }
                else
                {
                    record.Status = MediaCleanupStatus.PendingAdminApproval;
                    record.ScheduledForDeletionAt = null;
                }
            }
        }

        private MediaCleanupRequestViewModel ToViewModel(
            MediaCleanupRecord record,
            string userId,
            MediaCleanupSettings settings,
            IReadOnlyDictionary<string, string> voterDisplayNames)
        {
            if (record == null)
            {
                return null;
            }

            return new MediaCleanupRequestViewModel
            {
                Id = record.Id,
                Origin = record.Origin,
                Status = record.Status,
                KeepVotes = record.Votes.Count(x => x.Vote == MediaCleanupVoteType.Keep),
                DeleteVotes = record.Votes.Count(x => x.Vote == MediaCleanupVoteType.Delete),
                RequesterVeto = settings.RequesterCanVeto && record.Votes.Any(x => x.Vote == MediaCleanupVoteType.Keep && record.OwnerUserIds.Contains(x.UserId)),
                MyVote = record.Votes.FirstOrDefault(x => x.UserId == userId)?.Vote,
                CreatedAt = record.CreatedAt,
                VotingEndsAt = record.VotingEndsAt,
                ScheduledForDeletionAt = record.ScheduledForDeletionAt,
                ExternalDeletionCompletedAt = record.ExternalDeletionCompletedAt,
                RetryCount = record.RetryCount,
                LastFailureAt = record.LastFailureAt,
                NextRetryAt = record.NextRetryAt,
                FailureReason = record.FailureReason,
                EntireSeries = !IsPartialTvCleanup(record),
                ScopeLabel = BuildCleanupScopeLabel(record),
                SelectedEpisodeCount = record.SelectedEpisodes?.Count ?? 0,
                SelectedSizeOnDisk = IsPartialTvCleanup(record) ? record.SizeOnDisk : 0,
                SelectedEpisodes = record.SelectedEpisodes?
                    .Select(x => new MediaCleanupEpisodeSelection
                    {
                        SeasonNumber = x.SeasonNumber,
                        EpisodeNumber = x.EpisodeNumber
                    })
                    .ToList() ?? new List<MediaCleanupEpisodeSelection>(),
                SelectedSeasons = record.SelectedSeasons?.ToList() ?? new List<int>(),
                Voters = voterDisplayNames == null
                    ? new List<MediaCleanupVoterViewModel>()
                    : record.Votes
                        .OrderBy(x => x.Date)
                        .Select(x => new MediaCleanupVoterViewModel
                        {
                            DisplayName = voterDisplayNames.TryGetValue(x.UserId, out var displayName)
                                ? displayName
                                : "Former or unknown user",
                            Vote = x.Vote,
                            Date = x.Date,
                            IsRequester = record.OwnerUserIds.Contains(x.UserId)
                        })
                        .ToList()
            };
        }

        private static string BuildCleanupScopeLabel(MediaCleanupRecord record)
        {
            if (record.RequestType == RequestType.Movie)
            {
                return "Entire movie";
            }
            if (!IsPartialTvCleanup(record))
            {
                return "Entire series";
            }

            var selectedEpisodes = record.SelectedEpisodes ?? new List<MediaCleanupEpisodeRecord>();
            var seasons = selectedEpisodes
                .Select(x => x.SeasonNumber)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            var fullSeasons = (record.SelectedSeasons ?? new List<int>())
                .Where(seasons.Contains)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            var partialEpisodes = selectedEpisodes
                .Where(x => !fullSeasons.Contains(x.SeasonNumber))
                .ToList();

            string SeasonLabel(int seasonNumber) => seasonNumber == 0 ? "Specials" : $"Season {seasonNumber}";

            if (partialEpisodes.Count == 0 && fullSeasons.Count > 0)
            {
                if (fullSeasons.Count == 1)
                {
                    return SeasonLabel(fullSeasons[0]);
                }

                if (fullSeasons.Contains(0))
                {
                    var numbered = fullSeasons.Where(x => x != 0).Select(x => x.ToString());
                    return $"Specials + Seasons {string.Join(", ", numbered)}";
                }

                return $"Seasons {string.Join(", ", fullSeasons)}";
            }

            var partialSeasonCount = partialEpisodes.Select(x => x.SeasonNumber).Distinct().Count();
            var partialLabel = partialSeasonCount == 1
                ? $"{partialEpisodes.Count} episode{(partialEpisodes.Count == 1 ? string.Empty : "s")} from {SeasonLabel(partialEpisodes[0].SeasonNumber)}"
                : $"{partialEpisodes.Count} episode{(partialEpisodes.Count == 1 ? string.Empty : "s")} across {partialSeasonCount} seasons";

            if (fullSeasons.Count == 1)
            {
                return $"{SeasonLabel(fullSeasons[0])} + {partialLabel}";
            }
            if (fullSeasons.Count > 1)
            {
                return $"{fullSeasons.Count} full seasons + {partialLabel}";
            }

            return partialLabel;
        }

        private async Task<MediaCleanupState> LoadState()
        {
            _state.ClearCache();
            var state = await _state.GetSettingsAsync() ?? new MediaCleanupState();
            state.Requests ??= new List<MediaCleanupRecord>();
            foreach (var request in state.Requests)
            {
                request.OwnerUserIds ??= new List<string>();
                request.SelectedEpisodes ??= new List<MediaCleanupEpisodeRecord>();
                request.SelectedSeasons ??= new List<int>();
                request.Votes ??= new List<MediaCleanupVoteRecord>();
            }
            return state;
        }

        private async Task<bool> TrySaveState(MediaCleanupState state, string operation)
        {
            if (await _state.SaveSettingsAsync(state))
            {
                return true;
            }

            _logger.LogError(
                "Media Cleanup state persistence returned false while {Operation}; the in-memory transition will not be reported as successful",
                operation);
            return false;
        }

        private static MediaCleanupActionResult PersistenceFailure(string cleanupRequestId = null)
        {
            return Fail(
                "Media Cleanup could not persist this change. The action was not confirmed; please try again.",
                cleanupRequestId);
        }

        private async Task SaveStateCheckpoint(MediaCleanupState state)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var checkpointState = scope.ServiceProvider.GetRequiredService<ISettingsService<MediaCleanupState>>();
            checkpointState.ClearCache();
            if (!await checkpointState.SaveSettingsAsync(state))
            {
                throw new InvalidOperationException("Could not persist the Media Cleanup recovery checkpoint.");
            }
        }

        private static string GetRequesterDisplayName(OmbiUser requestedUser, string legacyRequestedByAlias, bool canSeeAliases)
        {
            if (requestedUser != null)
            {
                if (canSeeAliases && !string.IsNullOrWhiteSpace(requestedUser.Alias))
                {
                    return requestedUser.Alias;
                }

                // User aliases are administrator-assigned private labels. Normal cleanup
                // users should only receive the account/Plex username, matching the rest
                // of the non-moderation WebUI.
                return requestedUser.UserName ?? string.Empty;
            }

            // Older request rows can contain only RequestedByAlias. That value may be an
            // administrator-assigned nickname, so never expose it to a non-manager when
            // the underlying Ombi user can no longer be resolved.
            return canSeeAliases ? legacyRequestedByAlias ?? string.Empty : string.Empty;
        }

        private async Task<CleanupPermissions> GetPermissions(OmbiUser user)
        {
            var admin = await _userManager.IsInRoleAsync(user, OmbiRoles.Admin);
            var powerUser = await _userManager.IsInRoleAsync(user, OmbiRoles.PowerUser);
            var privileged = admin || powerUser;
            return new CleanupPermissions
            {
                CanRequestRemoval = privileged || await _userManager.IsInRoleAsync(user, OmbiRoles.RequestMediaRemoval),
                CanDeleteOwnMedia = privileged || await _userManager.IsInRoleAsync(user, OmbiRoles.DeleteOwnMedia),
                CanVote = privileged || await _userManager.IsInRoleAsync(user, OmbiRoles.VoteOnMediaCleanup),
                CanManage = privileged || await _userManager.IsInRoleAsync(user, OmbiRoles.ManageMediaCleanup)
            };
        }

        private static bool CanUseOwnRemoval(MediaCleanupSettings settings, CleanupPermissions permissions)
        {
            if (settings.OwnRequestRemoval == OwnRequestRemovalMode.Off || !permissions.CanRequestRemoval)
            {
                return false;
            }
            return settings.OwnRequestRemoval != OwnRequestRemovalMode.ImmediateDeletion || permissions.CanDeleteOwnMedia;
        }

        private static bool CanNominateUnderRestriction(
            MediaCleanupSettings settings,
            CleanupPermissions permissions,
            bool requestedByCurrentUser,
            bool isCleanupSteward)
        {
            return !settings.RestrictNominationsToOwnRequests ||
                   permissions.CanManage ||
                   requestedByCurrentUser ||
                   isCleanupSteward;
        }

        /// <summary>
        /// A Keep vote on a community cleanup that ultimately leaves the media in place grants
        /// cleanup stewardship for that title. Stewardship is derived from persisted cleanup
        /// history rather than stored separately, so existing/rejected cleanup votes immediately
        /// participate after upgrading without a migration.
        ///
        /// The user's latest vote from a rejected cleanup is their current stewardship position:
        /// Keep grants/retains stewardship; Delete gives it up. A later successful full-media
        /// cleanup resets the old lifecycle so a future re-request of the same provider title does
        /// not inherit stewardship from before it was deleted. Partial TV cleanups do not reset
        /// series stewardship because the series still remains in the cleanup catalog.
        ///
        /// When stewardship is active, return the effective date of the Keep vote that currently
        /// establishes it. The UI uses this to explain why the title appears under My Stewardship.
        /// </summary>
        private static DateTime? GetCleanupStewardshipSince(
            MediaCleanupState state,
            RequestType requestType,
            int requestId,
            int theMovieDbId,
            int tvDbId,
            string userId)
        {
            if (state?.Requests == null || string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var matchingHistory = state.Requests
                .Where(x => x != null && IsSameCleanupMedia(x, requestType, requestId, theMovieDbId, tvDbId))
                .ToList();

            if (matchingHistory.Count == 0)
            {
                return null;
            }

            var lifecycleResetAt = matchingHistory
                .Where(x => x.Status == MediaCleanupStatus.Completed && !IsPartialTvCleanup(x))
                .Select(x => (DateTime?)(x.CompletedAt ?? x.ScheduledForDeletionAt ?? x.CreatedAt))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            var latestClosedVote = matchingHistory
                .Where(x => x.Origin == MediaCleanupOrigin.Community && x.Status == MediaCleanupStatus.Rejected)
                .SelectMany(x => (x.Votes ?? new List<MediaCleanupVoteRecord>())
                    .Where(v => string.Equals(v.UserId, userId, StringComparison.Ordinal))
                    .Select(v => new
                    {
                        Vote = v.Vote,
                        Date = v.Date == default ? x.VotingEndsAt ?? x.CreatedAt : v.Date,
                        RecordCreatedAt = x.CreatedAt
                    }))
                .Where(x => !lifecycleResetAt.HasValue || x.Date > lifecycleResetAt.Value)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.RecordCreatedAt)
                .FirstOrDefault();

            if (latestClosedVote?.Vote != MediaCleanupVoteType.Keep)
            {
                return null;
            }

            return latestClosedVote.Date;
        }

        private static bool IsSameCleanupMedia(
            MediaCleanupRecord record,
            RequestType requestType,
            int requestId,
            int theMovieDbId,
            int tvDbId)
        {
            if (record.RequestType != requestType)
            {
                return false;
            }

            if (requestId > 0 && record.MediaRequestId == requestId)
            {
                return true;
            }

            if (requestType == RequestType.Movie)
            {
                return theMovieDbId > 0 && record.TheMovieDbId == theMovieDbId;
            }

            if (requestType == RequestType.TvShow)
            {
                return (tvDbId > 0 && record.TvDbId == tvDbId) ||
                       (theMovieDbId > 0 && record.TheMovieDbId == theMovieDbId);
            }

            return false;
        }

        private static bool CanSeeItem(MediaCleanupRecord cleanup, bool owned, MediaCleanupSettings settings, CleanupPermissions permissions, string userId)
        {
            if (cleanup != null && (permissions.CanManage || cleanup.RequestedByUserId == userId ||
                                    (cleanup.Origin == MediaCleanupOrigin.Community && permissions.CanVote)))
            {
                return true;
            }

            if (owned && CanUseOwnRemoval(settings, permissions))
            {
                return true;
            }

            return settings.CommunityCleanup != CommunityCleanupMode.Off && permissions.CanVote;
        }

        private static bool IsOriginEnabled(MediaCleanupRecord record, MediaCleanupSettings settings)
        {
            return record.Origin == MediaCleanupOrigin.OwnRequest
                ? settings.OwnRequestRemoval != OwnRequestRemovalMode.Off
                : settings.CommunityCleanup != CommunityCleanupMode.Off;
        }

        private static string GetTvCleanupIdentityKey(MediaCleanupRecord record)
        {
            if (record.TvDbId > 0)
            {
                return $"tvdb:{record.TvDbId}";
            }
            if (record.TheMovieDbId > 0)
            {
                return $"tmdb:{record.TheMovieDbId}";
            }
            return $"request:{record.MediaRequestId}";
        }

        private static MediaCleanupRecord FindResidualTvCatalogRecord(MediaCleanupState state, int requestId)
        {
            var latestCompleted = state.Requests
                .Where(x => x.RequestType == RequestType.TvShow &&
                            x.MediaRequestId == requestId &&
                            x.Status == MediaCleanupStatus.Completed)
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .FirstOrDefault();

            return IsPartialTvCleanup(latestCompleted) ? latestCompleted : null;
        }

        private async Task<bool> HasResidualTvSeries(MediaCleanupRecord record)
        {
            return await _sonarrCache.GetAll()
                .AsNoTracking()
                .AnyAsync(x =>
                    (record.TvDbId > 0 && x.TvDbId == record.TvDbId) ||
                    (record.TheMovieDbId > 0 && x.TheMovieDbId == record.TheMovieDbId));
        }

        private static bool IsAgeEligible(DateTime? availableSince, int minimumDays, DateTime now)
        {
            if (minimumDays <= 0)
            {
                return true;
            }
            return availableSince.HasValue && availableSince.Value <= now.AddDays(-minimumDays);
        }

        private static bool IsPartialTvCleanup(MediaCleanupRecord record)
        {
            return record?.RequestType == RequestType.TvShow && record.SelectedEpisodes?.Count > 0;
        }

        private static bool IsActive(MediaCleanupRecord record)
        {
            return record.Status == MediaCleanupStatus.Voting ||
                   record.Status == MediaCleanupStatus.PendingAdminApproval ||
                   record.Status == MediaCleanupStatus.ScheduledForDeletion;
        }

        private static bool IsVoteable(MediaCleanupRecord record)
        {
            return record.Status == MediaCleanupStatus.Voting &&
                   (!record.VotingEndsAt.HasValue || record.VotingEndsAt.Value > DateTime.UtcNow);
        }

        private static MediaCleanupRecord FindActive(MediaCleanupState state, CleanupTarget target)
        {
            if (target == null)
            {
                return null;
            }

            return FindActiveForMedia(
                state?.Requests?.Where(IsActive),
                target.RequestType,
                target.RequestId,
                target.TheMovieDbId,
                target.TvDbId);
        }

        private static MediaCleanupRecord FindActiveForMedia(
            IEnumerable<MediaCleanupRecord> records,
            RequestType requestType,
            int requestId,
            int theMovieDbId,
            int tvDbId)
        {
            return records?
                .Where(x => x != null && IsSameCleanupMedia(x, requestType, requestId, theMovieDbId, tvDbId))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();
        }

        private static void TrackRepresentedCleanup(HashSet<string> representedCleanupIds, MediaCleanupRecord cleanup)
        {
            if (!string.IsNullOrEmpty(cleanup?.Id))
            {
                representedCleanupIds.Add(cleanup.Id);
            }
        }

        private static MediaCleanupActionResult Success(string message, string id = null)
        {
            return new MediaCleanupActionResult { Result = true, Message = message, CleanupRequestId = id };
        }

        private static MediaCleanupActionResult Fail(string message, string id = null)
        {
            return new MediaCleanupActionResult { Result = false, Message = message, CleanupRequestId = id };
        }

        private sealed class PlexContentLookup
        {
            private readonly Dictionary<string, string> _movieByTmdb;
            private readonly Dictionary<string, string> _movieByImdb;
            private readonly Dictionary<string, string> _seriesByTvdb;
            private readonly Dictionary<string, string> _seriesByTmdb;
            private readonly Dictionary<string, string> _seriesByImdb;

            public PlexContentLookup(IEnumerable<PlexServerContent> content)
            {
                var items = content?.Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList() ?? new List<PlexServerContent>();
                _movieByTmdb = Build(items.Where(x => x.Type == MediaType.Movie), x => x.TheMovieDbId);
                _movieByImdb = Build(items.Where(x => x.Type == MediaType.Movie), x => x.ImdbId);
                _seriesByTvdb = Build(items.Where(x => x.Type == MediaType.Series), x => x.TvDbId);
                _seriesByTmdb = Build(items.Where(x => x.Type == MediaType.Series), x => x.TheMovieDbId);
                _seriesByImdb = Build(items.Where(x => x.Type == MediaType.Series), x => x.ImdbId);
            }

            public string FindMovie(int tmdbId, string imdbId)
            {
                if (tmdbId > 0 && _movieByTmdb.TryGetValue(tmdbId.ToString(), out var key))
                {
                    return key;
                }
                return !string.IsNullOrWhiteSpace(imdbId) && _movieByImdb.TryGetValue(imdbId, out key) ? key : null;
            }

            public string FindSeries(int tvdbId, int tmdbId, string imdbId)
            {
                if (tvdbId > 0 && _seriesByTvdb.TryGetValue(tvdbId.ToString(), out var key))
                {
                    return key;
                }
                if (tmdbId > 0 && _seriesByTmdb.TryGetValue(tmdbId.ToString(), out key))
                {
                    return key;
                }
                return !string.IsNullOrWhiteSpace(imdbId) && _seriesByImdb.TryGetValue(imdbId, out key) ? key : null;
            }

            private static Dictionary<string, string> Build(IEnumerable<PlexServerContent> content, Func<PlexServerContent, string> idSelector)
            {
                return content
                    .Select(x => new { Id = idSelector(x), x.Key })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First().Key, StringComparer.OrdinalIgnoreCase);
            }
        }

        private sealed class ResolvedTvCleanupSelection
        {
            public string Error { get; set; }
            public List<MediaCleanupEpisodeRecord> Episodes { get; set; } = new List<MediaCleanupEpisodeRecord>();
            public List<int> SelectedSeasons { get; set; } = new List<int>();
        }

        private class CleanupTarget
        {
            public RequestType RequestType { get; set; }
            public int RequestId { get; set; }
            public string Title { get; set; }
            public string PosterPath { get; set; }
            public int TheMovieDbId { get; set; }
            public int TvDbId { get; set; }
            public bool Available { get; set; }
            public DateTime? AvailableSince { get; set; }
            public List<string> OwnerUserIds { get; set; } = new List<string>();
        }

        private sealed class MediaCleanupTerminalException : InvalidOperationException
        {
            public MediaCleanupTerminalException(string message) : base(message)
            {
            }
        }

        private class CleanupPermissions
        {
            public bool CanRequestRemoval { get; set; }
            public bool CanDeleteOwnMedia { get; set; }
            public bool CanVote { get; set; }
            public bool CanManage { get; set; }
        }
    }
}
