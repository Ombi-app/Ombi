using System;
using System.Collections.Generic;
using Ombi.Api.Models.Emby;

namespace Ombi.Api.Interfaces
{
    public interface IEmbyApi
    {
        string ApiKey { get; }

        EmbyItemContainer<EmbyMovieItem> GetAllMovies(string apiKey, string userId, Uri baseUri);
        EmbyItemContainer<EmbySeriesItem> GetAllShows(string apiKey, string userId, Uri baseUri);
        List<EmbyUser> GetUsers(Uri baseUri, string apiKey);
        EmbyItemContainer<EmbyLibrary> ViewLibrary(string apiKey, string userId, Uri baseUri);
    }
}