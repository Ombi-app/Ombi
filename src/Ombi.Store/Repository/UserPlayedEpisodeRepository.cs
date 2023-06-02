using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class UserPlayedEpisodeRepository : ExternalRepository<UserPlayedEpisode>, IUserPlayedEpisodeRepository
    {
        protected ExternalContext Db { get; }
        public UserPlayedEpisodeRepository(ExternalContext db) : base(db)
        {
            Db = db;
        }

        public async Task<UserPlayedEpisode> Get(int theMovieDbId, int seasonNumber, int episodeNumber, string userId)
        {
            return await Db.UserPlayedEpisode.FirstOrDefaultAsync(x => x.TheMovieDbId == theMovieDbId && x.SeasonNumber == seasonNumber && x.EpisodeNumber == episodeNumber && x.UserId == userId);
        }
    }
}