using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Emby.Models;

namespace Ombi.Api.Emby
{
    public interface IEmbyApi
    {
        Task<EmbySystemInfo> GetSystemInformation(string apiKey, Uri baseUrl);
        Task<List<EmbyUser>> GetUsers(Uri baseUri, string apiKey);
        Task<EmbyUser> LogIn(string username, string password, string apiKey, Uri baseUri);
    }
}