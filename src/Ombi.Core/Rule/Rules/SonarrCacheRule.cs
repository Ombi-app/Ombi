using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules
{
    public class SonarrCacheRule
    {
        public SonarrCacheRule(IExternalContext ctx)
        {
            _ctx = ctx;
        }

        private readonly IExternalContext _ctx;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            if (obj.RequestType == RequestType.TvShow)
            {
                var vm = (ChildRequests) obj;
                var result = await _ctx.SonarrCache.FirstOrDefaultAsync(x => x.TvDbId == vm.Id);
                if (result != null)
                {
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
                                    return new RuleResult{Message = "We already have this request, please choose the \"Select...\" option to refine your request"};
                                }
                            }
                        }
                    }
                }
            }
            return new RuleResult { Success = true };
        }

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
                                var monitoredInSonarr = await sonarrEpisodes.FirstOrDefaultAsync(x =>
                                    x.EpisodeNumber == ep.EpisodeNumber && x.SeasonNumber == season.SeasonNumber
                                    && x.TvDbId == vm.Id);
                                if (monitoredInSonarr != null)
                                {
                                    ep.Approved = true;
                                    if (monitoredInSonarr.HasFile)
                                    {
                                        obj.Available = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new RuleResult { Success = true };
        }
    }
}