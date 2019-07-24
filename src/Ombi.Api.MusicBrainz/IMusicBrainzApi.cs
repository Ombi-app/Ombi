using System.Collections.Generic;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API.Entities;

namespace Ombi.Api.MusicBrainz
{
    public interface IMusicBrainzApi
    {
        Task<IEnumerable<Artist>> SearchArtist(string artistQuery);
        Task<IEnumerable<Release>> GetReleaseForArtist(string artistId);
        Task<Artist> GetArtistInformation(string artistId);
    }
}