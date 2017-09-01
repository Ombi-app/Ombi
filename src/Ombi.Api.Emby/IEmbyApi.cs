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

        Task<EmbyItemContainer<EmbyMovie>> GetAllMovies(string apiKey, string userId, string baseUri);
        Task<EmbyItemContainer<EmbyEpisodes>> GetAllEpisodes(string apiKey, string userId, string baseUri);
        Task<EmbyItemContainer<EmbySeries>> GetAllShows(string apiKey, string userId, string baseUri);

        Task<EmbyItemContainer<MovieInformation>> GetCollection(string mediaId, string apiKey, string userId,
            string baseUrl);

        Task<SeriesInformation> GetSeriesInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<MovieInformation> GetMovieInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<EpisodeInformation> GetEpisodeInformation(string mediaId, string apiKey, string userId, string baseUrl);
    }
}