using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexEpisodeSync : IPlexEpisodeSync
    {
        public PlexEpisodeSync(ISettingsService<PlexSettings> s, ILogger<PlexEpisodeSync> log, IPlexApi plexApi,
            IPlexContentRepository repo, IPlexAvailabilityChecker a)
        {
            _settings = s;
            _log = log;
            _api = plexApi;
            _repo = repo;
            _availabilityChecker = a;
        }

        private readonly ISettingsService<PlexSettings> _settings;
        private readonly ILogger<PlexEpisodeSync> _log;
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
                }

            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.Cacher, e, "Caching Episodes Failed");
            }

            BackgroundJob.Enqueue(() => _availabilityChecker.Start());
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
            var tvSections = sections.MediaContainer.Directory.Where(x => x.type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase));

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
            var resultCount = settings.EpisodeBatchSize == 0 ? 150 : settings.EpisodeBatchSize;
            var currentEpisodes = _repo.GetAllEpisodes();
            var episodes = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition, resultCount);
            _log.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Total Epsiodes found for {episodes.MediaContainer.librarySectionTitle} = {episodes.MediaContainer.totalSize}");

            // Delete all the episodes because we cannot uniquly match an episode to series every time, 
            // see comment below.

            // 12.03.2017 - I think we should be able to match them now
            //await _repo.ExecuteSql("DELETE FROM PlexEpisode");

            await ProcessEpsiodes(episodes?.MediaContainer?.Metadata ?? new Metadata[] { }, currentEpisodes);
            currentPosition += resultCount;

            while (currentPosition < episodes.MediaContainer.totalSize)
            {
                var ep = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition,
                    resultCount);

                await ProcessEpsiodes(ep?.MediaContainer?.Metadata ?? new Metadata[] { }, currentEpisodes);
                _log.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Processed {resultCount} more episodes. Total Remaining {episodes.MediaContainer.totalSize - currentPosition}");
                currentPosition += resultCount;
            }

            // we have now finished.
            _log.LogInformation(LoggingEvents.PlexEpisodeCacher, "We have finished caching the episodes.");
            await _repo.SaveChangesAsync();
        }

        public async Task<HashSet<PlexEpisode>> ProcessEpsiodes(Metadata[] episodes, IQueryable<PlexEpisode> currentEpisodes)
        {
            var ep = new HashSet<PlexEpisode>();
            try
            {
                foreach (var episode in episodes)
                {
                    // I don't think we need to get the metadata, we only need to get the metadata if we need the provider id (TheTvDbid). Why do we need it for episodes?
                    // We have the parent and grandparent rating keys to link up to the season and series
                    //var metadata = _api.GetEpisodeMetaData(server.PlexAuthToken, server.FullUri, episode.ratingKey);

                    // This does seem to work, it looks like we can somehow get different rating, grandparent and parent keys with episodes. Not sure how.
                    var epExists = currentEpisodes.Any(x => episode.ratingKey == x.Key &&
                                                              episode.grandparentRatingKey == x.GrandparentKey);
                    if (epExists)
                    {
                        continue;
                    }

                    // Let's check if we have the parent
                    var seriesExists = await _repo.GetByKey(episode.grandparentRatingKey);
                    if (seriesExists == null)
                    {
                        // Ok let's try and match it to a title. TODO (This is experimental)
                        seriesExists = await _repo.GetAll().FirstOrDefaultAsync(x =>
                            x.Title.Equals(episode.grandparentTitle, StringComparison.CurrentCultureIgnoreCase));
                        if (seriesExists == null)
                        {
                            _log.LogWarning(
                                "The episode title {0} we cannot find the parent series. The episode grandparentKey = {1}, grandparentTitle = {2}",
                                episode.title, episode.grandparentRatingKey, episode.grandparentTitle);
                            continue;
                        }

                        // Set the rating key to the correct one
                        episode.grandparentRatingKey = seriesExists.Key;
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
                return ep;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _repo?.Dispose();
                _settings?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}