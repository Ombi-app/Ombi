using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.TvMaze.Models;
using Ombi.Api.TvMaze.Models.V2;

namespace Ombi.Api.TvMaze
{
    public interface ITvMazeApi
    {
        Task<IEnumerable<TvMazeEpisodes>> EpisodeLookup(int showId);
        Task<List<TvMazeSeasons>> GetSeasons(int id);
        Task<List<TvMazeSearch>> Search(string searchTerm);
        Task<TvMazeShow> ShowLookup(int showId);
        Task<TvMazeShow> ShowLookupByTheTvDbId(int theTvDbId);
        Task<FullSearch> GetTvFullInformation(int id); 
    }
}