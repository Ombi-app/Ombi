using System.Collections.Generic;
using System.Threading.Tasks;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Get.Shows;
using TraktApiSharp.Objects.Get.Shows.Common;

namespace Ombi.Api.Trakt
{
    public interface ITraktApi
    {
        Task<IEnumerable<TraktMostAnticipatedShow>> GetAnticipatedShows(int? page = default(int?), int? limitPerPage = default(int?));
        Task<IEnumerable<TraktMostWatchedShow>> GetMostWatchesShows(TraktTimePeriod period = null, int? page = default(int?), int? limitPerPage = default(int?));
        Task<IEnumerable<TraktShow>> GetPopularShows(int? page = default(int?), int? limitPerPage = default(int?));
        Task<IEnumerable<TraktTrendingShow>> GetTrendingShows(int? page = default(int?), int? limitPerPage = default(int?));
    }
}