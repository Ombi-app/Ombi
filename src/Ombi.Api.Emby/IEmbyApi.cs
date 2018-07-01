using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;
using Ombi.Api.Emby.Models.Movie;

namespace Ombi.Api.Emby
{
    public interface IEmbyApi
    {
        Task<EmbySystemInfo> GetSystemInformation(string apiKey, string baseUrl);
        Task<List<EmbyUser>> GetUsers(string baseUri, string apiKey);
        Task<EmbyUser> LogIn(string username, string password, string apiKey, string baseUri);
        Task<EmbyConnectUser> LoginConnectUser(string username, string password);

        Task<EmbyItemContainer<EmbyMovie>> GetAllMovies(string apiKey, int startIndex, int count, string userId,
            string baseUri);

        Task<EmbyItemContainer<EmbyEpisodes>> GetAllEpisodes(string apiKey, int startIndex, int count, string userId,
            string baseUri);

        Task<EmbyItemContainer<EmbySeries>> GetAllShows(string apiKey, int startIndex, int count, string userId,
            string baseUri);

        Task<EmbyItemContainer<EmbyMovie>> GetCollection(string mediaId,
            string apiKey, string userId, string baseUrl);

        Task<SeriesInformation> GetSeriesInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<MovieInformation> GetMovieInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<EpisodeInformation> GetEpisodeInformation(string mediaId, string apiKey, string userId, string baseUrl);
    }
}