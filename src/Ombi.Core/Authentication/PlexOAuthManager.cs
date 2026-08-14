using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
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
        public PlexOAuthManager(IPlexApi api, ISettingsService<CustomizationSettings> settings, ISettingsService<PlexSettings> plexSettings, ILogger<PlexOAuthManager> logger, IMemoryCache memoryCache)
        {
            _api = api;
            _customizationSettingsService = settings;
            _plexSettingsService = plexSettings;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        private readonly IPlexApi _api;
        private readonly ISettingsService<CustomizationSettings> _customizationSettingsService;
        private readonly ISettingsService<PlexSettings> _plexSettingsService;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private const string PinCachePrefix = "PlexOAuthPinCode:";

        public async Task<OAuthContainer> CreatePin()
        {
            var pin = await _api.CreatePin();
            if (pin?.Result != null && !string.IsNullOrWhiteSpace(pin.Result.code))
            {
                // The PIN code is authentication material. Keep it server-side so polling only needs
                // the numeric PIN id and the code never has to be placed in Ombi request URLs.
                var lifetimeSeconds = pin.Result.expiresIn > 0 ? Math.Min(pin.Result.expiresIn, 1800) : 1800;
                _memoryCache.Set(GetPinCacheKey(pin.Result.id), pin.Result.code, TimeSpan.FromSeconds(lifetimeSeconds));
            }

            return pin;
        }

        public async Task<string> GetAccessTokenFromPin(int pinId)
        {
            if (!_memoryCache.TryGetValue(GetPinCacheKey(pinId), out string pinCode) || string.IsNullOrWhiteSpace(pinCode))
            {
                _logger.LogWarning("Plex OAuth PIN {PinId} was not created by this Ombi instance or its cached PIN code has expired.", pinId);
                return string.Empty;
            }

            var pin = await _api.GetPin(pinId, pinCode);
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
                    _logger.LogWarning("Plex OAuth sanity check: InstallId is empty; Plex PIN redemption cannot use a stable client identifier.");
                }
                else if (!string.Equals(installId, pinClientId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"Plex OAuth sanity check: Mismatch between server InstallId '{(installId?.Length >= 6 ? installId.Substring(0, 6) : installId)}' and PIN.clientIdentifier '{(pinClientId?.Length >= 6 ? pinClientId.Substring(0, 6) : pinClientId)}'. This can cause Plex PIN polling failures (code 1020).");
                }
                else
                {
                    _logger.LogDebug("Plex OAuth sanity check: Client identifier matches.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Plex OAuth sanity check logging failed");
            }

            if (!string.IsNullOrWhiteSpace(pin.Result.authToken))
            {
                _memoryCache.Remove(GetPinCacheKey(pinId));
            }

            return pin.Result.authToken;
        }

        private static string GetPinCacheKey(int pinId) => $"{PinCachePrefix}{pinId}";

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