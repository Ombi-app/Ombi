using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;
using Ombi.Api.Emby.Models.Movie;

namespace Ombi.Api.Emby
{
    public interface IBaseEmbyApi
    {
        Task<EmbySystemInfo> GetSystemInformation(string apiKey, string baseUrl);
        Task<List<EmbyUser>> GetUsers(string baseUri, string apiKey);
        Task<EmbyUser> LogIn(string username, string password, string apiKey, string baseUri, string clientIpAddress);

        Task<EmbyItemContainer<EmbyMovie>> GetAllMovies(string apiKey, string parentIdFilder, int startIndex, int count, string userId,
            string baseUri);

        Task<EmbyItemContainer<EmbyEpisodes>> GetAllEpisodes(string apiKey, string parentIdFilder, int startIndex, int count, string userId,
            string baseUri);

        Task<EmbyItemContainer<EmbySeries>> GetAllShows(string apiKey, string parentIdFilder, int startIndex, int count, string userId,
            string baseUri);

        Task<EmbyItemContainer<EmbyMovie>> GetCollection(string mediaId,
            string apiKey, string userId, string baseUrl);

        Task<SeriesInformation> GetSeriesInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<MovieInformation> GetMovieInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<EpisodeInformation> GetEpisodeInformation(string mediaId, string apiKey, string userId, string baseUrl);
        Task<PublicInfo> GetPublicInformation(string baseUrl);
        Task<EmbyItemContainer<EmbyMovie>> RecentlyAddedMovies(string apiKey, string parentIdFilder, int startIndex, int count, string userId, string baseUri);
        Task<EmbyItemContainer<EmbyEpisodes>> RecentlyAddedEpisodes(string apiKey, string parentIdFilder, int startIndex, int count, string userId, string baseUri);
        Task<EmbyItemContainer<EmbySeries>> RecentlyAddedShows(string apiKey, string parentIdFilder, int startIndex, int count, string userId, string baseUri);
        
        Task<EmbyItemContainer<EmbyMovie>> GetMoviesPlayed(string apiKey, string parentIdFilder, int startIndex, int count, string userId, string baseUri);
        Task<EmbyItemContainer<EmbyEpisodes>> GetTvPlayed(string apiKey, string parentIdFilder, int startIndex, int count, string userId, string baseUri);
    }
}