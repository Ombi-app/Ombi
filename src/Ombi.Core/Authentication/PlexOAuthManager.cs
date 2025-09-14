using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Api.External.MediaServers.Plex.Models.OAuth;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Authentication
{
    public class PlexOAuthManager : IPlexOAuthManager
    {
        public PlexOAuthManager(IPlexApi api, ISettingsService<CustomizationSettings> settings, ISettingsService<PlexSettings> plexSettings, ILogger<PlexOAuthManager> logger)
        {
            _api = api;
            _customizationSettingsService = settings;
            _plexSettingsService = plexSettings;
            _logger = logger;
        }

        private readonly IPlexApi _api;
        private readonly ISettingsService<CustomizationSettings> _customizationSettingsService;
        private readonly ISettingsService<PlexSettings> _plexSettingsService;
        private readonly ILogger _logger;

        public async Task<string> GetAccessTokenFromPin(int pinId)
        {
            var pin = await _api.GetPin(pinId);
            if (pin.Errors != null)
            {
                foreach (var err in pin.Errors?.errors ?? new List<OAuthErrors>())
                { 
                    _logger.LogError($"Code: '{err.code}' : '{err.message}'");
                }

                return string.Empty;
            }

            if (pin.Result.expiresIn <= 0)
            {
                _logger.LogError("Pin has expired");
                return string.Empty;
            }

            // Sanity log: compare the PIN clientIdentifier with our current InstallId used for X-Plex-Client-Identifier
            try
            {
                var plexSettings = await _plexSettingsService.GetSettingsAsync();
                var installId = plexSettings?.InstallId.ToString("N");
                var pinClientId = pin.Result.clientIdentifier;

                if (string.IsNullOrWhiteSpace(installId))
                {
                    _logger.LogWarning("Plex OAuth sanity check: InstallId is empty. The UI must call /api/v1/settings/clientid before starting OAuth.");
                }
                else if (!string.Equals(installId, pinClientId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"Plex OAuth sanity check: Mismatch between server InstallId '{(installId?.Length >= 6 ? installId.Substring(0, 6) : installId)}' and PIN.clientIdentifier '{(pinClientId?.Length >= 6 ? pinClientId.Substring(0, 6) : pinClientId)}'. This can cause Plex PIN polling failures (code 1020).");
                }
                else
                {
                    _logger.LogDebug($"Plex OAuth sanity check: Client identifier matches ({installId}).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Plex OAuth sanity check logging failed");
            }

            return pin.Result.authToken;
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