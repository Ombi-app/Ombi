using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexEpisodeCacher : IPlexEpisodeCacher
    {
        public PlexEpisodeCacher(ISettingsService<PlexSettings> s, ILogger<PlexEpisodeCacher> log, IPlexApi plexApi,
            IPlexContentRepository repo, IPlexAvailabilityChecker a)
        {
            _settings = s;
            _log = log;
            _api = plexApi;
            _repo = repo;
            _availabilityChecker = a;
        }

        private readonly ISettingsService<PlexSettings> _settings;
        private readonly ILogger<PlexEpisodeCacher> _log;
        private readonly IPlexApi _api;
        private readonly IPlexContentRepository _repo;
        private readonly IPlexAvailabilityChecker _availabilityChecker;

        public async Task Start()
        {
            try
            {
                var s = await _settings.GetSettingsAsync();
                if (!s.Enable)
                {
                    return;
                }

                foreach (var server in s.Servers)
                {
                    await Cache(server);
                    BackgroundJob.Enqueue(() => _availabilityChecker.Start());
                }
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.Cacher, e, "Caching Episodes Failed");
            }
        }

        private async Task Cache(PlexServers settings)
        {
            if (!Validate(settings))
            {

                _log.LogWarning("Validation failed");
                return;
            }

            // Get the librarys and then get the tv section
            var sections = await _api.GetLibrarySections(settings.PlexAuthToken, settings.FullUri);

            // Filter the libSections
            var tvSections = sections.MediaContainer.Directory.Where(x => x.type.Equals(Jobs.PlexContentCacher.PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase));

            foreach (var section in tvSections)
            {
                if (settings.PlexSelectedLibraries.Any())
                {
                    // Are any enabled?
                    if (settings.PlexSelectedLibraries.Any(x => x.Enabled))
                    {
                        // Make sure we have enabled this 
                        var keys = settings.PlexSelectedLibraries.Where(x => x.Enabled).Select(x => x.Key.ToString())
                            .ToList();
                        if (!keys.Contains(section.key))
                        {
                            // We are not monitoring this lib
                            continue;
                        }
                    }
                }

                // Get the episodes
                await GetEpisodes(settings, section);
            }

        }

        private async Task GetEpisodes(PlexServers settings, Directory section)
        {
            var currentPosition = 0;
            var resultCount = settings.EpisodeBatchSize == 0 ? 50 : settings.EpisodeBatchSize;
            var episodes = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition, resultCount);
            var currentData = _repo.GetAllEpisodes();
            _log.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Total Epsiodes found for {episodes.MediaContainer.librarySectionTitle} = {episodes.MediaContainer.totalSize}");

            await ProcessEpsiodes(episodes, currentData);
            currentPosition += resultCount;

            while (currentPosition < episodes.MediaContainer.totalSize)
            {
                var ep = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition,
                    resultCount);
                await ProcessEpsiodes(ep, currentData);
                _log.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Processed {resultCount} more episodes. Total Remaining {episodes.MediaContainer.totalSize - currentPosition}");
                currentPosition += resultCount;
            }

            // we have now finished.
            await _repo.SaveChangesAsync();
        }

        private async Task ProcessEpsiodes(PlexContainer episodes, IQueryable<PlexEpisode> currentEpisodes)
        {
            var ep = new HashSet<PlexEpisode>();

            foreach (var episode in episodes?.MediaContainer?.Metadata ?? new Metadata[]{})
            {
                // I don't think we need to get the metadata, we only need to get the metadata if we need the provider id (TheTvDbid). Why do we need it for episodes?
                // We have the parent and grandparent rating keys to link up to the season and series
                //var metadata = _api.GetEpisodeMetaData(server.PlexAuthToken, server.FullUri, episode.ratingKey);

                var epExists = currentEpisodes.Any(x => episode.ratingKey == x.Key &&
                                                          episode.grandparentRatingKey == x.GrandparentKey);
                if (epExists)
                {
                    continue;
                }

                ep.Add(new PlexEpisode
                {
                    EpisodeNumber = episode.index,
                    SeasonNumber = episode.parentIndex,
                    GrandparentKey = episode.grandparentRatingKey,
                    ParentKey = episode.parentRatingKey,
                    Key = episode.ratingKey,
                    Title = episode.title
                });
            }

            await _repo.AddRange(ep);
        }

        private bool Validate(PlexServers settings)
        {
            if (string.IsNullOrEmpty(settings.PlexAuthToken))
            {
                return false ;
            }

            return true;
        }
    }
}