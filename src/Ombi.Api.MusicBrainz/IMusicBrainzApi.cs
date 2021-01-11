using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API.Entities;
using Hqub.MusicBrainz.API.Entities.Collections;
using Ombi.Api.MusicBrainz.Models;

namespace Ombi.Api.MusicBrainz
{
    public interface IMusicBrainzApi
    {
        Task<IEnumerable<Artist>> SearchArtist(string artistQuery);
        Task<IEnumerable<Release>> GetReleaseForArtist(string artistId);
        Task<Artist> GetArtistInformation(string artistId);
        Task<Release> GetAlbumInformation(string albumId);
        Task<ReleaseGroupArt> GetCoverArtForReleaseGroup(string musicBrainzId, CancellationToken token);
    }
}