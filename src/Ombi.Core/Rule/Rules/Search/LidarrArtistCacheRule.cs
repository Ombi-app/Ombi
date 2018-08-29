﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class LidarrArtistCacheRule : SpecificRule, ISpecificRule<object>
    {
        public LidarrArtistCacheRule(IRepository<LidarrArtistCache> db)
        {
            _db = db;
        }

        private readonly IRepository<LidarrArtistCache> _db;

        public Task<RuleResult> Execute(object objec)
        {
            var obj = (SearchArtistViewModel) objec;
            // Check if it's in Lidarr
            var result = _db.GetAll().FirstOrDefault(x => x.ForeignArtistId.Equals(obj.ForignArtistId, StringComparison.InvariantCultureIgnoreCase));
            if (result != null)
            {
                obj.Monitored = true; // It's in Lidarr so it's monitored
            }

            return Task.FromResult(Success());
        }

        public override SpecificRules Rule => SpecificRules.LidarrArtist;
    }
}