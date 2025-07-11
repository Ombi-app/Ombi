using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;

namespace Ombi.Core.Authentication
{
    public interface IPlexTokenKeepAliveService
    {
        Task<bool> KeepTokenAliveAsync(string token, CancellationToken cancellationToken);
    }

    public class PlexTokenKeepAliveService : IPlexTokenKeepAliveService
    {
        private readonly IPlexApi _plexApi;
        private readonly ILogger<PlexTokenKeepAliveService> _logger;

        public PlexTokenKeepAliveService(IPlexApi plexApi, ILogger<PlexTokenKeepAliveService> logger)
        {
            _plexApi = plexApi;
            _logger = logger;
        }

        public async Task<bool> KeepTokenAliveAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token is null or empty");
                    return false;
                }

                // Use the Ping method to validate the token
                var isValid = await _plexApi.Ping(token, cancellationToken);
                
                if (!isValid)
                {
                    _logger.LogWarning("Token validation failed - token may be expired or invalid");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while keeping token alive");
                return false;
            }
        }
    }
} 