using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr.Models;

namespace Ombi.Api.Lidarr
{
    public class LidarrApi : ILidarrApi
    {
        public LidarrApi(ILogger<LidarrApi> logger, IApi api)
        {
            Api = api;
            Logger = logger;
        }

        private IApi Api { get; }
        private ILogger Logger { get; }

        private const string ApiVersion = "/api/v1";

        public Task<List<LidarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/qualityprofile", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return Api.Request<List<LidarrProfile>>(request);
        }

        public Task<List<LidarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/rootfolder", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return Api.Request<List<LidarrRootFolder>>(request);
        }

        public async Task<List<ArtistLookup>> ArtistLookup(string searchTerm, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/Artist/lookup", baseUrl, HttpMethod.Get);
            request.AddQueryString("term", searchTerm);

            AddHeaders(request, apiKey);
            return await Api.Request<List<ArtistLookup>>(request);
        }

        public Task<List<AlbumLookup>> AlbumLookup(string searchTerm, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/Album/lookup", baseUrl, HttpMethod.Get);
            request.AddQueryString("term", searchTerm);

            AddHeaders(request, apiKey);
            return Api.Request<List<AlbumLookup>>(request);
        }

        public Task<ArtistResult> GetArtist(int artistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist/{artistId}", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return Api.Request<ArtistResult>(request);
        }

        public async Task<ArtistResult> GetArtistByForeignId(string foreignArtistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist/lookup", baseUrl, HttpMethod.Get);

            request.AddQueryString("term", $"lidarr:{foreignArtistId}");
            AddHeaders(request, apiKey);
            return (await Api.Request<List<ArtistResult>>(request)).FirstOrDefault();
        }

        public async Task<AlbumLookup> GetAlbumByForeignId(string foreignArtistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album/lookup", baseUrl, HttpMethod.Get);

            request.AddQueryString("term", $"lidarr:{foreignArtistId}");
            AddHeaders(request, apiKey);
            var albums = await Api.Request<List<AlbumLookup>>(request);
            return albums.FirstOrDefault();
        }

        public Task<AlbumByArtistResponse> GetAlbumsByArtist(string foreignArtistId)
        {
            var request = new Request(string.Empty, $"https://api.lidarr.audio/api/v0.3/artist/{foreignArtistId}",
                HttpMethod.Get) {IgnoreBaseUrlAppend = true};
            return Api.Request<AlbumByArtistResponse>(request);
        }

        public Task<List<ArtistResult>> GetArtists(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return Api.Request<List<ArtistResult>>(request);
        }

        public Task<List<AlbumResponse>> GetAllAlbums(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return Api.Request<List<AlbumResponse>>(request);
        }

        public Task<ArtistResult> AddArtist(ArtistAdd artist, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/artist", baseUrl, HttpMethod.Post);
            request.AddJsonBody(artist);
            AddHeaders(request, apiKey);
            return Api.Request<ArtistResult>(request);
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
            return (await Api.Request<List<AlbumResponse>>(request)).FirstOrDefault();
        }

        public Task<List<AlbumResponse>> GetAllAlbumsByArtistId(int artistId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/album", baseUrl, HttpMethod.Get);
            request.AddQueryString("artistId", artistId.ToString());
            AddHeaders(request, apiKey);
            return Api.Request<List<AlbumResponse>>(request);
        }

        public Task<List<LanguageProfiles>> GetLanguageProfile(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/languageprofile", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);
            return Api.Request<List<LanguageProfiles>>(request);
        }

        public Task<List<MetadataProfile>> GetMetadataProfile(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/metadataprofile", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);
            return Api.Request<List<MetadataProfile>>(request);
        }

        public Task<LidarrStatus> Status(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/system/status", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);
            return Api.Request<LidarrStatus>(request);
        }

        public Task<CommandResult> AlbumSearch(int[] albumIds, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/command/", baseUrl, HttpMethod.Post);
            request.AddJsonBody(new { name = "AlbumSearch", albumIds });
            AddHeaders(request, apiKey);
            return Api.Request<CommandResult>(request);
        }

        private void AddHeaders(Request request, string key)
        {
            request.AddHeader("X-Api-Key", key);
        }
    }
}
