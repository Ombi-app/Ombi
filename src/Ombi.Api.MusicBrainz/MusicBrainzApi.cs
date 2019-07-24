using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;

namespace Ombi.Api.MusicBrainz
{
    public class MusicBrainzApi : IMusicBrainzApi
    {
        public async Task<IEnumerable<Artist>> SearchArtist(string artistQuery)
        {
            var artist = await Hqub.MusicBrainz.API.Entities.Artist.SearchAsync(artistQuery, 10);
            return artist.Items.Where(x => !x.Type.Equals("Person", StringComparison.CurrentCultureIgnoreCase));
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

        private void AddHeaders(Request req)
        {
            req.AddHeader("Accept", "application/json");
        }
    }
}
