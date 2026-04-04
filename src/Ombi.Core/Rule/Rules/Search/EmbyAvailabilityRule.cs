using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class EmbyAvailabilityRule : MediaServerAvailabilityRule
    {
        private readonly IEmbyContentRepository _repo;

        public EmbyAvailabilityRule(
            IEmbyContentRepository repo,
            ILogger<EmbyAvailabilityRule> log,
            IFeatureService featureService,
            ISettingsService<RadarrSettings> radarrSettings = null,
            ISettingsService<SonarrSettings> sonarrSettings = null)
            : base(log, featureService, radarrSettings, sonarrSettings)
        {
            _repo = repo;
        }

        protected override async Task<ContentLookupResult> FindContent(SearchViewModel obj)
        {
            var result = new ContentLookupResult();
            EmbyContent item = null;

            if (obj.ImdbId.HasValue())
            {
                item = await _repo.GetByImdbId(obj.ImdbId);
                if (item != null)
                {
                    result.UseImdb = true;
                }
            }

            if (item == null)
            {
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await _repo.GetByTheMovieDbId(obj.TheMovieDbId);
                    if (item != null)
                    {
                        result.UseTheMovieDb = true;
                    }
                }

                if (item == null && obj.TheTvDbId.HasValue())
                {
                    item = await _repo.GetByTvDbId(obj.TheTvDbId);
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
            return _repo.GetAllEpisodes().Include(x => x.Series);
        }

        protected override void SetMediaServerUrl(SearchViewModel obj, string url)
        {
            obj.EmbyUrl = url;
        }
    }
}
