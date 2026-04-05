using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class JellyfinAvailabilityRule : MediaServerAvailabilityRule
    {
        private readonly IJellyfinContentRepository _repo;

        public JellyfinAvailabilityRule(
            IJellyfinContentRepository repo,
            ILogger<JellyfinAvailabilityRule> log,
            IFeatureService featureService,
            ISettingsService<RadarrSettings> radarrSettings = null,
            ISettingsService<SonarrSettings> sonarrSettings = null)
            : base(log, featureService, radarrSettings, sonarrSettings)
        {
            _repo = repo;
        }

        protected override async Task<ContentLookupResult> FindContent(SearchViewModel obj)
        {
            return await FindContentByProviderIds(obj,
                async id => await _repo.GetByImdbId(id),
                async id => await _repo.GetByTheMovieDbId(id),
                async id => await _repo.GetByTvDbId(id),
                lookupById: true);
        }

        protected override IQueryable<IMediaServerEpisode> GetAllEpisodes()
        {
            return _repo.GetAllEpisodes().Include(x => x.Series);
        }

        protected override Task SetMediaServerUrl(SearchViewModel obj, string url)
        {
            obj.JellyfinUrl = url;
            return Task.CompletedTask;
        }
    }
}
