using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Core.Rule.Rules.Search
{
    public class SonarrCacheRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public SonarrCacheRule(IOmbiContext ctx)
        {
            _ctx = ctx;
        }

        private readonly IOmbiContext _ctx;

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            if (obj.Type == RequestType.TvShow)
            {
                var vm = (SearchTvShowViewModel) obj;
                // Check if it's in Radarr
                var result = await _ctx.SonarrCache.FirstOrDefaultAsync(x => x.TvDbId == vm.Id);
                if (result != null)
                {
                    vm.Approved = true;

                    if (vm.SeasonRequests.Any())
                    {
                        var sonarrEpisodes = _ctx.SonarrEpisodeCache;
                        foreach (var season in vm.SeasonRequests)
                        {
                            foreach (var ep in season.Episodes)
                            {
                                // Check if we have it
                                var monitoredInSonarr = sonarrEpisodes.Any(x =>
                                    x.EpisodeNumber == ep.EpisodeNumber && x.SeasonNumber == season.SeasonNumber
                                    && x.TvDbId == vm.Id);
                                if (monitoredInSonarr)
                                {
                                    ep.Approved = true;
                                }
                            }
                        }
                    }
                }
            }
            return Success();
        }
    }
}