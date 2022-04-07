using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
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
        private readonly ILogger _logger;

        public PlexWatchlistImport(IPlexApi plexApi, ISettingsService<PlexSettings> settings, OmbiUserManager ombiUserManager,
            IMovieRequestEngine movieRequestEngine,
            ILogger<PlexWatchlistImport> logger)
        {
            _plexApi = plexApi;
            _settings = settings;
            _ombiUserManager = ombiUserManager;
            _movieRequestEngine = movieRequestEngine;
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
                    switch (item.type)
                    {
                        case "show":
                            await ProcessShow(item);
                            break;
                        case "movie":
                            await ProcessMovie(user.MediaServerToken, item, user, context?.CancellationToken ?? CancellationToken.None);
                            break;
                    }
                }
            }
        }

        private async Task ProcessMovie(string authToken, Metadata movie, OmbiUser user, CancellationToken cancellationToken)
        {
            var providerIds = await GetProviderIds(authToken, movie, cancellationToken);
            if (!providerIds.TheMovieDb.HasValue())
            {
                // We need a MovieDbId to support this;
                return;
            }
            _movieRequestEngine.SetUser(user);
            var response = await _movieRequestEngine.RequestMovie(new() { TheMovieDbId = int.Parse(providerIds.TheMovieDb), Source = RequestSource.PlexWatchlist});
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

        private async Task ProcessShow(Metadata metadata)
        {

        }

        public void Dispose() { }
    }
}
