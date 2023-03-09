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
    public class UserPlayedMovieRepository : ExternalRepository<UserPlayedMovie>, IUserPlayedMovieRepository
    {
        protected ExternalContext Db { get; }
        public UserPlayedMovieRepository(ExternalContext db) : base(db)
        {
            Db = db;
        }

        public async Task<UserPlayedMovie> Get(string theMovieDbId, string userId)
        {
            return await Db.UserPlayedMovie.FirstOrDefaultAsync(x => x.TheMovieDbId == theMovieDbId && x.UserId == userId);
       
        }
    }
}