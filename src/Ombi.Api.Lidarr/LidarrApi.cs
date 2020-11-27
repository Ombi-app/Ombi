using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr.Models;

namespace Ombi.Api.Lidarr
{
    public class LidarrApi : ILidarrApi
    {
        public LidarrApi(IApi api)
        {
            _api = api;
        }

        private IApi _api { get; }

        private const string ApiVersion = "/api/v1";

        public Task<List<LidarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/qualityprofile", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return _api.Request<List<LidarrProfile>>(request);
        }

        public Task<List<LidarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/rootfolder", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return _api.Request<List<LidarrRootFolder>>(request);
        }

        public async Task<List<ArtistLookup>> ArtistLookup(string searchTerm, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/Artist/lookup", baseUrl, HttpMethod.Get);
            request.AddQueryString("term", searchTerm);

            AddHeaders(request, apiKey);
            return await _api.Request<List<ArtistLookup>>(request);
        }

        public Task<List<AlbumLookup>> AlbumLookup(string searchTerm, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/Album/lookup", baseUrl, HttpMethod.Get);
            request.AddQueryString("term", searchTerm);

            AddHeaders(request, apiKey);
            return _api.Request<List<AlbumLookup>>(request);
        }

        public Task<ArtistResult> GetArtist(int artistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist/{artistId}", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return _api.Request<ArtistResult>(request);
        }

        public async Task<ArtistResult> GetArtistByForeignId(string foreignArtistId, string apiKey, string baseUrl, CancellationToken token = default)
        {
            var request = new Request($"{ApiVersion}/artist/lookup", baseUrl, HttpMethod.Get);

            request.AddQueryString("term", $"lidarr:{foreignArtistId}");
            AddHeaders(request, apiKey);
            return (await _api.Request<List<ArtistResult>>(request, token)).FirstOrDefault();
        }

        public async Task<AlbumLookup> GetAlbumByForeignId(string foreignArtistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album/lookup", baseUrl, HttpMethod.Get);

            request.AddQueryString("term", $"lidarr:{foreignArtistId}");
            AddHeaders(request, apiKey);
            var albums = await _api.Request<List<AlbumLookup>>(request);
            return albums.FirstOrDefault();
        }

        public Task<AlbumByArtistResponse> GetAlbumsByArtist(string foreignArtistId)
        {
            var request = new Request(string.Empty, $"https://api.lidarr.audio/api/v0.4/artist/{foreignArtistId}",
                HttpMethod.Get) {IgnoreBaseUrlAppend = true};
            return _api.Request<AlbumByArtistResponse>(request);
        }

        public Task<List<ArtistResult>> GetArtists(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return _api.Request<List<ArtistResult>>(request);
        }

        public Task<List<AlbumResponse>> GetAllAlbums(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return _api.Request<List<AlbumResponse>>(request);
        }

        public async Task<AlbumByForeignId> AlbumInformation(string albumId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album", baseUrl, HttpMethod.Get);
            request.AddQueryString("foreignAlbumId", albumId);
            AddHeaders(request, apiKey);
            var albums = await _api.Request<List<AlbumByForeignId>>(request);
            return albums.FirstOrDefault();
        }


        /// <summary>
        /// THIS ONLY SUPPORTS ALBUMS THAT THE ARTIST IS IN LIDARR
        /// </summary>
        /// <param name="albumId"></param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public Task<List<LidarrTrack>> GetTracksForAlbum(int albumId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album", baseUrl, HttpMethod.Get);
            request.AddQueryString("albumId", albumId.ToString());
            AddHeaders(request, apiKey);
            return _api.Request<List<LidarrTrack>>(request);
        }

        public Task<ArtistResult> AddArtist(ArtistAdd artist, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist", baseUrl, HttpMethod.Post);
            request.AddJsonBody(artist);
            AddHeaders(request, apiKey);
            return _api.Request<ArtistResult>(request);
        }

        public async Task<AlbumResponse> MontiorAlbum(int albumId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album/monitor", baseUrl, HttpMethod.Put);
            request.AddJsonBody(new
            {
                albumIds = new[] { albumId },
                monitored = true
            });
            AddHeaders(request, apiKey);
            return (await _api.Request<List<AlbumResponse>>(request)).FirstOrDefault();
        }

        public Task<List<AlbumResponse>> GetAllAlbumsByArtistId(int artistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album", baseUrl, HttpMethod.Get);
            request.AddQueryString("artistId", artistId.ToString());
            AddHeaders(request, apiKey);
            return _api.Request<List<AlbumResponse>>(request);
        }

        public Task<List<MetadataProfile>> GetMetadataProfile(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/metadataprofile", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);
            return _api.Request<List<MetadataProfile>>(request);
        }

        public Task<LidarrStatus> Status(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/system/status", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);
            return _api.Request<LidarrStatus>(request);
        }

        public Task<CommandResult> AlbumSearch(int[] albumIds, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/command/", baseUrl, HttpMethod.Post);
            request.AddJsonBody(new { name = "AlbumSearch", albumIds });
            AddHeaders(request, apiKey);
            return _api.Request<CommandResult>(request);
        }

        private void AddHeaders(Request request, string key)
        {
            request.AddHeader("X-Api-Key", key);
        }
    }
}
