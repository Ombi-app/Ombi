using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api;
using Ombi.Api.MusicBrainz.Models;
using Ombi.Api.MusicBrainz.Models.Lookup;
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

        public async Task<IEnumerable<ReleaseGroups>> GetReleaseGroups(string artistId)
        {
            var request = new Request("release-group", _baseUrl, HttpMethod.Get);

            request.AddQueryString("artist", artistId);
            AddHeaders(request);

            // The count properties for release groups is called releasegroupcount... Will sort this out if I need paging
            var releases = await _api.Request<MusicBrainzResult<ReleaseGroups>>(request);
            return releases.Data;
        }

        private void AddHeaders(Request req)
        {
            req.AddHeader("Accept", "application/json");
        }
    }
}
