using System;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;

namespace Ombi.Core.Authentication
{
    public interface IPlexOAuthManager
    {
        Task<string> GetAccessTokenFromPin(int pinId);
        Task<Uri> GetOAuthUrl(string code, string websiteAddress = null);
        Task<Uri> GetWizardOAuthUrl(string code, string websiteAddress);
        Task<PlexAccount> GetAccount(string accessToken);
    }
}