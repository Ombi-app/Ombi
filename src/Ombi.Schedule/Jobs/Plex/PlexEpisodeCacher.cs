using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.Server;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Serilog;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexEpisodeCacher : IPlexEpisodeCacher
    {
        public PlexEpisodeCacher(ISettingsService<PlexSettings> s, ILogger<PlexEpisodeCacher> log, IPlexApi plexApi,
            IPlexContentRepository repo)
        {
            _settings = s;
            _log = log;
            _api = plexApi;
            _repo = repo;
        }

        private readonly ISettingsService<PlexSettings> _settings;
        private readonly ILogger<PlexEpisodeCacher> _log;
        private readonly IPlexApi _api;
        private readonly IPlexContentRepository _repo;
        
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
        }

        private async Task Cache(PlexServers settings)
        {
            if (!Validate(settings))
            {
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

            // Get the first 50
            var currentPosition = 0;
            var ResultCount = 50;
            var episodes = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition, ResultCount);

            _log.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Total Epsiodes found for {episodes.MediaContainer.librarySectionTitle} = {episodes.MediaContainer.totalSize}");

            await ProcessEpsiodes(episodes);
            currentPosition += ResultCount;

            while (currentPosition < episodes.MediaContainer.totalSize)
            {
                var ep = await _api.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, section.key, currentPosition,
                    ResultCount);
                await ProcessEpsiodes(ep);
                _log.LogInformation(LoggingEvents.PlexEpisodeCacher, $"Processed {ResultCount} more episodes. Total Remaining {currentPosition - episodes.MediaContainer.totalSize}");
                currentPosition += ResultCount;
            }

        }

        private async Task ProcessEpsiodes(PlexContainer episodes)
        {
            var ep = new HashSet<PlexEpisode>();
            foreach (var episode in episodes.MediaContainer.Metadata)
            {
                // I don't think we need to get the metadata, we only need to get the metadata if we need the provider id (TheTvDbid). Why do we need it for episodes?
                // We have the parent and grandparent rating keys to link up to the season and series
                //var metadata = _api.GetEpisodeMetaData(server.PlexAuthToken, server.FullUri, episode.ratingKey);

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