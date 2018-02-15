using System;
using System.Collections.Generic;
using Ombi.Core.Models;

namespace Ombi.Core.Engine
{
    public interface IRecentlyAddedEngine
    {
        IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies();
        IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies(DateTime from, DateTime to);
    }
}