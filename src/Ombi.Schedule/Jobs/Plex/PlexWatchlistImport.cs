using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Api.External.MediaServers.Plex.Models.Community;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserType = Ombi.Store.Entities.UserType;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexWatchlistImport : IPlexWatchlistImport
    {
        private readonly IPlexApi _plexApi;
        private readonly ISettingsService<PlexSettings> _settings;
        private readonly ISettingsService<UserManagementSettings> _userManagementSettings;
        private readonly OmbiUserManager _ombiUserManager;
        private readonly IMovieRequestEngine _movieRequestEngine;
        private readonly ITvRequestEngine _tvRequestEngine;
        private readonly INotificationHubService _notificationHubService;
        private readonly ILogger<PlexWatchlistImport> _logger;
        private readonly IExternalRepository<PlexWatchlistHistory> _watchlistRepo;
        private readonly IMovieDbApi _movieDbApi;
        private readonly IPlexWatchlistStatusStore _statusStore;

        public PlexWatchlistImport(IPlexApi plexApi, ISettingsService<PlexSettings> settings,
            ISettingsService<UserManagementSettings> userManagementSettings, OmbiUserManager ombiUserManager,
            IMovieRequestEngine movieRequestEngine, ITvRequestEngine tvRequestEngine, INotificationHubService notificationHubService,
            ILogger<PlexWatchlistImport> logger, IExternalRepository<PlexWatchlistHistory> watchlistRepo,
            IMovieDbApi movieDbApi, IPlexWatchlistStatusStore statusStore)
        {
            _plexApi = plexApi;
            _settings = settings;
            _userManagementSettings = userManagementSettings;
            _ombiUserManager = ombiUserManager;
            _movieRequestEngine = movieRequestEngine;
            _tvRequestEngine = tvRequestEngine;
            _notificationHubService = notificationHubService;
            _logger = logger;
            _watchlistRepo = watchlistRepo;
            _movieDbApi = movieDbApi;
            _statusStore = statusStore;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = await _settings.GetSettingsAsync();
            if (!settings.Enable || !settings.EnableWatchlistImport)
            {
                _logger.LogDebug($"Not enabled. Plex Enabled: {settings.Enable}, Watchlist Enabled: {settings.EnableWatchlistImport}");
                return;
            }

            var selectedServer = settings.Servers?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.PlexAuthToken));
            if (selectedServer == null)
            {
                _logger.LogWarning("No Plex server configured; skipping watchlist import");
                return;
            }

            var ct = context?.CancellationToken ?? CancellationToken.None;

            // Community/plex.tv endpoints only accept a user OAuth token, not a server token.
            // The server owner's OAuth token is captured in OmbiUser.MediaServerToken when they
            // sign into Ombi with Plex; fall back to nothing if we can't find one.
            var adminToken = await ResolveAdminOAuthToken(ct);
            if (string.IsNullOrWhiteSpace(adminToken))
            {
                _logger.LogWarning("Watchlist import requires the server owner to be signed into Ombi via Plex OAuth; skipping");
                await NotifyClient("Watchlist import skipped — the server owner must sign into Ombi via Plex");
                return;
            }

            _logger.LogInformation($"Watchlist import using server '{selectedServer.Name}' (machine {selectedServer.MachineIdentifier})");
            await NotifyClient("Starting Watchlist Import");

            var (targets, friendsFetched, adminResolved) = await BuildTargetList(adminToken, ct);
            if (targets.Count == 0)
            {
                _logger.LogInformation("No watchlist targets found (admin + friends)");
                await NotifyClient("Finished Watchlist Import");
                return;
            }

            var userManagement = await _userManagementSettings.GetSettingsAsync();
            var matchedUserIds = new HashSet<string>();

            foreach (var target in targets)
            {
                if (ct.IsCancellationRequested) break;

                OmbiUser user = null;
                try
                {
                    user = await EnsureOmbiUser(target, userManagement, ct);
                    if (user == null)
                    {
                        continue;
                    }

                    matchedUserIds.Add(user.Id);

                    var succeeded = await ImportWatchlistForUser(adminToken, target, user, settings.MonitorAll, ct);
                    await _statusStore.SetAsync(user.Id, succeeded ? WatchlistSyncStatus.Successful : WatchlistSyncStatus.Failed, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception thrown when importing watchlist for Plex user {target.username}");
                    if (user != null)
                    {
                        try { await _statusStore.SetAsync(user.Id, WatchlistSyncStatus.Failed, ct); }
                        catch (Exception statusEx) { _logger.LogWarning(statusEx, "Failed to record watchlist status"); }
                    }
                }
            }

            // For existing Plex users that didn't match a target this run, mark them as NotAFriend.
            // The sweep skips a row if any of these are true:
            //   * we successfully matched it to a target (matchedUserIds), OR
            //   * its UserName matches a target's username, OR
            //   * its ProviderUserId matches a target's community id.
            // The latter two cover edge cases where matchedUserIds is incomplete:
            // banned users (EnsureOmbiUser returns null), transient EnsureOmbiUser failures,
            // and legacy-numeric ProviderUserId rows. Only run the sweep when both admin and
            // friends resolved cleanly AND the run wasn't cancelled — otherwise matched data
            // may be incomplete and we'd mislabel valid friends.
            if (!ct.IsCancellationRequested && friendsFetched && adminResolved)
            {
                var targetUsernames = new HashSet<string>(
                    targets.Where(t => !string.IsNullOrWhiteSpace(t.username)).Select(t => t.username),
                    StringComparer.OrdinalIgnoreCase);
                var targetIds = new HashSet<string>(
                    targets.Where(t => !string.IsNullOrWhiteSpace(t.id)).Select(t => t.id),
                    StringComparer.Ordinal);

                var existingPlexUsers = await _ombiUserManager.Users.Where(x => x.UserType == UserType.PlexUser).ToListAsync(ct);
                foreach (var existing in existingPlexUsers)
                {
                    if (matchedUserIds.Contains(existing.Id)) continue;
                    if (!string.IsNullOrWhiteSpace(existing.UserName) && targetUsernames.Contains(existing.UserName)) continue;
                    if (!string.IsNullOrWhiteSpace(existing.ProviderUserId) && targetIds.Contains(existing.ProviderUserId)) continue;
                    await _statusStore.SetAsync(existing.Id, WatchlistSyncStatus.NotAFriend, ct);
                }
            }

            await NotifyClient("Finished Watchlist Import");
        }

        private async Task<string> ResolveAdminOAuthToken(CancellationToken ct)
        {
            var candidates = await _ombiUserManager.Users
                .Where(u => u.UserType == UserType.PlexUser && u.MediaServerToken != null && u.MediaServerToken != string.Empty)
                .ToListAsync(ct);

            foreach (var candidate in candidates)
            {
                if (await _ombiUserManager.IsInRoleAsync(candidate, OmbiRoles.Admin))
                {
                    return candidate.MediaServerToken;
                }
            }

            return null;
        }

        private async Task<(List<PlexCommunityUser> targets, bool friendsFetched, bool adminResolved)> BuildTargetList(string adminToken, CancellationToken ct)
        {
            var targets = new List<PlexCommunityUser>();
            var friendsFetched = false;
            var adminResolved = false;

            try
            {
                var account = await _plexApi.GetAccount(adminToken);
                if (account?.user != null && !string.IsNullOrWhiteSpace(account.user.uuid))
                {
                    var adminUser = new PlexCommunityUser
                    {
                        id = account.user.uuid,
                        username = account.user.username ?? account.user.title,
                        displayName = account.user.title,
                    };
                    targets.Add(adminUser);
                    adminResolved = true;
                }
                else
                {
                    _logger.LogWarning("Unable to resolve admin Plex account; admin watchlist will be skipped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch admin Plex account");
            }

            try
            {
                var friendsResponse = await _plexApi.GetAllFriends(adminToken, ct);
                var hasErrors = friendsResponse?.errors != null && friendsResponse.errors.Count > 0;
                if (hasErrors)
                {
                    _logger.LogWarning("Plex community API returned errors fetching friends: {Errors}",
                        string.Join("; ", friendsResponse.errors.Select(e => e.message)));
                }
                var friends = friendsResponse?.data?.allFriendsV2 ?? new List<PlexCommunityFriend>();
                _logger.LogInformation("Found {Count} Plex friends", friends.Count);
                foreach (var friend in friends)
                {
                    if (friend?.user == null || string.IsNullOrWhiteSpace(friend.user.id)) continue;
                    targets.Add(friend.user);
                }
                // Only claim we fetched the friends list if the API returned no errors. A populated
                // errors collection means the data we got is unreliable, so downstream code must not
                // treat missing entries as "no longer a friend".
                friendsFetched = !hasErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Plex friends");
            }

            return (targets, friendsFetched, adminResolved);
        }

        private async Task<OmbiUser> EnsureOmbiUser(PlexCommunityUser plexUser, UserManagementSettings userManagement, CancellationToken ct)
        {
            if (userManagement.BannedPlexUserIds != null && userManagement.BannedPlexUserIds.Contains(plexUser.id))
            {
                _logger.LogInformation($"Skipping banned Plex user '{plexUser.username}'");
                return null;
            }

            var existing = await _ombiUserManager.Users.FirstOrDefaultAsync(x => x.UserType == UserType.PlexUser && x.ProviderUserId == plexUser.id, ct);
            if (existing == null && !string.IsNullOrWhiteSpace(plexUser.username))
            {
                // Fall back to a username match. Pre-existing users imported via the legacy
                // /api/users path store the numeric plex.tv account id, while the community API
                // returns the account UUID — same user, different identifier. We adopt that row
                // without rewriting ProviderUserId so /api/v1/Token/plextoken
                // (OmbiUserManager.GetOmbiUserFromPlexToken matches on the numeric id) keeps
                // working; the in-memory matchedUserIds set drives the NotAFriend sweep instead.
                //
                // If the existing row already has a non-numeric ProviderUserId that doesn't
                // match the community id, treat it as a real collision and skip — Plex usernames
                // can be reused after an account is deleted, and we don't want to hijack the
                // existing row.
                var usernameMatch = await _ombiUserManager.Users.FirstOrDefaultAsync(x => x.UserType == UserType.PlexUser && x.UserName == plexUser.username, ct);
                if (usernameMatch != null)
                {
                    var stored = usernameMatch.ProviderUserId;
                    var conflict =
                        !string.IsNullOrWhiteSpace(stored)
                        && !string.Equals(stored, plexUser.id, StringComparison.Ordinal)
                        && !IsLegacyNumericPlexId(stored);

                    if (conflict)
                    {
                        _logger.LogWarning(
                            "Plex user '{Username}' ({PlexUserId}) collides with existing Ombi user bound to {ProviderUserId}; skipping",
                            plexUser.username, plexUser.id, stored);
                        return null;
                    }

                    existing = usernameMatch;
                }
            }

            if (existing != null)
            {
                return existing;
            }

            if (string.IsNullOrWhiteSpace(plexUser.username))
            {
                _logger.LogInformation($"Cannot create Ombi user for Plex id {plexUser.id} without a username");
                return null;
            }

            var newUser = new OmbiUser
            {
                UserType = UserType.PlexUser,
                UserName = plexUser.username,
                ProviderUserId = plexUser.id,
                Email = string.Empty,
                Alias = plexUser.displayName ?? string.Empty,
                MovieRequestLimit = userManagement.MovieRequestLimit,
                MovieRequestLimitType = userManagement.MovieRequestLimitType,
                EpisodeRequestLimit = userManagement.EpisodeRequestLimit,
                EpisodeRequestLimitType = userManagement.EpisodeRequestLimitType,
                MusicRequestLimit = userManagement.MusicRequestLimit,
                MusicRequestLimitType = userManagement.MusicRequestLimitType,
                StreamingCountry = userManagement.DefaultStreamingCountry,
            };

            _logger.LogInformation($"Creating Ombi user for Plex friend '{newUser.UserName}'");
            var result = await _ombiUserManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning($"Could not create Ombi user for '{plexUser.username}': {error.Description}");
                }
                return null;
            }

            if (userManagement.DefaultRoles != null)
            {
                foreach (var role in userManagement.DefaultRoles)
                {
                    await _ombiUserManager.AddToRoleAsync(newUser, role);
                }
            }
            return newUser;
        }

        // The legacy /api/users path stores a numeric plex.tv account id. The community API
        // returns a UUID-shaped string. We use this to recognise legacy-imported rows that
        // are safe to adopt during a username fallback without tripping the hijack guard.
        private static bool IsLegacyNumericPlexId(string providerUserId)
        {
            if (string.IsNullOrWhiteSpace(providerUserId)) return false;
            for (var i = 0; i < providerUserId.Length; i++)
            {
                if (!char.IsDigit(providerUserId[i])) return false;
            }
            return true;
        }

        private const int MaxWatchlistPages = 200;

        private async Task<bool> ImportWatchlistForUser(string adminToken, PlexCommunityUser plexUser, OmbiUser user, bool monitorAll, CancellationToken ct)
        {
            string cursor = null;
            var currentWatchlistTmdbIds = new HashSet<string>();
            var pendingItems = new List<(PlexCommunityWatchlistNode node, ProviderId ids)>();
            var seenCursors = new HashSet<string>();
            var pageCount = 0;

            do
            {
                pageCount++;
                if (pageCount > MaxWatchlistPages)
                {
                    _logger.LogWarning("Watchlist for '{User}' exceeded {Max} pages; stopping to avoid an infinite loop",
                        plexUser.username, MaxWatchlistPages);
                    break;
                }
                if (!string.IsNullOrEmpty(cursor) && !seenCursors.Add(cursor))
                {
                    _logger.LogWarning("Plex community API returned a repeated pagination cursor for '{User}'; stopping",
                        plexUser.username);
                    break;
                }

                var response = await _plexApi.GetWatchlistForUser(adminToken, plexUser.id, cursor, ct);
                if (response?.errors != null && response.errors.Count > 0)
                {
                    _logger.LogWarning($"Plex community API returned errors for '{plexUser.username}': {string.Join("; ", response.errors.Select(e => e.message))}");
                    return false;
                }

                var watchlist = response?.data?.userV2?.watchlist;
                var nodes = watchlist?.nodes ?? new List<PlexCommunityWatchlistNode>();
                foreach (var node in nodes)
                {
                    if (string.IsNullOrWhiteSpace(node.id)) continue;

                    var ids = await ResolveProviderIds(adminToken, node, ct);
                    if (!ids.TheMovieDb.HasValue())
                    {
                        var alt = await FindTmdbIdFromAlternateSources(ids, node.type);
                        if (string.IsNullOrEmpty(alt))
                        {
                            _logger.LogWarning($"No TheMovieDb Id found for {node.title} for user {user.UserName}, skipping");
                            continue;
                        }
                        ids.TheMovieDb = alt;
                    }

                    currentWatchlistTmdbIds.Add(ids.TheMovieDb);
                    pendingItems.Add((node, ids));
                }

                cursor = watchlist?.pageInfo?.hasNextPage == true ? watchlist.pageInfo.endCursor : null;
            }
            while (!string.IsNullOrEmpty(cursor) && !ct.IsCancellationRequested);

            // Always purge history for items no longer on the user's Plex watchlist
            // (including when the watchlist has been fully cleared).
            var historyEntries = await _watchlistRepo.GetAll().Where(x => x.UserId == user.Id).ToListAsync(ct);
            var existingTmdbIds = new HashSet<string>(historyEntries.Select(h => h.TmdbId));
            foreach (var entry in historyEntries)
            {
                if (!currentWatchlistTmdbIds.Contains(entry.TmdbId))
                {
                    _logger.LogDebug($"Removing old history entry for TMDB ID {entry.TmdbId} (no longer in Plex watchlist for {user.UserName})");
                    await _watchlistRepo.Delete(entry);
                }
            }

            foreach (var (node, ids) in pendingItems)
            {
                if (existingTmdbIds.Contains(ids.TheMovieDb))
                {
                    continue;
                }

                if (!int.TryParse(ids.TheMovieDb, out var tmdbId))
                {
                    _logger.LogWarning($"Skipping {node.title} for {user.UserName}: non-numeric TMDB id '{ids.TheMovieDb}'");
                    continue;
                }

                var nodeType = node.type ?? string.Empty;
                if (nodeType.Equals("show", StringComparison.OrdinalIgnoreCase) ||
                    nodeType.Equals("tvshow", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessShow(tmdbId, user, monitorAll);
                }
                else if (nodeType.Equals("movie", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessMovie(tmdbId, user);
                }
                else
                {
                    _logger.LogDebug($"Skipping unknown watchlist type '{node.type}' for {node.title}");
                }
            }

            return true;
        }

        private async Task<ProviderId> ResolveProviderIds(string adminToken, PlexCommunityWatchlistNode node, CancellationToken ct)
        {
            var guids = new List<string>();
            var metaData = await _plexApi.GetWatchlistMetadata(node.id, adminToken, ct);
            var meta = metaData?.MediaContainer?.Metadata?.FirstOrDefault();
            if (meta != null)
            {
                if (!string.IsNullOrEmpty(meta.guid))
                {
                    guids.Add(meta.guid);
                }
                if (meta.Guid != null)
                {
                    foreach (var g in meta.Guid)
                    {
                        guids.Add(g.Id);
                    }
                }
            }
            return PlexHelper.GetProviderIdsFromMetadata(guids.ToArray());
        }

        private async Task<string> FindTmdbIdFromAlternateSources(ProviderId providerId, string type)
        {
            FindResult result = null;
            var hasResult = false;
            var movie = string.Equals(type, "movie", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(providerId.TheTvDb))
            {
                result = await _movieDbApi.Find(providerId.TheTvDb, ExternalSource.tvdb_id);
                hasResult = movie ? result?.movie_results?.Length > 0 : result?.tv_results?.Length > 0;
            }
            if (!string.IsNullOrEmpty(providerId.ImdbId) && !hasResult)
            {
                result = await _movieDbApi.Find(providerId.ImdbId, ExternalSource.imdb_id);
                hasResult = movie ? result?.movie_results?.Length > 0 : result?.tv_results?.Length > 0;
            }
            if (hasResult)
            {
                return movie
                    ? result.movie_results?[0]?.id.ToString() ?? string.Empty
                    : result.tv_results?[0]?.id.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        private async Task ProcessMovie(int theMovieDbId, OmbiUser user)
        {
            _movieRequestEngine.SetUser(user);
            var response = await _movieRequestEngine.RequestMovie(new() { TheMovieDbId = theMovieDbId, Source = RequestSource.PlexWatchlist });
            if (response.IsError)
            {
                if (response.ErrorCode == ErrorCode.AlreadyRequested)
                {
                    _logger.LogDebug($"Movie already requested for user '{user.UserName}'");
                    await AddToHistory(theMovieDbId, user.Id);
                    return;
                }
                _logger.LogInformation($"Error adding title from PlexWatchlist for user '{user.UserName}'. Message: '{response.ErrorMessage}'");
            }
            else
            {
                await AddToHistory(theMovieDbId, user.Id);
                _logger.LogInformation($"Added title from PlexWatchlist for user '{user.UserName}'. {response.Message}");
            }
        }

        private async Task ProcessShow(int theMovieDbId, OmbiUser user, bool requestAll)
        {
            _tvRequestEngine.SetUser(user);
            var requestModel = new TvRequestViewModelV2 { LatestSeason = true, TheMovieDbId = theMovieDbId, Source = RequestSource.PlexWatchlist };
            if (requestAll)
            {
                requestModel.RequestAll = true;
                requestModel.LatestSeason = false;
            }
            var response = await _tvRequestEngine.RequestTvShow(requestModel);
            if (response.IsError)
            {
                if (response.ErrorCode == ErrorCode.AlreadyRequested)
                {
                    _logger.LogDebug($"Show already requested for user '{user.UserName}'");
                    await AddToHistory(theMovieDbId, user.Id);
                    return;
                }
                _logger.LogInformation($"Error adding title from PlexWatchlist for user '{user.UserName}'. Message: '{response.ErrorMessage}'");
            }
            else
            {
                await AddToHistory(theMovieDbId, user.Id);
                _logger.LogInformation($"Added title from PlexWatchlist for user '{user.UserName}'. {response.Message}");
            }
        }

        private async Task AddToHistory(int theMovieDbId, string userId)
        {
            var history = new PlexWatchlistHistory
            {
                TmdbId = theMovieDbId.ToString(),
                AddedAt = DateTime.UtcNow,
                UserId = userId,
            };
            await _watchlistRepo.Add(history);
        }

        private async Task NotifyClient(string message)
        {
            await _notificationHubService.SendNotificationToAdmins($"Plex Watchlist Import - {message}");
        }

        public void Dispose() { }
    }
}
