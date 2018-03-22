using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models;

namespace Ombi.Core.Engine
{
    public interface IRecentlyAddedEngine
    {
        IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies();
        IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies(DateTime from, DateTime to);
        IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(DateTime from, DateTime to, bool groupBySeason);
        IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(bool groupBySeason);
        Task<bool> UpdateRecentlyAddedDatabase();
    }
}