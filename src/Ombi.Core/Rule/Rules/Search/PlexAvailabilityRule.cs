using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class PlexAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly IFeatureService _featureService;
        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<SonarrSettings> _sonarrSettings;

        public PlexAvailabilityRule(IPlexContentRepository repo, ILogger<PlexAvailabilityRule> log, ISettingsService<PlexSettings> plexSettings,
            IFeatureService featureService, ISettingsService<RadarrSettings> radarrSettings = null, ISettingsService<SonarrSettings> sonarrSettings = null)
        {
            PlexContentRepository = repo;
            Log = log;
            _plexSettings = plexSettings;
            _featureService = featureService;
            _radarrSettings = radarrSettings;
            _sonarrSettings = sonarrSettings;
        }

        private IPlexContentRepository PlexContentRepository { get; }
        private ILogger Log { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            PlexServerContent item = null;
            var useImdb = false;
            var useTheMovieDb = false;
            var useId = false;
            var useTvDb = false;

            MediaType type = ConvertType(obj.Type);

            if (obj.ImdbId.HasValue())
            {
                item = await PlexContentRepository.GetByType(obj.ImdbId, ProviderType.ImdbId, type);
                if (item != null)
                {
                    useImdb = true;
                }
            }
            if (item == null)
            {
                if (obj.Id > 0)
                {
                    item = await PlexContentRepository.GetByType(obj.Id.ToString(), ProviderType.TheMovieDbId, type);
                    if (item != null)
                    {
                        useId = true;
                    }
                }
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await PlexContentRepository.GetByType(obj.TheMovieDbId, ProviderType.TheMovieDbId, type);
                    if (item != null)
                    {
                        useTheMovieDb = true;
                    }
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await PlexContentRepository.GetByType(obj.TheTvDbId, ProviderType.TvDbId, type);
                        if (item != null)
                        {
                            useTvDb = true;
                        }
                    }
                }
            }

            if (item != null)
            {
                var settings = await _plexSettings.GetSettingsAsync();
                var firstServer = settings.Servers.FirstOrDefault();
                var host = string.Empty;
                if (firstServer != null)
                {
                    host = firstServer.ServerHostname;
                }
                if (useId)
                {
                    obj.TheMovieDbId = obj.Id.ToString();
                    useTheMovieDb = true;
                }

                if (obj is SearchMovieViewModel movie)
                {
                    // Check if Radarr has priority - if so, don't mark as available from Plex
                    var radarrHasPriority = await ShouldDeferToRadarr();
                    if (!radarrHasPriority)
                    {
                        var is4kEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);

                        if (item.Has4K && is4kEnabled)
                        {
                            movie.Available4K = true;
                        }
                        else
                        {
                            obj.Available = true;
                            obj.Quality = item.Quality;
                        }

                        if (item.Quality.HasValue())
                        {
                            obj.Available = true;
                            obj.Quality = item.Quality;
                        }
                    }
                }
                else
                {
                    // Check if Sonarr has priority for TV shows
                    var sonarrHasPriority = await ShouldDeferToSonarr();
                    if (!sonarrHasPriority)
                    {
                        obj.Available = true;
                    }
                }

                if (item.Url.StartsWith("http"))
                {
                    obj.PlexUrl = item.Url;
                }
                else
                {
                    // legacy content
                    obj.PlexUrl = PlexHelper.BuildPlexMediaUrl(item.Url, host);
                }

                if (obj is SearchTvShowViewModel search)
                {
                    // Let's go through the episodes now
                    if (search.SeasonRequests.Any())
                    {
                        var allEpisodes = PlexContentRepository.GetAllEpisodes();
                        foreach (var season in search.SeasonRequests.ToList())
                        {
                            foreach (var episode in season.Episodes.ToList())
                            {
                                await AvailabilityRuleHelper.SingleEpisodeCheck(useImdb, allEpisodes, episode, season, item, useTheMovieDb, useTvDb, Log);
                            }
                        }

                        AvailabilityRuleHelper.CheckForUnairedEpisodes(search);
                    }
                }
            }
            return Success();
        }

        private MediaType ConvertType(RequestType type) =>
            type switch
            {
                RequestType.Movie => MediaType.Movie,
                RequestType.TvShow => MediaType.Series,
                _ => MediaType.Movie,
            };

        private async Task<bool> ShouldDeferToRadarr()
        {
            if (_radarrSettings == null)
            {
                return false;
            }

            var radarrSettings = await _radarrSettings.GetSettingsAsync();
            return radarrSettings != null && radarrSettings.Enabled &&
                   radarrSettings.ScanForAvailability && radarrSettings.PrioritizeArrAvailability;
        }

        private async Task<bool> ShouldDeferToSonarr()
        {
            if (_sonarrSettings == null)
            {
                return false;
            }

            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();
            return sonarrSettings != null && sonarrSettings.Enabled &&
                   sonarrSettings.ScanForAvailability && sonarrSettings.PrioritizeArrAvailability;
        }
    }
}