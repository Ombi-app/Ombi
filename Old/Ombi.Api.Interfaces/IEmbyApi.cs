using System;
using System.Collections.Generic;
using Ombi.Api.Models.Emby;

namespace Ombi.Api.Interfaces
{
    public interface IEmbyApi
    {
        EmbyItemContainer<EmbyMovieItem> GetAllMovies(string apiKey, string userId, Uri baseUri);
        EmbyItemContainer<EmbySeriesItem> GetAllShows(string apiKey, string userId, Uri baseUri);
        EmbyItemContainer<EmbyEpisodeItem> GetAllEpisodes(string apiKey, string userId, Uri baseUri);
        EmbyItemContainer<EmbyMovieInformation> GetCollection(string mediaId, string apiKey, string userId, Uri baseUrl);
        List<EmbyUser> GetUsers(Uri baseUri, string apiKey);
        EmbyItemContainer<EmbyLibrary> ViewLibrary(string apiKey, string userId, Uri baseUri);
        EmbyInformation GetInformation(string mediaId, EmbyMediaType type, string apiKey, string userId, Uri baseUri);
        EmbyUser LogIn(string username, string password, string apiKey,  Uri baseUri);
        EmbySystemInfo GetSystemInformation(string apiKey, Uri baseUrl);
    }
}