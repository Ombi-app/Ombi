using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Store.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ombi.Core.Authentication
{
    public interface IPlexTokenKeepAliveService
    {
        Task<bool> KeepTokenAliveAsync(string token, CancellationToken cancellationToken);
        Task<bool> TryRefreshTokenAsync(OmbiUser user, CancellationToken cancellationToken);
    }

    public class PlexTokenKeepAliveService : IPlexTokenKeepAliveService
    {
        private readonly IPlexApi _plexApi;
        private readonly ILogger<PlexTokenKeepAliveService> _logger;
        private readonly UserManager<OmbiUser> _userManager;

        public PlexTokenKeepAliveService(IPlexApi plexApi, ILogger<PlexTokenKeepAliveService> logger, UserManager<OmbiUser> userManager)
        {
            _plexApi = plexApi;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<bool> KeepTokenAliveAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Plex token is null or empty - cannot validate");
                    return false;
                }

                _logger.LogDebug("Validating Plex token using watchlist endpoint");
                
                // Use the Ping method to validate the token
                var isValid = await _plexApi.Ping(token, cancellationToken);
                
                if (!isValid)
                {
                    _logger.LogWarning("Plex token validation failed - token may be expired, invalid, or lacks watchlist permissions. User will need to re-authenticate.");
                }
                else
                {
                    _logger.LogDebug("Plex token validation successful");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while validating Plex token - treating as invalid");
                return false;
            }
        }

        public async Task<bool> TryRefreshTokenAsync(OmbiUser user, CancellationToken cancellationToken)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("Cannot refresh token - user is null");
                    return false;
                }

                _logger.LogInformation($"Attempting to refresh Plex token for user '{user.UserName}'");

                // Try to get account info with current token to see if we can get a fresh token
                var account = await _plexApi.GetAccount(user.MediaServerToken);
                
                if (account?.user?.authentication_token != null && 
                    account.user.authentication_token != user.MediaServerToken)
                {
                    _logger.LogInformation($"Successfully refreshed Plex token for user '{user.UserName}'");
                    user.MediaServerToken = account.user.authentication_token;
                    await _userManager.UpdateAsync(user);
                    return true;
                }

                _logger.LogWarning($"Could not refresh Plex token for user '{user.UserName}' - account info returned same or null token");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while trying to refresh Plex token for user '{user.UserName}'");
                return false;
            }
        }
    }
} 