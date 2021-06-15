using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules
{
    public class SonarrCacheRule
    {
        public SonarrCacheRule(ExternalContext ctx)
        {
            _ctx = ctx;
        }

        private readonly ExternalContext _ctx;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            if (obj.RequestType == RequestType.TvShow)
            {
                var vm = (ChildRequests) obj;
                var result = await _ctx.SonarrCache.FirstOrDefaultAsync(x => x.TheMovieDbId == vm.Id);
                if (result != null)
                {
                    if (vm.SeasonRequests.Any())
                    {
                        var sonarrEpisodes = _ctx.SonarrEpisodeCache;
                        foreach (var season in vm.SeasonRequests)
                        {
                            var toRemove = new List<EpisodeRequests>();
                            foreach (var ep in season.Episodes)
                            {
                                // Check if we have it
                                var monitoredInSonarr = sonarrEpisodes.FirstOrDefault(x =>
                                    x.EpisodeNumber == ep.EpisodeNumber && x.SeasonNumber == season.SeasonNumber
                                    && x.MovieDbId == vm.Id);
                                if (monitoredInSonarr != null)
                                {
                                    toRemove.Add(ep);
                                                                   }
                            }

                            toRemove.ForEach(x =>
                            {
                                season.Episodes.Remove(x);
                            });

                        }
                        var anyEpisodes = vm.SeasonRequests.SelectMany(x => x.Episodes).Any();

                        if (!anyEpisodes)
                        {
                            return new RuleResult { Message = $"We already have episodes requested from series {vm.Title}" };
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
                // Check if it's in Sonarr
                if (!vm.TheTvDbId.HasValue())
                {
                    return new RuleResult { Success = true };
                }
                var tvdbidint = int.Parse(vm.TheTvDbId);
                var result = await _ctx.SonarrCache.FirstOrDefaultAsync(x => x.TvDbId == tvdbidint);
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
                                    && x.TvDbId == tvdbidint);
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