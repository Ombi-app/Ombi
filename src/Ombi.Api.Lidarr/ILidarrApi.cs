using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.Lidarr.Models;

namespace Ombi.Api.Lidarr
{
    public interface ILidarrApi
    {
        Task<List<AlbumLookup>> AlbumLookup(string searchTerm, string apiKey, string baseUrl);
        Task<List<ArtistLookup>> ArtistLookup(string searchTerm, string apiKey, string baseUrl);
        Task<List<LidarrProfile>> GetProfiles(string apiKey, string baseUrl);
        Task<List<LidarrRootFolder>> GetRootFolders(string apiKey, string baseUrl);
        Task<ArtistResult> GetArtist(int artistId, string apiKey, string baseUrl);
        Task<ArtistResult> GetArtistByForeignId(string foreignArtistId, string apiKey, string baseUrl, CancellationToken token = default);
        Task<AlbumByArtistResponse> GetAlbumsByArtist(string foreignArtistId);
        Task<AlbumLookup> GetAlbumByForeignId(string foreignArtistId, string apiKey, string baseUrl);
        Task<List<ArtistResult>> GetArtists(string apiKey, string baseUrl);
        Task<List<AlbumResponse>> GetAllAlbums(string apiKey, string baseUrl);
        Task<ArtistResult> AddArtist(ArtistAdd artist, string apiKey, string baseUrl);
        Task<AlbumResponse> MontiorAlbum(int albumId, string apiKey, string baseUrl);
        Task<List<AlbumResponse>> GetAllAlbumsByArtistId(int artistId, string apiKey, string baseUrl);
        Task<List<MetadataProfile>> GetMetadataProfile(string apiKey, string baseUrl);
        Task<LidarrStatus> Status(string apiKey, string baseUrl);
        Task<CommandResult> AlbumSearch(int[] albumIds, string apiKey, string baseUrl);
        Task<AlbumByForeignId> AlbumInformation(string albumId, string apiKey, string baseUrl);
    }
}