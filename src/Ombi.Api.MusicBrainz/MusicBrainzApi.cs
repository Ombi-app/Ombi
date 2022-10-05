using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;
using Newtonsoft.Json;
using Ombi.Api.MusicBrainz.Models;

namespace Ombi.Api.MusicBrainz
{
    public class MusicBrainzApi : IMusicBrainzApi
    {
        private readonly MusicBrainzClient _client;
        private readonly IApi _api;

        public MusicBrainzApi(MusicBrainzClient client, IApi api)
        {
            _client = client;
            _api = api;
        }

        public Task<Release> GetAlbumInformation(string albumId)
        {
            var album = _client.Releases.GetAsync(albumId);
            return album;
        }

        public async Task<IEnumerable<Artist>> SearchArtist(string artistQuery)
        {
            var artist = await _client.Artists.SearchAsync(artistQuery, 10);
            return artist.Items.Where(x => x.Type != null);
        }

        public async Task<Artist> GetArtistInformation(string artistId)
        {
            var artist = await _client.Artists.GetAsync(artistId, "artist-rels", "url-rels", "releases", "release-groups");
            return artist;
        }

        public async Task<IEnumerable<Release>> GetReleaseForArtist(string artistId)
        {
            // Build an advanced query to search for the release.
            var query = new QueryParameters<Release>()
            {
                { "arid", artistId },
                { "status", "official" }
            };

            // Search for a release by title.
            var releases = await _client.Releases.SearchAsync(query);
            return releases.Items;
        }

        public async Task<ReleaseGroupArt> GetCoverArtForReleaseGroup(string musicBrainzId, CancellationToken token)
        {
            var request = new Request($"release-group/{musicBrainzId}", "http://coverartarchive.org", HttpMethod.Get);
            var result = await _api.Request(request, token);
            if (result.IsSuccessStatusCode)
            {
                var jsonContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReleaseGroupArt>(jsonContent, Api.Settings);
            }
            return null;
        }
    }
}
