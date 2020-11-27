using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2.Music;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class LidarrAlbumCacheRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public LidarrAlbumCacheRule(IExternalRepository<LidarrAlbumCache> db)
        {
            _db = db;
        }

        private readonly IExternalRepository<LidarrAlbumCache> _db;

        public Task<RuleResult> Execute(SearchViewModel objec)
        {
            if (objec is SearchAlbumViewModel obj)
            {
                // Check if it's in Lidarr
                var result = _db.GetAll().FirstOrDefault(x =>
                    x.ForeignAlbumId == obj.ForeignAlbumId);
                if (result != null)
                {
                    obj.PercentOfTracks = result.PercentOfTracks;
                    obj.Monitored = true; // It's in Lidarr so it's monitored
                }
            }

            if (objec is ReleaseGroup release)
            {
                // Check if it's in Lidarr
                var result = _db.GetAll().FirstOrDefault(x =>
                    x.ForeignAlbumId == release.Id);
                if (result != null)
                {
                    release.PercentOfTracks = result.PercentOfTracks;
                    release.Monitored = true; // It's in Lidarr so it's monitored
                }
            }

            return Task.FromResult(Success());
        }
    }
}