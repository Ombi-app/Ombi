using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Models.External;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Ombi.Controllers.V1
{


    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class TokenController : BaseController
    {
        public TokenController(OmbiUserManager um, ITokenRepository token,
            IPlexOAuthManager oAuthManager, ILogger<TokenController> logger, ISettingsService<AuthenticationSettings> auth,
            ISettingsService<UserManagementSettings> userManagement, ISettingsService<PlexSettings> plexSettings)
        {
            _userManager = um;
            _token = token;
            _plexOAuthManager = oAuthManager;
            _log = logger;
            _authSettings = auth;
            _userManagementSettings = userManagement;
            _plexSettings = plexSettings;
        }

        private readonly ITokenRepository _token;
        private readonly OmbiUserManager _userManager;
        private readonly IPlexOAuthManager _plexOAuthManager;
        private readonly ILogger<TokenController> _log;
        private readonly ISettingsService<AuthenticationSettings> _authSettings;
        private readonly ISettingsService<UserManagementSettings> _userManagementSettings;
        private readonly ISettingsService<PlexSettings> _plexSettings;

        private const string PlexAccountUnauthorizedMessage = "This Plex account is not authorized to access Ombi";

        /// <summary>
        /// Creates a strong Plex OAuth PIN from the Ombi backend.
        /// Keeping PIN creation server-side avoids Plex cross-origin PIN creation issues.
        /// </summary>
        [HttpPost("plexpin")]
        [EnableRateLimiting("PlexPinCreation")]
        public async Task<IActionResult> CreatePlexPin()
        {
            var pin = await _plexOAuthManager.CreatePin();
            if (pin?.Result != null && !string.IsNullOrWhiteSpace(pin.Result.pollToken))
            {
                // Do not expose Plex's numeric PIN id or PIN code to the browser. The opaque
                // poll token is the only handle the client needs for URL creation and polling.
                return Ok(new
                {
                    pollToken = pin.Result.pollToken,
                    expiresIn = pin.Result.expiresIn,
                    expiresAt = pin.Result.expiresAt
                });
            }

            if (pin?.Errors?.errors != null)
            {
                foreach (var err in pin.Errors.errors)
                {
                    _log.LogError(
                        "Plex PIN creation failed. Code: '{Code}' : '{Message}'",
                        err.code,
                        err.message);
                }
            }

            return StatusCode(502, new { errorMessage = "Could not create Plex authentication PIN" });
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [EnableRateLimiting("TokenLogin")]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(Token), 200)]
        public async Task<IActionResult> GetToken([FromBody] UserAuthModel model)
        {
            if (!model.UsePlexOAuth)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user == null)
                {
                    // Could this be an email login?
                    user = await _userManager.FindByEmailAsync(model.Username);

                    if (user == null)
                    {
                        _log.LogWarning(string.Format("Failed login attempt by IP: {0}", GetRequestIP()));
                        return new UnauthorizedResult();
                    }

                    user.EmailLogin = true;
                }

                _userManager.ClientIpAddress = GetRequestIP();
                // Verify Password
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return await CreateToken(model.RememberMe, user);
                }
            }
            else
            {
                // Plex OAuth
                // Redirect them to Plex

                var pollToken = model.PlexTvPin?.pollToken;
                if (string.IsNullOrWhiteSpace(pollToken))
                {
                    return BadRequest(new { error = "Plex OAuth session is missing or invalid" });
                }

                var websiteAddress = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                var url = await _plexOAuthManager.GetOAuthUrl(pollToken, websiteAddress);
                if (url == null)
                {
                    return new JsonResult(new
                    {
                        error = "Application URL has not been set or the Plex OAuth session has expired"
                    });
                }
                return new JsonResult(new { url = url.ToString(), pollToken });
            }

            _log.LogWarning(string.Format("Failed login attempt by IP: {0}", GetRequestIP()));
            return new UnauthorizedResult();
        }

        /// <summary>
        /// Returns the Token for the Ombi User if we can match the Plex user with a valid Ombi User
        /// </summary>
        [HttpPost("plextoken")]
        [EnableRateLimiting("PlexTokenLogin")]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetTokenWithPlexToken([FromBody] PlexTokenAuthentication model)
        {
            if (!model.PlexToken.HasValue())
            {
                return BadRequest("Token was not provided");
            }
            var user = await _userManager.GetOmbiUserFromPlexToken(model.PlexToken);
            if (user == null)
            {
                return Unauthorized();
            }
            
            return await CreateToken(true, user);
        }


        private async Task<IActionResult> CreateToken(bool rememberMe, OmbiUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(OmbiRoles.Disabled))
            {
                return new UnauthorizedResult();
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", user.Id)
            };
            claims.AddRange(roles.Select(role => new Claim("role", role)));
            if (user.Email.HasValue())
            {
                claims.Add(new Claim("Email", user.Email));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StartupSingleton.Instance.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: rememberMe ? DateTime.Now.AddYears(1) : DateTime.Now.AddDays(7),
                signingCredentials: creds,
                audience: "Ombi", issuer: "Ombi"
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            if (rememberMe)
            {
                // Save the token so we can refresh it later
                //await _token.CreateToken(new Tokens() {Token = accessToken, User = user});
            }

            var loginTime = DateTime.UtcNow;
            user.LastLoggedIn = loginTime;
            user.LastActive = loginTime;

            await _userManager.UpdateAsync(user);

            return Ok(new Token
            {
                AccessToken = accessToken,
                Expiration = token.ValidTo
            });
        }

        [HttpPost("plexoauth")]
        [EnableRateLimiting("PlexPinPolling")]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> OAuth([FromBody] PlexOAuthPollRequest request)
        {
            // Poll-token format and server-side session validation belong to the OAuth manager.
            // Always delegate here so user-controlled input does not gate the sensitive redemption call.
            var accessToken = await _plexOAuthManager.GetAccessTokenFromPollToken(request?.PollToken);
            if (accessToken.IsNullOrEmpty())
            {
                return PlexOAuthError("Could not authenticate with Plex");
            }

            var account = await _plexOAuthManager.GetAccount(accessToken);
            var accountError = ValidatePlexAccount(account);
            if (accountError != null)
            {
                return accountError;
            }

            var user = await FindUserByProviderId(account.user.id);
            if (user == null)
            {
                var resolution = await ResolveUnlinkedPlexUser(account);
                if (resolution.Error != null)
                {
                    return resolution.Error;
                }

                user = resolution.User;
            }

            if (user == null)
            {
                var plexUserName = GetPlexUserName(account);
                _log.LogWarning(
                    "Plex OAuth account {PlexUserId} ({PlexUserName}) could not be matched to an authorized Plex user or linked as the configured Plex server owner.",
                    account.user.id, plexUserName);
                return PlexOAuthError(PlexAccountUnauthorizedMessage);
            }

            user.MediaServerToken = account.user.authentication_token;
            await _userManager.UpdateAsync(user);

            return await CreateToken(true, user);
        }

        private JsonResult ValidatePlexAccount(PlexAccount account)
        {
            if (account?.user == null)
            {
                return PlexOAuthError("Plex account details are invalid or missing");
            }

            if (string.IsNullOrEmpty(account.user.authentication_token))
            {
                return PlexOAuthError("Plex authentication token is missing");
            }

            return null;
        }

        private async Task<OmbiUser> FindUserByProviderId(string providerUserId)
        {
            if (string.IsNullOrEmpty(providerUserId))
            {
                return null;
            }

            return await _userManager.Users.FirstOrDefaultAsync(x =>
                x.ProviderUserId == providerUserId &&
                (x.UserType == UserType.PlexUser || x.UserType == UserType.LocalUser));
        }

        private async Task<PlexUserResolution> ResolveUnlinkedPlexUser(PlexAccount account)
        {
            var plexUserName = GetPlexUserName(account);
            var candidates = await GetIdentityCandidates(account, plexUserName);
            if (candidates.Error != null)
            {
                return PlexUserResolution.FromError(candidates.Error);
            }

            var candidateResolution = await ResolveIdentityCandidate(account, candidates);
            if (candidateResolution.Error != null || candidateResolution.User != null)
            {
                return candidateResolution;
            }

            if (!await IsConfiguredPlexServerOwner(account))
            {
                return PlexUserResolution.Empty();
            }

            return await LinkOrCreatePlexAdmin(account, plexUserName, candidateResolution.LocalLinkCandidate);
        }

        private async Task<PlexIdentityCandidates> GetIdentityCandidates(PlexAccount account, string plexUserName)
        {
            OmbiUser usernameMatch = null;
            OmbiUser emailMatch = null;

            if (!string.IsNullOrEmpty(plexUserName))
            {
                usernameMatch = await _userManager.FindByNameAsync(plexUserName);
            }

            if (!string.IsNullOrEmpty(account.user.email))
            {
                emailMatch = await _userManager.FindByEmailAsync(account.user.email);
            }

            if (usernameMatch != null && emailMatch != null &&
                !string.Equals(usernameMatch.Id, emailMatch.Id, StringComparison.Ordinal))
            {
                _log.LogWarning("Plex OAuth username and email resolve to different Ombi accounts; refusing authentication.");
                return PlexIdentityCandidates.FromError(PlexOAuthError(PlexAccountUnauthorizedMessage));
            }

            var identityCandidate = emailMatch ?? usernameMatch;
            if (HasConflictingProviderId(identityCandidate, account.user.id))
            {
                _log.LogWarning("An Ombi account matched by Plex username/email is already linked to a different Plex identity; refusing authentication.");
                return PlexIdentityCandidates.FromError(PlexOAuthError(PlexAccountUnauthorizedMessage));
            }

            return new PlexIdentityCandidates(usernameMatch, emailMatch);
        }

        private static bool HasConflictingProviderId(OmbiUser candidate, string plexUserId)
        {
            return candidate != null &&
                   !string.IsNullOrWhiteSpace(candidate.ProviderUserId) &&
                   !string.Equals(candidate.ProviderUserId, plexUserId, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<PlexUserResolution> ResolveIdentityCandidate(PlexAccount account, PlexIdentityCandidates candidates)
        {
            var identityCandidate = candidates.EmailMatch ?? candidates.UsernameMatch;
            if (identityCandidate == null)
            {
                return PlexUserResolution.Empty();
            }

            if (identityCandidate.UserType == UserType.PlexUser)
            {
                return await ResolveExistingPlexUser(account, identityCandidate);
            }

            if (identityCandidate.UserType == UserType.LocalUser)
            {
                return ResolveLocalUserCandidate(identityCandidate, candidates);
            }

            _log.LogWarning(
                "An existing Ombi account matched the Plex username or email but is not eligible for Plex account linking; refusing authentication.");
            return PlexUserResolution.FromError(PlexOAuthError(PlexAccountUnauthorizedMessage));
        }

        private async Task<PlexUserResolution> ResolveExistingPlexUser(PlexAccount account, OmbiUser identityCandidate)
        {
            if (!string.IsNullOrWhiteSpace(identityCandidate.ProviderUserId))
            {
                return PlexUserResolution.FromUser(identityCandidate);
            }

            identityCandidate.ProviderUserId = account.user.id;
            var updateResult = await _userManager.UpdateAsync(identityCandidate);
            if (updateResult.Succeeded)
            {
                return PlexUserResolution.FromUser(identityCandidate);
            }

            LogIdentityErrors(updateResult.Errors, "Failed to backfill Plex ProviderUserId");
            return PlexUserResolution.FromError(PlexOAuthError("Failed to update the existing Plex user"));
        }

        private PlexUserResolution ResolveLocalUserCandidate(OmbiUser identityCandidate, PlexIdentityCandidates candidates)
        {
            var sameAccountByEmail = candidates.EmailMatch != null;
            var sameAccountByUsernameWithoutEmail = candidates.UsernameMatch != null &&
                                                    string.IsNullOrWhiteSpace(identityCandidate.Email);

            if (sameAccountByEmail || sameAccountByUsernameWithoutEmail)
            {
                return PlexUserResolution.ForLocalLink(identityCandidate);
            }

            _log.LogWarning("A local Ombi account matches the Plex username but has a different email; refusing automatic account linking.");
            return PlexUserResolution.FromError(PlexOAuthError(PlexAccountUnauthorizedMessage));
        }

        private async Task<bool> IsConfiguredPlexServerOwner(PlexAccount account)
        {
            var plexSettings = await _plexSettings.GetSettingsAsync();
            if (string.IsNullOrEmpty(account.user.id) || plexSettings?.Servers == null)
            {
                return false;
            }

            if (ConfiguredServerUsesAuthenticationToken(plexSettings, account.user.authentication_token))
            {
                return true;
            }

            var serverTokens = GetDistinctServerTokens(plexSettings, account.user.authentication_token);
            foreach (var token in serverTokens)
            {
                if (await TokenBelongsToPlexAccount(token, account.user.id))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ConfiguredServerUsesAuthenticationToken(PlexSettings plexSettings, string authenticationToken)
        {
            if (string.IsNullOrEmpty(authenticationToken))
            {
                return false;
            }

            return plexSettings.Servers.Any(server =>
                !string.IsNullOrEmpty(server.PlexAuthToken) &&
                string.Equals(server.PlexAuthToken, authenticationToken, StringComparison.Ordinal));
        }

        private static IEnumerable<string> GetDistinctServerTokens(PlexSettings plexSettings, string authenticationToken)
        {
            return plexSettings.Servers
                .Select(server => server.PlexAuthToken)
                .Where(token => !string.IsNullOrEmpty(token) &&
                                (string.IsNullOrEmpty(authenticationToken) ||
                                 !string.Equals(token, authenticationToken, StringComparison.Ordinal)))
                .Distinct();
        }

        private async Task<bool> TokenBelongsToPlexAccount(string token, string plexUserId)
        {
            try
            {
                var serverAdminAccount = await _plexOAuthManager.GetAccount(token);
                return serverAdminAccount?.user != null &&
                       !string.IsNullOrEmpty(serverAdminAccount.user.id) &&
                       string.Equals(serverAdminAccount.user.id, plexUserId, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to retrieve Plex account for token verification during Plex Admin OAuth import.");
                return false;
            }
        }

        private async Task<PlexUserResolution> LinkOrCreatePlexAdmin(
            PlexAccount account,
            string plexUserName,
            OmbiUser matchingUser)
        {
            if (matchingUser != null)
            {
                return await LinkExistingLocalAdmin(account, matchingUser);
            }

            return await CreatePlexAdmin(account, plexUserName);
        }

        private async Task<PlexUserResolution> LinkExistingLocalAdmin(PlexAccount account, OmbiUser matchingUser)
        {
            if (!await _userManager.IsInRoleAsync(matchingUser, OmbiRoles.Admin))
            {
                _log.LogWarning(
                    "Verified Plex server owner matches an existing Ombi user that is not an Admin; refusing automatic account linking.");
                return PlexUserResolution.Empty();
            }

            if (matchingUser.UserType != UserType.LocalUser)
            {
                _log.LogWarning(
                    "Verified Plex server owner matches an Ombi user type that cannot be linked automatically; refusing account linking.");
                return PlexUserResolution.Empty();
            }

            matchingUser.ProviderUserId = account.user.id;
            var linkResult = await _userManager.UpdateAsync(matchingUser);
            if (!linkResult.Succeeded)
            {
                LogIdentityErrors(linkResult.Errors, "Failed to link existing Ombi admin to Plex owner");
                return PlexUserResolution.FromError(
                    PlexOAuthError("Failed to link the existing Ombi admin account to the Plex server owner"));
            }

            _log.LogInformation(
                "Linked an existing Ombi admin to the verified Plex server owner while preserving local authentication.");
            return PlexUserResolution.FromUser(matchingUser);
        }

        private async Task<PlexUserResolution> CreatePlexAdmin(PlexAccount account, string plexUserName)
        {
            var userManagementSettings = await _userManagementSettings.GetSettingsAsync();
            var user = new OmbiUser
            {
                UserType = UserType.PlexUser,
                UserName = plexUserName,
                ProviderUserId = account.user.id,
                Email = account.user.email ?? string.Empty,
                Alias = string.Empty,
                StreamingCountry = userManagementSettings.DefaultStreamingCountry ?? string.Empty
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                return await AssignAdminRoleToCreatedPlexUser(user);
            }

            return await RecoverConcurrentPlexAdminCreation(account.user.id, plexUserName, createResult.Errors);
        }

        private async Task<PlexUserResolution> AssignAdminRoleToCreatedPlexUser(OmbiUser user)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, OmbiRoles.Admin);
            if (roleResult.Succeeded)
            {
                return PlexUserResolution.FromUser(user);
            }

            LogIdentityErrors(roleResult.Errors, $"Failed to add auto-created Plex admin user {user.UserName} to Admin role");
            await TryDeleteFailedPlexAdmin(user);
            return PlexUserResolution.FromError(
                PlexOAuthError("Failed to assign admin permissions to the auto-created Plex admin user"));
        }

        private async Task TryDeleteFailedPlexAdmin(OmbiUser user)
        {
            try
            {
                await _userManager.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                _log.LogError(ex,
                    "Failed to roll back auto-created Plex admin user {UserName} after role assignment failure",
                    user.UserName);
            }
        }

        private async Task<PlexUserResolution> RecoverConcurrentPlexAdminCreation(
            string plexUserId,
            string plexUserName,
            IEnumerable<Microsoft.AspNetCore.Identity.IdentityError> createErrors)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x =>
                x.ProviderUserId == plexUserId && x.UserType == UserType.PlexUser);

            if (user == null)
            {
                LogIdentityErrors(createErrors, $"Failed to auto-create Plex admin user {plexUserName}");
                return PlexUserResolution.Empty();
            }

            if (await _userManager.IsInRoleAsync(user, OmbiRoles.Admin))
            {
                return PlexUserResolution.FromUser(user);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, OmbiRoles.Admin);
            if (roleResult.Succeeded)
            {
                return PlexUserResolution.FromUser(user);
            }

            LogIdentityErrors(roleResult.Errors, $"Failed to add fallback Plex admin user {user.UserName} to Admin role");
            return PlexUserResolution.FromError(
                PlexOAuthError("Failed to assign admin permissions to the fallback Plex admin user"));
        }

        private void LogIdentityErrors(IEnumerable<Microsoft.AspNetCore.Identity.IdentityError> errors, string operation)
        {
            foreach (var error in errors)
            {
                _log.LogError("{Operation}: {Description}", operation, error.Description);
            }
        }

        private static string GetPlexUserName(PlexAccount account)
        {
            return !string.IsNullOrEmpty(account.user.username) ? account.user.username : account.user.id;
        }

        private JsonResult PlexOAuthError(string message)
        {
            return new JsonResult(new { errorMessage = message });
        }

        private sealed class PlexIdentityCandidates
        {
            public PlexIdentityCandidates(OmbiUser usernameMatch, OmbiUser emailMatch)
            {
                UsernameMatch = usernameMatch;
                EmailMatch = emailMatch;
            }

            private PlexIdentityCandidates(IActionResult error)
            {
                Error = error;
            }

            public OmbiUser UsernameMatch { get; }
            public OmbiUser EmailMatch { get; }
            public IActionResult Error { get; }

            public static PlexIdentityCandidates FromError(IActionResult error)
            {
                return new PlexIdentityCandidates(error);
            }
        }

        private sealed class PlexUserResolution
        {
            private PlexUserResolution(OmbiUser user = null, OmbiUser localLinkCandidate = null, IActionResult error = null)
            {
                User = user;
                LocalLinkCandidate = localLinkCandidate;
                Error = error;
            }

            public OmbiUser User { get; }
            public OmbiUser LocalLinkCandidate { get; }
            public IActionResult Error { get; }

            public static PlexUserResolution Empty()
            {
                return new PlexUserResolution();
            }

            public static PlexUserResolution FromUser(OmbiUser user)
            {
                return new PlexUserResolution(user: user);
            }

            public static PlexUserResolution ForLocalLink(OmbiUser user)
            {
                return new PlexUserResolution(localLinkCandidate: user);
            }

            public static PlexUserResolution FromError(IActionResult error)
            {
                return new PlexUserResolution(error: error);
            }
        }

        /// <summary>
        /// Refreshes the token.
        /// </summary>
        /// <param name="token">The model.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("refresh")]
        [ProducesResponseType(401)]
        public IActionResult RefreshToken([FromBody] TokenRefresh token)
        {

            // Check if token exists
            var dbToken = _token.GetToken(token.Token).FirstOrDefault();
            if (dbToken == null)
            {
                return new UnauthorizedResult();
            }


            throw new NotImplementedException();
        }

        [HttpPost("requirePassword")]
        public async Task<bool> DoesUserRequireAPassword([FromBody] UserAuthModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                // Could this be an email login?
                user = await _userManager.FindByEmailAsync(model.Username);

                if (user == null)
                {
                    return true;
                }
            }

            var requires = await _userManager.RequiresPassword(user);
            return requires;
        }

        public class TokenRefresh
        {
            public string Token { get; set; }
            public string Userename { get; set; }
        }


        [HttpPost("header_auth")]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> HeaderAuth()
        {
            var authSettings = await _authSettings.GetSettingsAsync();
            _log.LogInformation("Logging with header: " + authSettings.HeaderAuthVariable);
            if (authSettings.HeaderAuthVariable != null && authSettings.EnableHeaderAuth)
            {
                if (Request.HttpContext?.Request?.Headers != null && Request.HttpContext.Request.Headers.ContainsKey(authSettings.HeaderAuthVariable))
                {
                    var username = Request.HttpContext.Request.Headers[authSettings.HeaderAuthVariable].ToString();

                    // Check if user exists
                    var user = await _userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        if (authSettings.HeaderAuthCreateUser)
                        {
                            var defaultSettings = await _userManagementSettings.GetSettingsAsync();
                            user = new OmbiUser {
                                UserName = username,
                                UserType = UserType.LocalUser,
                                StreamingCountry = defaultSettings.DefaultStreamingCountry ?? "US",
                                MovieRequestLimit = defaultSettings.MovieRequestLimit,
                                MovieRequestLimitType = defaultSettings.MovieRequestLimitType,
                                EpisodeRequestLimit = defaultSettings.EpisodeRequestLimit,
                                EpisodeRequestLimitType = defaultSettings.EpisodeRequestLimitType,
                                MusicRequestLimit = defaultSettings.MusicRequestLimit,
                                MusicRequestLimitType = defaultSettings.MusicRequestLimitType,
                            };

                            await _userManager.CreateAsync(user);
                            await _userManager.AddToRolesAsync(user, defaultSettings.DefaultRoles);
                        }
                        else
                        {
                            return new UnauthorizedResult();
                        }
                    }

                    return await CreateToken(true, user);
                }
                else
                {
                    return new UnauthorizedResult();
                }
            }    
            else
            {
                return new UnauthorizedResult();
            }
        }
    }
}
