using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IUserPlayedMovieRepository : IExternalRepository<UserPlayedMovie>
    {
        Task<UserPlayedMovie> Get(string theMovieDbId, string userId);
    }
}