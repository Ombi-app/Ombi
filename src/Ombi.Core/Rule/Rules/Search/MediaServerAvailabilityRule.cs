using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;

namespace Ombi.Core.Rule.Rules.Search
{
    public abstract class MediaServerAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        private readonly IFeatureService _featureService;
        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<SonarrSettings> _sonarrSettings;

        protected ILogger Log { get; }

        protected MediaServerAvailabilityRule(
            ILogger log,
            IFeatureService featureService,
            ISettingsService<RadarrSettings> radarrSettings,
            ISettingsService<SonarrSettings> sonarrSettings)
        {
            Log = log;
            _featureService = featureService;
            _radarrSettings = radarrSettings;
            _sonarrSettings = sonarrSettings;
        }

        /// <summary>
        /// Looks up content in the media server by provider IDs.
        /// Returns the content item and which provider ID types matched.
        /// </summary>
        protected abstract Task<ContentLookupResult> FindContent(SearchViewModel obj);

        /// <summary>
        /// Gets all episodes from the media server repository.
        /// </summary>
        protected abstract IQueryable<IMediaServerEpisode> GetAllEpisodes();

        /// <summary>
        /// Sets the media server URL on the search view model (PlexUrl, EmbyUrl, or JellyfinUrl).
        /// </summary>
        protected abstract Task SetMediaServerUrl(SearchViewModel obj, string url);

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            var lookup = await FindContent(obj);
            if (lookup?.Content == null)
            {
                return Success();
            }

            var item = lookup.Content;

            await SetMediaServerUrl(obj, item.Url);

            if (obj is SearchMovieViewModel movie)
            {
                await SetMovieAvailability(movie, item);
            }
            else
            {
                await SetTvShowAvailability(obj);
            }

            if (obj is SearchTvShowViewModel search)
            {
                await CheckEpisodeAvailability(search, lookup, item);
            }

            return Success();
        }

        private async Task SetMovieAvailability(SearchMovieViewModel movie, IMediaServerContent item)
        {
            if (await ShouldDeferToRadarr())
            {
                return;
            }

            var is4kEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);

            if (item.Has4K && is4kEnabled)
            {
                movie.Available4K = true;
            }
            else
            {
                movie.Available = true;
                movie.Quality = item.Quality;
            }

            if (item.Quality.HasValue())
            {
                movie.Available = true;
                movie.Quality = item.Quality;
            }
        }

        private async Task SetTvShowAvailability(SearchViewModel obj)
        {
            if (!await ShouldDeferToSonarr())
            {
                obj.Available = true;
            }
        }

        private async Task CheckEpisodeAvailability(SearchTvShowViewModel search, ContentLookupResult lookup, IMediaServerContent item)
        {
            if (!search.SeasonRequests.Any())
            {
                return;
            }

            var allEpisodes = GetAllEpisodes();
            foreach (var season in search.SeasonRequests.ToList())
            {
                foreach (var episode in season.Episodes.ToList())
                {
                    await AvailabilityRuleHelper.SingleEpisodeCheck(
                        lookup.UseImdb, allEpisodes, episode, season, item,
                        lookup.UseTheMovieDb, lookup.UseTvDb, Log);
                }
            }

            AvailabilityRuleHelper.CheckForUnairedEpisodes(search);
        }

        private async Task<bool> ShouldDeferToRadarr()
        {
            if (_radarrSettings == null)
            {
                return false;
            }

            var settings = await _radarrSettings.GetSettingsAsync();
            return settings != null && settings.Enabled &&
                   settings.ScanForAvailability && settings.PrioritizeArrAvailability;
        }

        private async Task<bool> ShouldDeferToSonarr()
        {
            if (_sonarrSettings == null)
            {
                return false;
            }

            var settings = await _sonarrSettings.GetSettingsAsync();
            return settings != null && settings.Enabled &&
                   settings.ScanForAvailability && settings.PrioritizeArrAvailability;
        }
    }

    /// <summary>
    /// Result of looking up content in a media server, including which provider IDs were used for matching.
    /// </summary>
    public class ContentLookupResult
    {
        public IMediaServerContent Content { get; set; }
        public bool UseImdb { get; set; }
        public bool UseTheMovieDb { get; set; }
        public bool UseTvDb { get; set; }
    }
}
