using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Helpers;
using TraktApiSharp;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Get.Shows;
using TraktApiSharp.Objects.Get.Shows.Common;
using TraktApiSharp.Requests.Parameters;

namespace Ombi.Api.Trakt
{
    public class TraktApi : ITraktApi
    {
        private TraktClient Client { get; }

        private static readonly string Encrypted = "MTM0ZTU2ODM1MGY3NDI3NTExZTI1N2E2NTM0MDI2NjYwNDgwY2Y5YjkzYzc3ZjczNzhmMzQwNjAzYjY3MzgxZA==";
        private readonly string _apiKey = StringCipher.DecryptString(Encrypted, "ApiKey");
        public TraktApi()
        {
            Client = new TraktClient(_apiKey);
        }

        public async Task<IEnumerable<TraktShow>> GetPopularShows(int? page = null, int? limitPerPage = null)
        {
            var popular = await Client.Shows.GetPopularShowsAsync(new TraktExtendedInfo { Full = true, Images = true}, null, page ?? 1, limitPerPage ?? 10);
            return popular.Value;
        }

        public async Task<IEnumerable<TraktTrendingShow>> GetTrendingShows(int? page = null, int? limitPerPage = null)
        {
            var trendingShowsTop10 = await Client.Shows.GetTrendingShowsAsync(new TraktExtendedInfo { Full = true, Images = true }, null, page ?? 1, limitPerPage ?? 10);
            return trendingShowsTop10.Value;
        }

        public async Task<IEnumerable<TraktMostAnticipatedShow>> GetAnticipatedShows(int? page = null, int? limitPerPage = null)
        {
            var anticipatedShows = await Client.Shows.GetMostAnticipatedShowsAsync(new TraktExtendedInfo { Full = true, Images = true }, null, page ?? 1, limitPerPage ?? 10);
            return anticipatedShows.Value;
        }

        public async Task<IEnumerable<TraktMostWatchedShow>> GetMostWatchesShows(TraktTimePeriod period = null, int? page = null, int? limitPerPage = null)
        {
            var anticipatedShows = await Client.Shows.GetMostWatchedShowsAsync(period ?? TraktTimePeriod.Monthly, new TraktExtendedInfo { Full = true, Images = true }, null, page ?? 1, limitPerPage ?? 10);
            return anticipatedShows.Value;
        }
    }
}

