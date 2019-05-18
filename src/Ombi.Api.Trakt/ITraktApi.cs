using System.Collections.Generic;
using System.Threading.Tasks;
using TraktSharp.Entities;

namespace Ombi.Api.Trakt
{
    public interface ITraktApi
    {
        Task<IEnumerable<TraktShow>> GetAnticipatedShows(int? page = default(int?), int? limitPerPage = default(int?));
        Task<IEnumerable<TraktShow>> GetPopularShows(int? page = default(int?), int? limitPerPage = default(int?));
        Task<IEnumerable<TraktShow>> GetTrendingShows(int? page = default(int?), int? limitPerPage = default(int?));
        Task<TraktShow> GetTvExtendedInfo(string imdbId);
    }
}