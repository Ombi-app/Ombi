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

        public async Task<OAuthPin> RequestPin()
        {
            var pin = await _api.CreatePin();
            return pin;
        }

        public async Task<string> GetAccessTokenFromPin(int pinId)
        {
            var pin = await _api.GetPin(pinId);
            if (pin.expiresAt < DateTime.UtcNow)
            {
                return string.Empty;
            }

            if (pin.authToken.IsNullOrEmpty())
            {
                // Looks like we do not have a pin yet, we should retry a few times.
                var retryCount = 0;
                var retryMax = 5;
                var retryWaitMs = 1000;
                while (pin.authToken.IsNullOrEmpty() && retryCount < retryMax)
                {
                    retryCount++;
                    await Task.Delay(retryWaitMs);
                    pin = await _api.GetPin(pinId);
                }
            }
            return pin.authToken;
        }

        public async Task<PlexAccount> GetAccount(string accessToken)
        {
            return await _api.GetAccount(accessToken);
        }

        public async Task<Uri> GetOAuthUrl(int pinId, string code, string websiteAddress = null)
        {
            var settings = await _customizationSettingsService.GetSettingsAsync();
            var url = _api.GetOAuthUrl(pinId, code, settings.ApplicationUrl.IsNullOrEmpty() ? websiteAddress : settings.ApplicationUrl, false);

            return url;
        }

        public Uri GetWizardOAuthUrl(int pinId, string code, string websiteAddress)
        {
            var url = _api.GetOAuthUrl(pinId, code, websiteAddress, true);
            return url;
        }
    }
}