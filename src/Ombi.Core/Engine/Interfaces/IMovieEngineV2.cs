using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core
{
    public interface IMovieEngineV2
    {
        Task<MovieFullInfoViewModel> GetFullMovieInformation(int theMovieDbId, string langCode = null);
    }
}