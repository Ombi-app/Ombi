
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Helpers;
using TraktSharp;
using TraktSharp.Entities;
using TraktSharp.Enums;

namespace Ombi.Api.Trakt
{
    public class TraktApi : ITraktApi
    {
        private TraktClient Client { get; }

        private static readonly string Encrypted = "MTM0ZTU2ODM1MGY3NDI3NTExZTI1N2E2NTM0MDI2NjYwNDgwY2Y5YjkzYzc3ZjczNzhmMzQwNjAzYjY3MzgxZA==";
        private readonly string _apiKey = StringCipher.DecryptString(Encrypted, "ApiKey");
        public TraktApi()
        {
            Client = new TraktClient
            {
                Authentication =
                {
                    ClientId = _apiKey,
                    ClientSecret = "7beeb6f1c0c842341f6bd285adb86200cb810e431fff0a8a1ed7158af4cffbbb"
                }
            };
        }

        public async Task<IEnumerable<TraktShow>> GetPopularShows(int? page = null, int? limitPerPage = null)
        {
            var popular = await Client.Shows.GetPopularShowsAsync(TraktExtendedOption.Full, page, limitPerPage);
            return popular;
        }

        public async Task<IEnumerable<TraktShow>> GetTrendingShows(int? page = null, int? limitPerPage = null)
        {
            var trendingShowsTop10 = await Client.Shows.GetTrendingShowsAsync(TraktExtendedOption.Full, page, limitPerPage);
            return trendingShowsTop10.Select(x => x.Show);
        }

        public async Task<IEnumerable<TraktShow>> GetAnticipatedShows(int? page = null, int? limitPerPage = null)
        {
            var anticipatedShows = await Client.Shows.GetAnticipatedShowsAsync(TraktExtendedOption.Full, page, limitPerPage);
            return anticipatedShows.Select(x => x.Show);
        }


        public async Task<TraktShow> GetTvExtendedInfo(string imdbId)
        {
            try
            {
                return await Client.Shows.GetShowAsync(imdbId, TraktExtendedOption.Full);
            }
            catch (Exception e)
            {
                // Ignore the exception since the information returned from this API is optional.
                Console.WriteLine($"Failed to retrieve extended tv information from Trakt. IMDbId: '{imdbId}'.");
                Console.WriteLine(e);
            }

            return null;
        }
    }
}

