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

        private static readonly string Encrypted = "134e568350f7427511e257a6534026660480cf9b93c77f7378f340603b67381d";
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
