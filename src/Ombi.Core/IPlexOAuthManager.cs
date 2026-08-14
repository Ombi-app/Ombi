using System;
using System.Threading.Tasks;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Api.External.MediaServers.Plex.Models.OAuth;

namespace Ombi.Core.Authentication
{
    public interface IPlexOAuthManager
    {
        Task<OAuthContainer> CreatePin();
        Task<string> GetAccessTokenFromPin(int pinId);
        Task<Uri> GetOAuthUrl(string code, string websiteAddress = null);
        Task<Uri> GetWizardOAuthUrl(string code, string websiteAddress);
        Task<PlexAccount> GetAccount(string accessToken);
    }
}