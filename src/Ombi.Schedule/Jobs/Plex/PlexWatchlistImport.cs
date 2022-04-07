using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Quartz;
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
        private readonly ILogger _logger;

        public PlexWatchlistImport(IPlexApi plexApi, ISettingsService<PlexSettings> settings, OmbiUserManager ombiUserManager,
            IMovieRequestEngine movieRequestEngine, ITvRequestEngine tvRequestEngine,
            ILogger<PlexWatchlistImport> logger)
        {
            _plexApi = plexApi;
            _settings = settings;
            _ombiUserManager = ombiUserManager;
            _movieRequestEngine = movieRequestEngine;
            _tvRequestEngine = tvRequestEngine;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = await _settings.GetSettingsAsync();
            if (!settings.Enable || !settings.EnableWatchlistImport)
            {
                return;
            }

            var plexUsersWithTokens = _ombiUserManager.Users.Where(x => x.UserType == UserType.PlexUser && x.MediaServerToken != null).ToList();
            foreach (var user in plexUsersWithTokens)
            {
                var watchlist = await _plexApi.GetWatchlist(user.MediaServerToken, context?.CancellationToken ?? CancellationToken.None);
                if (watchlist == null || !(watchlist.MediaContainer?.Metadata?.Any() ?? false))
                {
                    return;
                }

                var items = watchlist.MediaContainer.Metadata;
                foreach (var item in items)
                {
                    var providerIds = await GetProviderIds(user.MediaServerToken, item, context?.CancellationToken ?? CancellationToken.None);
                    if (!providerIds.TheMovieDb.HasValue())
                    {
                        // We need a MovieDbId to support this;
                        return;
                    }
                    switch (item.type)
                    {
                        case "show":
                            await ProcessShow(int.Parse(providerIds.TheMovieDb), user, context?.CancellationToken ?? CancellationToken.None);
                            break;
                        case "movie":
                            await ProcessMovie(int.Parse(providerIds.TheMovieDb), user, context?.CancellationToken ?? CancellationToken.None);
                            break;
                    }
                }
            }
        }

        private async Task ProcessMovie(int theMovieDbId, OmbiUser user, CancellationToken cancellationToken)
        {
            _movieRequestEngine.SetUser(user);
            var response = await _movieRequestEngine.RequestMovie(new() { TheMovieDbId = theMovieDbId, Source = RequestSource.PlexWatchlist});
            if (response.IsError)
            {
                if (response.ErrorCode == ErrorCode.AlreadyRequested)
                {
                    return;
                }
                _logger.LogInformation($"Error adding title from PlexWatchlist for user '{user.UserName}'. Message: '{response.ErrorMessage}'");
            }
            else
            {
                _logger.LogInformation($"Added title from PlexWatchlist for user '{user.UserName}'. {response.Message}");
            }
        }


        private async Task ProcessShow(int theMovieDbId, OmbiUser user, CancellationToken cancellationToken)
        {
            _tvRequestEngine.SetUser(user);
            var response = await _tvRequestEngine.RequestTvShow(new TvRequestViewModelV2 { RequestAll = true, TheMovieDbId = theMovieDbId, Source = RequestSource.PlexWatchlist });
            if (response.IsError)
            {
                if (response.ErrorCode == ErrorCode.AlreadyRequested)
                {
                    return;
                }
                _logger.LogInformation($"Error adding title from PlexWatchlist for user '{user.UserName}'. Message: '{response.ErrorMessage}'");
            }
            else
            {
                _logger.LogInformation($"Added title from PlexWatchlist for user '{user.UserName}'. {response.Message}");
            }
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

        public void Dispose() { }
    }
}
