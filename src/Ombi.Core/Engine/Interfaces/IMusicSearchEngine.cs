using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Lidarr.Models;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Engine
{
    public interface IMusicSearchEngine
    {
        Task<ArtistResult> GetAlbumArtist(string foreignArtistId);
        Task<ArtistResult> GetArtist(int artistId);
        Task<ArtistResult> GetArtistAlbums(string foreignArtistId);
        Task<IEnumerable<AlbumLookup>> SearchAlbum(string search);
        Task<IEnumerable<SearchArtistViewModel>> SearchArtist(string search);
    }
}