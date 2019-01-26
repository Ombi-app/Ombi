using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;

namespace Ombi.Core.Engine.V2
{
    public interface IMultiSearchEngine
    {
        Task<List<MultiSearch>> MultiSearch(string searchTerm, string lang = "en");
    }
}