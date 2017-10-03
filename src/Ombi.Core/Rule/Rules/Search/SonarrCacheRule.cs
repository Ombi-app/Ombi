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
                // Check if it's in Radarr
                var result = await _ctx.SonarrCache.FirstOrDefaultAsync(x => x.TvDbId == obj.Id);
                if (result != null)
                {
                    obj.Approved =
                        true; // It's in radarr so it's approved... Maybe have a new property called "Processing" or something?
                }
            }
            return Success();
        }
    }
}