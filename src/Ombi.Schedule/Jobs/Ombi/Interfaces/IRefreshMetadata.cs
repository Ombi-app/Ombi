using Ombi.Store.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IRefreshMetadata : IBaseJob
    {
        Task<string> GetTheMovieDbId(bool hasTvDbId, bool hasImdb, string tvdbID, string imdbId, string title, bool movie);
    }
}