using System;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;

namespace Ombi.Core.Authentication
{
    public interface IPlexOAuthManager
    {
        Task<string> GetAccessTokenFromPin(int pinId);
        Task<Uri> GetOAuthUrl(int pinId, string code, string websiteAddress = null);
        Task<Uri> GetWizardOAuthUrl(int pinId, string code, string websiteAddress);
        Task<PlexAccount> GetAccount(string accessToken);
    }
}