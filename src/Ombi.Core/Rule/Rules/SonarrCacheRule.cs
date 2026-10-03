using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine;
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
                if (result != null && vm.SeasonRequests.Any())
                {
                    var seriesEpisodes = await _ctx.SonarrEpisodeCache
                        .Where(x => x.MovieDbId == vm.Id)
                        .ToListAsync();
                    var episodeSet = seriesEpisodes
                        .Select(x => (x.SeasonNumber, x.EpisodeNumber))
                        .ToHashSet();

                    foreach (var season in vm.SeasonRequests)
                    {
                        season.Episodes.RemoveAll(ep => episodeSet.Contains((season.SeasonNumber, ep.EpisodeNumber)));
                    }

                    var anyEpisodes = vm.SeasonRequests.SelectMany(x => x.Episodes).Any();

                    if (!anyEpisodes)
                    {
                        return new RuleResult { ErrorCode = ErrorCode.EpisodesAlreadyRequested, Message = $"We already have episodes requested from series {vm.Title}" };
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
                        var seriesEpisodes = await _ctx.SonarrEpisodeCache
                            .Where(x => x.TvDbId == tvdbidint)
                            .ToListAsync();

                        var episodeLookup = new Dictionary<(int Season, int Episode), SonarrEpisodeCache>();
                        foreach (var ep in seriesEpisodes)
                        {
                            if (!episodeLookup.TryGetValue((ep.SeasonNumber, ep.EpisodeNumber), out var existing) || (!existing.HasFile && ep.HasFile))
                            {
                                episodeLookup[(ep.SeasonNumber, ep.EpisodeNumber)] = ep;
                            }
                        }

                        foreach (var season in vm.SeasonRequests)
                        {
                            foreach (var ep in season.Episodes)
                            {
                                if (episodeLookup.TryGetValue((season.SeasonNumber, ep.EpisodeNumber), out var monitoredInSonarr))
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