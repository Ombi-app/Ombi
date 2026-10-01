using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ombi.Core.Authentication;
using Ombi.Store.Entities;

namespace Ombi
{
    /// <summary>
    /// Records recent authenticated Ombi activity without changing LastLoggedIn.
    /// Activity writes are throttled so normal page/API traffic does not write to the
    /// user table on every request.
    /// </summary>
    public sealed class UserActivityMiddleware
    {
        private static readonly TimeSpan ActivityWriteInterval = TimeSpan.FromMinutes(1);
        private const string ActivityCachePrefix = "user-last-active:";
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActivityMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public UserActivityMiddleware(
            RequestDelegate next,
            ILogger<UserActivityMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context, IMemoryCache cache)
        {
            await RecordActivity(context, cache);
            await _next(context);
        }

        private async Task RecordActivity(HttpContext context, IMemoryCache cache)
        {
            if (!context.Request.Path.StartsWithSegments(new PathString("/api")) ||
                context.User?.Identity?.IsAuthenticated != true ||
                IsApiKeyRequest(context))
            {
                return;
            }

            var userName = context.User.Identity.Name;
            if (string.IsNullOrWhiteSpace(userName) ||
                string.Equals(userName, "API", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Use the same explicit Ombi user-id claim that JWT validation uses in
            // StartupExtensions. Do not prefer ClaimTypes.NameIdentifier here: inbound JWT
            // claim mapping can make that claim ambiguous when both `sub` and the explicit
            // NameIdentifier claim are present.
            var userId = context.User.Claims
                .FirstOrDefault(x => string.Equals(x.Type, "id", StringComparison.OrdinalIgnoreCase))
                ?.Value;
            var cacheIdentity = string.IsNullOrWhiteSpace(userId) ? userName.ToUpperInvariant() : userId;
            var cacheKey = ActivityCachePrefix + cacheIdentity;

            if (cache.TryGetValue(cacheKey, out _))
            {
                return;
            }

            try
            {
                // Identity's EF user store shares the request-scoped OmbiContext with request
                // repositories. If a LastActive update loses an optimistic-concurrency race,
                // Identity can leave the failed OmbiUser entity tracked as Modified. A later
                // SaveChanges for the user's real request can then retry that stale user update
                // and fail the unrelated request. Keep activity tracking in its own scope so any
                // failed Identity update and its change tracker are disposed before the request
                // pipeline continues.
                using var activityScope = _scopeFactory.CreateScope();
                var userManager = activityScope.ServiceProvider.GetRequiredService<OmbiUserManager>();

                OmbiUser user;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    user = await userManager.FindByIdAsync(userId);
                }
                else
                {
                    // This is only a defensive fallback for authenticated principals that do
                    // not expose Ombi's explicit Id claim.
                    user = await userManager.FindByNameAsync(userName);
                }

                if (user == null)
                {
                    _logger.LogWarning(
                        "Could not resolve authenticated Ombi user {UserName} for LastActive tracking (user id claim: {UserId})",
                        userName,
                        userId ?? "<missing>");
                    return;
                }

                if (user.IsSystemUser)
                {
                    return;
                }

                // Only throttle after a real Ombi user has been resolved. A bad/missing identity
                // lookup must not suppress subsequent activity attempts for a full minute. Recheck
                // here as well so parallel requests that passed the first cache check do not all write.
                if (cache.TryGetValue(cacheKey, out _))
                {
                    return;
                }

                cache.Set(cacheKey, true, ActivityWriteInterval);

                var now = DateTime.UtcNow;
                if (user.LastActive.HasValue && now - user.LastActive.Value < ActivityWriteInterval)
                {
                    _logger.LogDebug(
                        "Skipping LastActive update for Ombi user {UserName}; previous activity was {LastActive}",
                        user.UserName,
                        user.LastActive.Value);
                    return;
                }

                user.LastActive = now;
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Could not update LastActive for Ombi user {UserName}: {Errors}",
                        user.UserName,
                        string.Join("; ", result.Errors.Select(x => x.Description)));
                    cache.Remove(cacheKey);
                    return;
                }

                _logger.LogDebug(
                    "Updated LastActive for Ombi user {UserName} ({UserId}) to {LastActive}",
                    user.UserName,
                    user.Id,
                    user.LastActive);
            }
            catch (Exception ex)
            {
                // Activity tracking must never prevent the user's real request from running.
                cache.Remove(cacheKey);
                _logger.LogWarning(ex, "Could not update LastActive for Ombi user {UserName}", userName);
            }
        }

        private static bool IsApiKeyRequest(HttpContext context)
        {
            return context.Request.Headers.Keys.Any(x => string.Equals(x, "ApiKey", StringComparison.OrdinalIgnoreCase)) ||
                   context.Request.Query.ContainsKey("apikey");
        }
    }
}
