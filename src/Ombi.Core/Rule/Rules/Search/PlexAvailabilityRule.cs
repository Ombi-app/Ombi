using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class PlexAvailabilityRule : MediaServerAvailabilityRule
    {
        private readonly IPlexContentRepository _repo;
        private readonly ISettingsService<PlexSettings> _plexSettings;

        public PlexAvailabilityRule(
            IPlexContentRepository repo,
            ILogger<PlexAvailabilityRule> log,
            ISettingsService<PlexSettings> plexSettings,
            IFeatureService featureService,
            ISettingsService<RadarrSettings> radarrSettings = null,
            ISettingsService<SonarrSettings> sonarrSettings = null)
            : base(log, featureService, radarrSettings, sonarrSettings)
        {
            _repo = repo;
            _plexSettings = plexSettings;
        }

        protected override async Task<ContentLookupResult> FindContent(SearchViewModel obj)
        {
            var result = new ContentLookupResult();
            PlexServerContent item = null;
            MediaType type = obj.Type switch
            {
                RequestType.Movie => MediaType.Movie,
                RequestType.TvShow => MediaType.Series,
                _ => MediaType.Movie,
            };

            if (obj.ImdbId.HasValue())
            {
                item = await _repo.GetByType(obj.ImdbId, ProviderType.ImdbId, type);
                if (item != null)
                {
                    result.UseImdb = true;
                }
            }

            if (item == null)
            {
                if (obj.Id > 0)
                {
                    item = await _repo.GetByType(obj.Id.ToString(), ProviderType.TheMovieDbId, type);
                    if (item != null)
                    {
                        obj.TheMovieDbId = obj.Id.ToString();
                        result.UseTheMovieDb = true;
                    }
                }

                if (item == null && obj.TheMovieDbId.HasValue())
                {
                    item = await _repo.GetByType(obj.TheMovieDbId, ProviderType.TheMovieDbId, type);
                    if (item != null)
                    {
                        result.UseTheMovieDb = true;
                    }
                }

                if (item == null && obj.TheTvDbId.HasValue())
                {
                    item = await _repo.GetByType(obj.TheTvDbId, ProviderType.TvDbId, type);
                    if (item != null)
                    {
                        result.UseTvDb = true;
                    }
                }
            }

            result.Content = item;
            return result;
        }

        protected override IQueryable<IMediaServerEpisode> GetAllEpisodes()
        {
            return _repo.GetAllEpisodes();
        }

        protected override void SetMediaServerUrl(SearchViewModel obj, string url)
        {
            if (url.StartsWith("http"))
            {
                obj.PlexUrl = url;
            }
            else
            {
                var settings = _plexSettings.GetSettingsAsync().Result;
                var host = settings?.Servers?.FirstOrDefault()?.ServerHostname ?? string.Empty;
                obj.PlexUrl = PlexHelper.BuildPlexMediaUrl(url, host);
            }
        }
    }
}
