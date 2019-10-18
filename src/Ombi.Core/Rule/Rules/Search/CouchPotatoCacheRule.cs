using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class CouchPotatoCacheRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public CouchPotatoCacheRule(IExternalRepository<CouchPotatoCache> ctx)
        {
            _ctx = ctx;
        }

        private readonly IExternalRepository<CouchPotatoCache> _ctx;

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            if (obj.Type == RequestType.Movie)
            {
                // Check if it's in Radarr
                var result = await _ctx.FirstOrDefaultAsync(x => x.TheMovieDbId == obj.Id);
                if (result != null)
                {
                    obj.Approved =
                        true; // It's in cp so it's approved... Maybe have a new property called "Processing" or something?
                }
            }
            return Success();
        }
    }
}