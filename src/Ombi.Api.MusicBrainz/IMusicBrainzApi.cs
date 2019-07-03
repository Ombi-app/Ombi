using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.MusicBrainz.Models.Lookup;
using Ombi.Api.MusicBrainz.Models.Search;

namespace Ombi.Api.MusicBrainz
{
    public interface IMusicBrainzApi
    {
        Task<IEnumerable<Artist>> SearchArtist(string artistQuery);
        Task<IEnumerable<ReleaseGroups>> GetReleaseGroups(string artistId);
    }
}