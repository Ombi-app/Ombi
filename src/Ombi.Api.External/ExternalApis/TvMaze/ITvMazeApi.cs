using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.External.ExternalApis.TvMaze.Models;
using Ombi.Api.External.ExternalApis.TvMaze.Models.V2;

namespace Ombi.Api.External.ExternalApis.TvMaze
{
    public interface ITvMazeApi
    {
        Task<IEnumerable<TvMazeEpisodes>> EpisodeLookup(int showId);
        Task<List<TvMazeSearch>> Search(string searchTerm);
        Task<TvMazeShow> ShowLookup(int showId);
        Task<TvMazeShow> ShowLookupByTheTvDbId(int theTvDbId);
    }
}