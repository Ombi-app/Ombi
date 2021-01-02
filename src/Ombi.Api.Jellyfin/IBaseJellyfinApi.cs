using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Jellyfin.Models;
using Ombi.Api.Jellyfin.Models.Media.Tv;
using Ombi.Api.Jellyfin.Models.Movie;

namespace Ombi.Api.Jellyfin
{
    public interface IBaseJellyfinApi
    {
        Task<JellyfinSystemInfo> GetSystemInformation(string apiKey, string baseUrl);
        Task<List<JellyfinUser>> GetUsers(string baseUri, string apiKey);
        Task<JellyfinUser> LogIn(string username, string password, string apiKey, string baseUri);

        Task<JellyfinItemContainer<JellyfinMovie>> GetAllMovies(string apiKey, int startIndex, int count, string userId,
            string baseUri);

        Task<JellyfinItemContainer<JellyfinEpisodes>> GetAllEpisodes(string apiKey, int startIndex, int count, string userId,
            string baseUri);

        Task<JellyfinItemContainer<JellyfinSeries>> GetAllShows(string apiKey, int startIndex, int count, string userId,
            string baseUri);

        Task<JellyfinItemContainer<JellyfinMovie>> GetCollection(string mediaId,
            string apiKey, string userId, string baseUrl);

        Task<SeriesInformation> GetSeriesInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<MovieInformation> GetMovieInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<EpisodeInformation> GetEpisodeInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<PublicInfo> GetPublicInformation(string baseUrl);
    }
}