using System.Threading;
using System.Threading.Tasks;
using Ombi.Core.Models.Search.V2.Music;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMusicSearchEngineV2
    {
        Task<ArtistInformation> GetArtistInformation(string artistId);
        Task<AlbumArt> GetReleaseGroupArt(string musicBrainzId, CancellationToken token);
    }
}