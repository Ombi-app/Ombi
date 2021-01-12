using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;
using Hqub.MusicBrainz.API.Entities.Collections;
using Newtonsoft.Json;
using Ombi.Api.MusicBrainz.Models;

namespace Ombi.Api.MusicBrainz
{
    public class MusicBrainzApi : IMusicBrainzApi
    {
        private readonly IApi _api;

        public MusicBrainzApi(IApi api)
        {
            _api = api;
        }

        public Task<Release> GetAlbumInformation(string albumId)
        {
            var album = Release.GetAsync(albumId);
            return album;
        }

        public async Task<IEnumerable<Artist>> SearchArtist(string artistQuery)
        {
            var artist = await Artist.SearchAsync(artistQuery, 10);
            return artist.Items.Where(x => x.Type != null);
        }

        public async Task<Artist> GetArtistInformation(string artistId)
        {
            var artist = await Artist.GetAsync(artistId, "artist-rels", "url-rels", "releases", "release-groups");
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
            var releases = await Release.SearchAsync(query);

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

        private void AddHeaders(Request req)
        {
            req.AddHeader("Accept", "application/json");
        }
    }
}
