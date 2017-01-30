using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ombi.Api.Interfaces;
using Ombi.Helpers;
using TraktApiSharp;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Get.Shows;
using TraktApiSharp.Objects.Get.Shows.Common;
using TraktApiSharp.Requests.Params;

namespace Ombi.Api
{
    public class TraktApi : ITraktApi
    {
        private TraktClient Client { get; }

        private static readonly string Encrypted = "z/56wM/oEkkCWEvSIZCrzQyUvvqmafQ3njqf0UNK5xuKbNYh5Wz8ocoG2QDa5y1DBkozLaKsGxORmAB1XUvwbnom8DVNo9gE++9GTuwxmGlLDD318PXpRmYmpKqNwFSKRZgF6ewiY9qR4t3iG0pGQwPA08FK3+H7kpOKAGJNR9RMDP9wwB6Vl4DuOiZb9/DETjzZ+/zId0ZqimrbN+PLrg==";
        private readonly string _apiKey = StringCipher.Decrypt(Encrypted, "ApiKey");
        public TraktApi()
        {
            Client = new TraktClient(_apiKey);
        }

        public async Task<IEnumerable<TraktShow>> GetPopularShows(int? page = null, int? limitPerPage = null)
        {
            var popular = await Client.Shows.GetPopularShowsAsync(new TraktExtendedInfo { Full = true }, null, page ?? 1, limitPerPage ?? 10);
            return popular.Items;
        }

        public async Task<IEnumerable<TraktTrendingShow>> GetTrendingShows(int? page = null, int? limitPerPage = null)
        {
            var trendingShowsTop10 = await Client.Shows.GetTrendingShowsAsync(new TraktExtendedInfo { Full = true }, null, page ?? 1, limitPerPage ?? 10);
            return trendingShowsTop10.Items;
        }

        public async Task<IEnumerable<TraktMostAnticipatedShow>> GetAnticipatedShows(int? page = null, int? limitPerPage = null)
        {
            var anticipatedShows = await Client.Shows.GetMostAnticipatedShowsAsync(new TraktExtendedInfo { Full = true }, null, page ?? 1, limitPerPage ?? 10);
            return anticipatedShows.Items;
        }

        public async Task<IEnumerable<TraktMostWatchedShow>> GetMostWatchesShows(TraktTimePeriod period = null, int? page = null, int? limitPerPage = null)
        {
            var anticipatedShows = await Client.Shows.GetMostWatchedShowsAsync(period ?? TraktTimePeriod.Monthly, new TraktExtendedInfo { Full = true }, null, page ?? 1, limitPerPage ?? 10);
            return anticipatedShows.Items;
        }
    }
}
