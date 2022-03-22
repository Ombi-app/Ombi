using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.MediaServer;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexEpisodeSync : MediaServerEpisodeSync<Metadata, PlexEpisode, IPlexContentRepository, PlexServerContent>, IPlexEpisodeSync
    {
        public PlexEpisodeSync(ISettingsService<PlexSettings> s, ILogger<PlexEpisodeSync> log, IPlexApi plexApi,
            IPlexContentRepository repo, IHubContext<NotificationHub> hub) : base(log, repo, hub)
        {
            _settings = s;
            _api = plexApi;
            _settings.ClearCache();
        }

        private readonly ISettingsService<PlexSettings> _settings;
        private readonly IPlexApi _api;

        public override async Task Execute(IJobExecutionContext job)
        {
            try
            {
                await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Plex Episode Sync Started");

                await CacheEpisodes();
                
                _logger.LogInformation(LoggingEvents.PlexEpisodeCacher, "We have finished caching the episodes.");
                await _repo.SaveChangesAsync();

            }
            catch (Exception e)
            {
                await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Plex Episode Sync Failed");
                _logger.LogError(LoggingEvents.Cacher, e, "Caching Episodes Failed");
            }


            _logger.LogInformation("Plex Episode Sync Finished - Triggering Metadata refresh");
            await OmbiQuartz.TriggerJob(nameof(IRefreshMetadata), "System");

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Plex Episode Sync Finished");
        }

        private async IAsyncEnumerable<Metadata> GetEpisodes(PlexServers settings, Directory section)
        {
            var currentPosition = 0;
            var resultCount = settings.EpisodeBatchSize == 0 ? 150 : settings.EpisodeBatchSize;
            var currentEpisodes = _repo.GetAllEpisodes().Cast<PlexEpisode>();
            var episodes = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition, resultCount);
            _logger.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Total Epsiodes found for {episodes.MediaContainer.librarySectionTitle} = {episodes.MediaContainer.totalSize}");

            currentPosition += resultCount;

            while (currentPosition < episodes.MediaContainer.totalSize)
            {
                var ep = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition,
                    resultCount);

                foreach (var episode in ep.MediaContainer.Metadata)
                {
                    // Let's check if we have the parent
                    var seriesExists = await _repo.GetByKey(episode.grandparentRatingKey);
                    if (seriesExists == null)
                    {
                        // Ok let's try and match it to a title. TODO (This is experimental)
                        seriesExists = await _repo.GetAll().FirstOrDefaultAsync(x =>
                            x.Title == episode.grandparentTitle);
                        if (seriesExists == null)
                        {
                            _logger.LogWarning(
                                "The episode title {0} we cannot find the parent series. The episode grandparentKey = {1}, grandparentTitle = {2}",
                                episode.title, episode.grandparentRatingKey, episode.grandparentTitle);
                            continue;
                        }

                        // Set the rating key to the correct one
                        episode.grandparentRatingKey = seriesExists.Key;
                    }
                    yield return episode;

                }
                _logger.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Processed {resultCount} more episodes. Total Remaining {episodes.MediaContainer.totalSize - currentPosition}");
                currentPosition += resultCount;
            }

        }


        private bool Validate(PlexServers settings)
        {
            if (string.IsNullOrEmpty(settings.PlexAuthToken))
            {
                return false;
            }

            return true;
        }

        protected override async IAsyncEnumerable<Metadata> GetMediaServerEpisodes()
        {
            var s = await _settings.GetSettingsAsync();
            if (!s.Enable)
            {
                yield break;
            }
            foreach (var server in s.Servers)
            {
                if (!Validate(server))
                {
                    _logger.LogWarning("Validation failed");
                    continue;
                }

                // Get the librarys and then get the tv section
                var sections = await _api.GetLibrarySections(server.PlexAuthToken, server.FullUri);

                // Filter the libSections
                var tvSections = sections.MediaContainer.Directory.Where(x => x.type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase));

                foreach (var section in tvSections)
                {
                    if (server.PlexSelectedLibraries.Any())
                    {
                        // Are any enabled?
                        if (server.PlexSelectedLibraries.Any(x => x.Enabled))
                        {
                            // Make sure we have enabled this 
                            var keys = server.PlexSelectedLibraries.Where(x => x.Enabled).Select(x => x.Key.ToString())
                                .ToList();
                            if (!keys.Contains(section.key))
                            {
                                // We are not monitoring this lib
                                continue;
                            }
                        }
                    }

                    // Get the episodes
                    await foreach (var ep in GetEpisodes(server, section))
                    {
                        yield return ep;
                    }
                }
            }
        }

        protected override Task<PlexEpisode> GetExistingEpisode(Metadata ep)
        { 
            return _repo.GetEpisodeByKey(ep.ratingKey);
        }

        protected override bool IsIn(Metadata ep, ICollection<PlexEpisode> list)
        {
            return false; // That check was never needed in Plex before refactoring
        }

        protected override void addEpisode(Metadata ep, ICollection<PlexEpisode> epToAdd)
        {
            epToAdd.Add(new PlexEpisode
            {
                EpisodeNumber = ep.index,
                SeasonNumber = ep.parentIndex,
                GrandparentKey = ep.grandparentRatingKey,
                ParentKey = ep.parentRatingKey,
                Key = ep.ratingKey,
                Title = ep.title
            });
        }

    }
}
