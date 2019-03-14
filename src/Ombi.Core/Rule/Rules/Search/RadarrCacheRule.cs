using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class RadarrCacheRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public RadarrCacheRule(IExternalRepository<RadarrCache> db)
        {
            _db = db;
        }

        private readonly IExternalRepository<RadarrCache> _db;

        public Task<RuleResult> Execute(SearchViewModel obj)
        {
            if (obj.Type == RequestType.Movie)
            {
                // Check if it's in Radarr
                var result = _db.GetAll().FirstOrDefault(x => x.TheMovieDbId == obj.Id);
                if (result != null)
                {
                    obj.Approved = true; // It's in radarr so it's approved... Maybe have a new property called "Processing" or something?
                    if (result.HasFile)
                    {
                        obj.Available = true;
                    }
                }
            }
            return Task.FromResult(Success());
        }
    }
}