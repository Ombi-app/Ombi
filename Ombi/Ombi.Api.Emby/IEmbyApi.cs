using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Emby.Models;

namespace Ombi.Api.Emby
{
    public interface IEmbyApi
    {
        Task<EmbySystemInfo> GetSystemInformation(string apiKey, string baseUrl);
        Task<List<EmbyUser>> GetUsers(string baseUri, string apiKey);
        Task<EmbyUser> LogIn(string username, string password, string apiKey, string baseUri);
    }
}