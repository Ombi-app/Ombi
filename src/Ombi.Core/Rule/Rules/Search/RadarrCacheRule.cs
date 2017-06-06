using System.Linq;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Context;

namespace Ombi.Core.Rule.Rules.Search
{
    public class RadarrCacheRule : BaseSearchRule, IRequestRules<SearchViewModel>
    {
        public RadarrCacheRule(IOmbiContext ctx)
        {
            _ctx = ctx;
        }

        private readonly IOmbiContext _ctx;

        public RuleResult Execute(SearchViewModel obj)
        {
           // Check if it's in Radarr

            var result = _ctx.RadarrCache.FirstOrDefault(x => x.TheMovieDbId == obj.Id);
            if (result != null)
            {
                obj.Approved = true; // It's in radarr so it's approved... Maybe have a new property called "Processing" or something?
            }

            return Success();
        }
    }
}