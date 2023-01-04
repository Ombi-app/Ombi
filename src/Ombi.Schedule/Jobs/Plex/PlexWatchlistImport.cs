using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexWatchlistImport : IPlexWatchlistImport
    {
        private readonly IPlexApi _plexApi;
        private readonly ISettingsService<PlexSettings> _settings;
        private readonly OmbiUserManager _ombiUserManager;
        private readonly IMovieRequestEngine _movieRequestEngine;
        private readonly ITvRequestEngine _tvRequestEngine;
        private readonly INotificationHubService _notificationHubService;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IExternalRepository<PlexWatchlistHistory> _watchlistRepo;
        private readonly IRepository<PlexWatchlistUserError> _userError;
        private readonly IMovieDbApi _movieDbApi;

        public PlexWatchlistImport(IPlexApi plexApi, ISettingsService<PlexSettings> settings, OmbiUserManager ombiUserManager,
            IMovieRequestEngine movieRequestEngine, ITvRequestEngine tvRequestEngine, INotificationHubService notificationHubService,
            ILogger<PlexWatchlistImport> logger, IExternalRepository<PlexWatchlistHistory> watchlistRepo, IRepository<PlexWatchlistUserError> userError,
            IMovieDbApi movieDbApi)
        {
            _plexApi = plexApi;
            _settings = settings;
            _ombiUserManager = ombiUserManager;
            _movieRequestEngine = movieRequestEngine;
            _tvRequestEngine = tvRequestEngine;
            _notificationHubService = notificationHubService;
            _logger = logger;
            _watchlistRepo = watchlistRepo;
            _userError = userError;
            _movieDbApi = movieDbApi;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = await _settings.GetSettingsAsync();
            if (!settings.Enable || !settings.EnableWatchlistImport)
            {
                _logger.LogDebug($"Not enabled. Plex Enabled: {settings.Enable}, Watchlist Enabled: {settings.EnableWatchlistImport}");
                return;
            }

            var plexUsersWithTokens = _ombiUserManager.Users.Where(x => x.UserType == UserType.PlexUser && x.MediaServerToken != null).ToList();
            _logger.LogInformation($"Found {plexUsersWithTokens.Count} users with tokens");
            await NotifyClient("Starting Watchlist Import");

            foreach (var user in plexUsersWithTokens)
            {
                try
                {
                    // Check if the user has errors and the token is the same (not refreshed)
                    var failedUser = await _userError.GetAll().Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                    if (failedUser != null)
                    {
                        if (failedUser.MediaServerToken.Equals(user.MediaServerToken))
                        {
                            _logger.LogInformation($"Skipping Plex Watchlist Import for user '{user.UserName}' as they failed previously and the token has not yet been refreshed");
                            continue;
                        }
                        else
                        {
                            // remove that guy
                            await _userError.Delete(failedUser);
                            failedUser = null;
                        }
                    }

                    _logger.LogDebug($"Starting Watchlist Import for {user.UserName} with token {user.MediaServerToken}");
                    var watchlist = await _plexApi.GetWatchlist(user.MediaServerToken, context?.CancellationToken ?? CancellationToken.None);
                    if (watchlist?.AuthError ?? false)
                    {
                        _logger.LogError($"Auth failed for user '{user.UserName}'. Need to re-authenticate with Ombi.");
                        await _userError.Add(new PlexWatchlistUserError
                        {
                            UserId = user.Id,
                            MediaServerToken = user.MediaServerToken,
                        });
                        continue;
                    }
                    if (watchlist == null || !(watchlist.MediaContainer?.Metadata?.Any() ?? false))
                    {
                        _logger.LogDebug($"No watchlist found for {user.UserName}");
                        continue;
                    }

                    var items = watchlist.MediaContainer.Metadata;
                    _logger.LogDebug($"Items found in watchlist: {watchlist.MediaContainer.totalSize}");
                    foreach (var item in items)
                    {
                        _logger.LogDebug($"Processing {item.title} {item.type}");
                        var providerIds = await GetProviderIds(user.MediaServerToken, item, context?.CancellationToken ?? CancellationToken.None);
                        if (!providerIds.TheMovieDb.HasValue())
                        {
                            // Try and use another Id to figure out TheMovieDB
                            var movieDbId = await FindTmdbIdFromAlternateSources(providerIds, item.type);
                            if (string.IsNullOrEmpty(movieDbId))
                            {
                                _logger.LogWarning($"No TheMovieDb Id found for {item.title} for user {user.UserName}, could not import via Plex WatchList");
                                // We need a MovieDbId to support this;
                                continue;
                            }

                            providerIds.TheMovieDb = movieDbId;
                        }

                        // Check to see if we have already imported this item
                        var alreadyImported = _watchlistRepo.GetAll().Any(x => x.TmdbId == providerIds.TheMovieDb);
                        if (alreadyImported)
                        {
                            _logger.LogDebug($"{item.title} already imported via Plex WatchList, skipping");
                            continue;
                        }

                        switch (item.type)
                        {
                            case "show":
                                await ProcessShow(int.Parse(providerIds.TheMovieDb), user, settings.MonitorAll);
                                break;
                            case "movie":
                                await ProcessMovie(int.Parse(providerIds.TheMovieDb), user);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception thrown when importing watchlist for user {user.UserName}");
                    continue;
                }
            }

            await NotifyClient("Finished Watchlist Import");
        }

        private async Task<string> FindTmdbIdFromAlternateSources(ProviderId providerId, string type)
        {
            FindResult result = null;
            var hasResult = false;
            var movie = type == "movie";
            if (!string.IsNullOrEmpty(providerId.TheTvDb))
            {
                result = await _movieDbApi.Find(providerId.TheTvDb, ExternalSource.tvdb_id);
                hasResult = result?.tv_results?.Length > 0;
            }
            if (!string.IsNullOrEmpty(providerId.ImdbId) && !hasResult)
            {
                result = await _movieDbApi.Find(providerId.ImdbId, ExternalSource.imdb_id);
                if (movie)
                {
                    hasResult = result?.movie_results?.Length > 0;
                }
                else
                {
                    hasResult = result?.tv_results?.Length > 0;
                }
            }
            if (hasResult)
            {
                if (movie)
                {
                    return result.movie_results?[0]?.id.ToString() ?? string.Empty;
                }
                else
                {

                    return result.tv_results?[0]?.id.ToString() ?? string.Empty;
                }
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
                    await AddToHistory(theMovieDbId);
                    return;
                }
                _logger.LogInformation($"Error adding title from PlexWatchlist for user '{user.UserName}'. Message: '{response.ErrorMessage}'");
            }
            else
            {
                await AddToHistory(theMovieDbId);

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
                    await AddToHistory(theMovieDbId);
                    return;
                }
                _logger.LogInformation($"Error adding title from PlexWatchlist for user '{user.UserName}'. Message: '{response.ErrorMessage}'");
            }
            else
            {
                await AddToHistory(theMovieDbId);
                _logger.LogInformation($"Added title from PlexWatchlist for user '{user.UserName}'. {response.Message}");
            }
        }
        private async Task AddToHistory(int theMovieDbId)
        {

            // Add to the watchlist history
            var history = new PlexWatchlistHistory
            {
                TmdbId = theMovieDbId.ToString()
            };
            await _watchlistRepo.Add(history);
        }

        private async Task<ProviderId> GetProviderIds(string authToken, Metadata movie, CancellationToken cancellationToken)
        {
            var guids = new List<string>();
            if (!movie.Guid.Any())
            {
                var metaData = await _plexApi.GetWatchlistMetadata(movie.ratingKey, authToken, cancellationToken);

                var meta = metaData.MediaContainer.Metadata.FirstOrDefault();
                guids.Add(meta.guid);
                if (meta.Guid != null)
                {
                    foreach (var g in meta.Guid)
                    {
                        guids.Add(g.Id);
                    }
                }
            }
            else
            {
                // Currently a Plex Pass feature only
                foreach (var g in movie.Guid)
                {
                    guids.Add(g.Id);
                }
            }
            var providerIds = PlexHelper.GetProviderIdsFromMetadata(guids.ToArray());
            return providerIds;
        }

        private async Task NotifyClient(string message)
        {
            await _notificationHubService.SendNotificationToAdmins($"Plex Watchlist Import - {message}");
        }

        public void Dispose() { }
    }
}
