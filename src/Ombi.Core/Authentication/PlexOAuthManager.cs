using System;
using System.Threading.Tasks;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.OAuth;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Authentication
{
    public class PlexOAuthManager : IPlexOAuthManager
    {
        public PlexOAuthManager(IPlexApi api, ISettingsService<CustomizationSettings> settings)
        {
            _api = api;
            _customizationSettingsService = settings;
        }

        private readonly IPlexApi _api;
        private readonly ISettingsService<CustomizationSettings> _customizationSettingsService;

        public async Task<string> GetAccessTokenFromPin(int pinId)
        {
            var pin = await _api.GetPin(pinId);
            if (pin.expiresAt < DateTime.UtcNow)
            {
                return string.Empty;
            }

            return pin.authToken;
        }

        public async Task<PlexAccount> GetAccount(string accessToken)
        {
            return await _api.GetAccount(accessToken);
        }

        public async Task<Uri> GetOAuthUrl(string code, string websiteAddress = null)
        {
            var settings = await _customizationSettingsService.GetSettingsAsync();
            var url = await _api.GetOAuthUrl(code, settings.ApplicationUrl.IsNullOrEmpty() ? websiteAddress : settings.ApplicationUrl);

            return url;
        }

        public async Task<Uri> GetWizardOAuthUrl(string code, string websiteAddress)
        {
            var url = await _api.GetOAuthUrl(code, websiteAddress);
            return url;
        }
    }
}