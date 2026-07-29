using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

            if (item.Quality.HasValue() || !item.Has4K || !is4kEnabled)
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
            var seriesEpisodes = new List<IMediaServerEpisode>();

            try
            {
                if (lookup.UseImdb && !string.IsNullOrEmpty(item.ImdbId))
                {
                    seriesEpisodes = await allEpisodes.Where(x => x.Series.ImdbId == item.ImdbId).ToListAsync();
                }
                else if (lookup.UseTheMovieDb && !string.IsNullOrEmpty(item.TheMovieDbId))
                {
                    seriesEpisodes = await allEpisodes.Where(x => x.Series.TheMovieDbId == item.TheMovieDbId).ToListAsync();
                }
                else if (lookup.UseTvDb && !string.IsNullOrEmpty(item.TvDbId))
                {
                    seriesEpisodes = await allEpisodes.Where(x => x.Series.TvDbId == item.TvDbId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Exception thrown when pre-fetching series episodes for availability check");
            }

            foreach (var season in search.SeasonRequests)
            {
                foreach (var episode in season.Episodes.Where(e => seriesEpisodes.Any(x => x.EpisodeNumber == e.EpisodeNumber && x.SeasonNumber == season.SeasonNumber)))
                {
                    episode.Available = true;
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

        /// <summary>
        /// Shared content lookup by IMDB, TheMovieDb, and TvDb IDs.
        /// Used by media servers that expose separate GetByImdbId/GetByTheMovieDbId/GetByTvDbId methods.
        /// </summary>
        protected static async Task<ContentLookupResult> FindContentByProviderIds(
            SearchViewModel obj,
            Func<string, Task<IMediaServerContent>> getByImdbId,
            Func<string, Task<IMediaServerContent>> getByTheMovieDbId,
            Func<string, Task<IMediaServerContent>> getByTvDbId,
            bool lookupById = false)
        {
            var result = new ContentLookupResult();
            IMediaServerContent item = null;

            // TheMovieDb (and potentially other providers) use separate ID namespaces for
            // movies and TV shows, so the same ID can refer to both a movie and a series.
            // Only accept content whose type matches the thing we are searching for, otherwise
            // a movie can be wrongly marked as available because a series shares its ID (and vice versa).
            var expectedType = obj is SearchMovieViewModel ? MediaType.Movie : MediaType.Series;
            bool Matches(IMediaServerContent content) => content != null && content.Type == expectedType;

            if (obj.ImdbId.HasValue())
            {
                var match = await getByImdbId(obj.ImdbId);
                if (Matches(match))
                {
                    item = match;
                    result.UseImdb = true;
                }
            }

            if (item == null)
            {
                if (lookupById && obj.Id > 0)
                {
                    var match = await getByTheMovieDbId(obj.Id.ToString());
                    if (Matches(match))
                    {
                        item = match;
                        obj.TheMovieDbId = obj.Id.ToString();
                        result.UseTheMovieDb = true;
                    }
                }

                if (item == null && obj.TheMovieDbId.HasValue())
                {
                    var match = await getByTheMovieDbId(obj.TheMovieDbId);
                    if (Matches(match))
                    {
                        item = match;
                        result.UseTheMovieDb = true;
                    }
                }

                if (item == null && obj.TheTvDbId.HasValue())
                {
                    var match = await getByTvDbId(obj.TheTvDbId);
                    if (Matches(match))
                    {
                        item = match;
                        result.UseTvDb = true;
                    }
                }
            }

            result.Content = item;
            return result;
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
