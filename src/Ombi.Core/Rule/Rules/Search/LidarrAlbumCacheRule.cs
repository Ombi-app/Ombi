using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class LidarrAlbumCacheRule : SpecificRule, ISpecificRule<object>
    {
        public LidarrAlbumCacheRule(IExternalRepository<LidarrAlbumCache> db)
        {
            _db = db;
        }

        private readonly IExternalRepository<LidarrAlbumCache> _db;

        public Task<RuleResult> Execute(object objec)
        {
            var obj = (SearchAlbumViewModel) objec;
            // Check if it's in Lidarr
            var result = _db.GetAll().FirstOrDefault(x => x.ForeignAlbumId.Equals(obj.ForeignAlbumId, StringComparison.InvariantCultureIgnoreCase));
            if (result != null)
            {
                obj.PercentOfTracks = result.PercentOfTracks;
                obj.Monitored = true; // It's in Lidarr so it's monitored
            }

            return Task.FromResult(Success());
        }

        public override SpecificRules Rule => SpecificRules.LidarrAlbum;
    }
}