using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Ombi.Api.MusicBrainz.Models;
using Ombi.Api.MusicBrainz.Models.Artist;
using Ombi.Api.MusicBrainz.Models.Browse;
using Ombi.Api.MusicBrainz.Models.Search;

namespace Ombi.Api.MusicBrainz
{
    public class MusicBrainzApi : IMusicBrainzApi
    {
        public MusicBrainzApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;
        private const string _baseUrl = "https://musicbrainz.org/ws/2/";

        public async Task<IEnumerable<Artist>> SearchArtist(string artistQuery)
        {
            var request = new Request("artist", _baseUrl, HttpMethod.Get);

            request.AddQueryString("query", artistQuery);
            AddHeaders(request);
            var albums = await _api.Request<MusicBrainzResult<Artist>>(request);
            return albums.Data.Where(x => !x.type.Equals("Person", StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<ArtistInformation> GetArtistInformation(string artistId)
        {
            var request = new Request($"artist/{artistId}", _baseUrl, HttpMethod.Get);
            AddHeaders(request);

            return await _api.Request<ArtistInformation>(request);
        }

        public async Task<IEnumerable<Release>> GetReleaseForArtist(string artistId)
        {
            var request = new Request("release", _baseUrl, HttpMethod.Get);

            request.AddQueryString("artist", artistId);
            request.AddQueryString("inc", "recordings");
            AddHeaders(request);

            var releases = await _api.Request<ReleaseResult>(request);
            return releases.releases;
        }

        private void AddHeaders(Request req)
        {
            req.AddHeader("Accept", "application/json");
        }
    }
}
