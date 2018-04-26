using System;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.OAuth;

namespace Ombi.Core.Authentication
{
    public interface IPlexOAuthManager
    {
        Task<string> GetAccessTokenFromPin(int pinId);
        Task<OAuthPin> RequestPin();
        Task<Uri> GetOAuthUrl(int pinId, string code, string websiteAddress = null);
        Uri GetWizardOAuthUrl(int pinId, string code, string websiteAddress);
        Task<PlexAccount> GetAccount(string accessToken);
    }
}