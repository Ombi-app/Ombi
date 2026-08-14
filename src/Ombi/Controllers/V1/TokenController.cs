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

        /// <summary>
        /// Creates a strong Plex OAuth PIN from the Ombi backend.
        /// Keeping PIN creation server-side avoids Plex cross-origin PIN creation issues.
        /// </summary>
        [HttpPost("plexpin")]
        [EnableRateLimiting("PlexPinCreation")]
        public async Task<IActionResult> CreatePlexPin()
        {
            var pin = await _plexOAuthManager.CreatePin();
            if (pin?.Result != null)
            {
                return Ok(pin.Result);
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

                var websiteAddress = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                //https://app.plex.tv/auth#?forwardUrl=http://google.com/&clientID=Ombi-Test&context%5Bdevice%5D%5Bproduct%5D=Ombi%20SSO&pinID=798798&code=4lgfd
                var url = await _plexOAuthManager.GetOAuthUrl(model.PlexTvPin.code, websiteAddress);
                if (url == null)
                {
                    return new JsonResult(new
                    {
                        error = "Application URL has not been set"
                    });
                }
                return new JsonResult(new { url = url.ToString(), pinId = model.PlexTvPin.id });
            }

            _log.LogWarning(string.Format("Failed login attempt by IP: {0}", GetRequestIP()));
            return new UnauthorizedResult();
        }

        /// <summary>
        /// Returns the Token for the Ombi User if we can match the Plex user with a valid Ombi User
        /// </summary>
        [HttpPost("plextoken")]
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

            user.LastLoggedIn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Ok(new Token
            {
                AccessToken = accessToken,
                Expiration = token.ValidTo
            });
        }

        [HttpGet("{pinId:int}")]
        [ProducesResponseType(401)]
        public async Task<IActionResult> OAuth(int pinId)
        {
            var accessToken = await _plexOAuthManager.GetAccessTokenFromPin(pinId);

            if (accessToken.IsNullOrEmpty())
            {
                // Looks like we are not authenticated.
                return new JsonResult(new
                {
                    errorMessage = "Could not authenticate with Plex"
                });
            }

            // Let's look for the users account
            var account = await _plexOAuthManager.GetAccount(accessToken);
            if (account?.user == null)
            {
                return new JsonResult(new
                {
                    errorMessage = "Plex account details are invalid or missing"
                });
            }

            if (string.IsNullOrEmpty(account.user.authentication_token))
            {
                return new JsonResult(new
                {
                    errorMessage = "Plex authentication token is missing"
                });
            }

            // Get the Ombi user using the stable Plex account id first.
            OmbiUser user = null;
            if (!string.IsNullOrEmpty(account.user.id))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(x =>
                    x.ProviderUserId == account.user.id &&
                    (x.UserType == UserType.PlexUser || x.UserType == UserType.LocalUser));
            }

            var plexUserName = !string.IsNullOrEmpty(account.user.username) ? account.user.username : account.user.id;

            if (user == null)
            {
                // Preserve the original Plex-user username/email fallback, but only treat an
                // existing LocalUser as a link candidate under stricter identity rules below.
                OmbiUser matchingUser = null;
                OmbiUser usernameMatch = null;

                if (!string.IsNullOrEmpty(plexUserName))
                {
                    usernameMatch = await _userManager.FindByNameAsync(plexUserName);
                    if (usernameMatch?.UserType == UserType.PlexUser)
                    {
                        user = usernameMatch;
                    }
                }

                if (user == null && !string.IsNullOrEmpty(account.user.email))
                {
                    var emailMatch = await _userManager.FindByEmailAsync(account.user.email);
                    if (emailMatch?.UserType == UserType.PlexUser)
                    {
                        user = emailMatch;
                    }
                    else if (emailMatch?.UserType == UserType.LocalUser)
                    {
                        // Matching a verified email is the strongest available way to associate an
                        // existing local Ombi administrator with the Plex server owner.
                        matchingUser = emailMatch;
                    }
                }

                if (user == null && matchingUser == null && usernameMatch?.UserType == UserType.LocalUser)
                {
                    // Local admins created by Ombi's first-run wizard historically have no email.
                    // Permit an exact username match only while that local account still has no email.
                    // If it has an email, require that email to match Plex rather than merging two
                    // identities solely because their usernames happen to be the same.
                    if (string.IsNullOrWhiteSpace(usernameMatch.Email))
                    {
                        matchingUser = usernameMatch;
                    }
                    else if (!string.IsNullOrWhiteSpace(account.user.email) &&
                             string.Equals(usernameMatch.Email, account.user.email, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingUser = usernameMatch;
                    }
                    else
                    {
                        _log.LogWarning(
                            "Plex server owner {PlexUserId} has username {PlexUserName}, which matches local Ombi user {UserName}, but the account emails do not match; refusing automatic account linking.",
                            account.user.id, plexUserName, usernameMatch.UserName);
                    }
                }

                if (user == null)
                {
                    // Check whether this OAuth account is the owner of one of the configured Plex
                    // servers. The OAuth token itself can differ from the token stored in Plex
                    // settings, so compare the stable Plex account ids when necessary.
                    var isPlexAdmin = false;
                    var plexSettings = await _plexSettings.GetSettingsAsync();
                    if (!string.IsNullOrEmpty(account.user.id) && plexSettings?.Servers != null)
                    {
                        foreach (var server in plexSettings.Servers)
                        {
                            if (string.IsNullOrEmpty(server.PlexAuthToken))
                            {
                                continue;
                            }

                            if (!string.IsNullOrEmpty(account.user.authentication_token) &&
                                string.Equals(server.PlexAuthToken, account.user.authentication_token, StringComparison.Ordinal))
                            {
                                isPlexAdmin = true;
                                break;
                            }
                        }

                        if (!isPlexAdmin)
                        {
                            var uniqueTokens = plexSettings.Servers
                                .Select(s => s.PlexAuthToken)
                                .Where(token => !string.IsNullOrEmpty(token) &&
                                                (string.IsNullOrEmpty(account.user.authentication_token) ||
                                                 !string.Equals(token, account.user.authentication_token, StringComparison.Ordinal)))
                                .Distinct()
                                .ToList();

                            foreach (var token in uniqueTokens)
                            {
                                try
                                {
                                    var serverAdminAccount = await _plexOAuthManager.GetAccount(token);
                                    if (serverAdminAccount?.user != null &&
                                        !string.IsNullOrEmpty(serverAdminAccount.user.id) &&
                                        string.Equals(serverAdminAccount.user.id, account.user.id, StringComparison.OrdinalIgnoreCase))
                                    {
                                        isPlexAdmin = true;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _log.LogWarning(ex, "Failed to retrieve Plex account for token verification during Plex Admin OAuth import.");
                                }
                            }
                        }
                    }

                    if (isPlexAdmin)
                    {
                        if (matchingUser != null)
                        {
                            // Link only a verified Plex owner to an existing local Ombi Admin. Keep
                            // the local account type/password intact so Plex is a secondary identity.
                            if (!await _userManager.IsInRoleAsync(matchingUser, OmbiRoles.Admin))
                            {
                                _log.LogWarning(
                                    "Verified Plex server owner {PlexUserId} matches Ombi user {UserName}, but that user is not an Admin; refusing automatic account linking.",
                                    account.user.id, matchingUser.UserName);
                            }
                            else if (matchingUser.UserType != UserType.LocalUser)
                            {
                                _log.LogWarning(
                                    "Verified Plex server owner {PlexUserId} matches Ombi user {UserName}, but that user's provider type {UserType} cannot be linked to Plex automatically.",
                                    account.user.id, matchingUser.UserName, matchingUser.UserType);
                            }
                            else
                            {
                                matchingUser.ProviderUserId = account.user.id;

                                var linkResult = await _userManager.UpdateAsync(matchingUser);
                                if (!linkResult.Succeeded)
                                {
                                    foreach (var err in linkResult.Errors)
                                    {
                                        _log.LogError(
                                            "Failed to link existing Ombi admin {UserName} to Plex owner {PlexUserId}: {Description}",
                                            matchingUser.UserName, account.user.id, err.Description);
                                    }

                                    return new JsonResult(new
                                    {
                                        errorMessage = "Failed to link the existing Ombi admin account to the Plex server owner"
                                    });
                                }

                                _log.LogInformation(
                                    "Linked existing Ombi admin {UserName} to Plex server owner {PlexUserId} while preserving local authentication.",
                                    matchingUser.UserName, account.user.id);
                                user = matchingUser;
                            }
                        }
                        else
                        {
                            // No matching Ombi account exists, so create the Plex owner as an Admin.
                            var userManagementSettings = await _userManagementSettings.GetSettingsAsync();
                            user = new OmbiUser
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
                                var roleResult = await _userManager.AddToRoleAsync(user, OmbiRoles.Admin);
                                if (!roleResult.Succeeded)
                                {
                                    foreach (var err in roleResult.Errors)
                                    {
                                        _log.LogError("Failed to add auto-created Plex admin user {UserName} to Admin role: {Description}", user.UserName, err.Description);
                                    }
                                    try
                                    {
                                        await _userManager.DeleteAsync(user);
                                    }
                                    catch (Exception ex)
                                    {
                                        _log.LogError(ex, "Failed to roll back auto-created Plex admin user {UserName} after role assignment failure", user.UserName);
                                    }
                                    return new JsonResult(new
                                    {
                                        errorMessage = "Failed to assign admin permissions to the auto-created Plex admin user"
                                    });
                                }
                            }
                            else
                            {
                                // In case of a race where the Plex user was created concurrently,
                                // resolve it by provider id and make sure it has the Admin role.
                                user = await _userManager.Users.FirstOrDefaultAsync(x =>
                                    x.ProviderUserId == account.user.id && x.UserType == UserType.PlexUser);

                                if (user != null)
                                {
                                    if (!await _userManager.IsInRoleAsync(user, OmbiRoles.Admin))
                                    {
                                        var roleResult = await _userManager.AddToRoleAsync(user, OmbiRoles.Admin);
                                        if (!roleResult.Succeeded)
                                        {
                                            foreach (var err in roleResult.Errors)
                                            {
                                                _log.LogError("Failed to add fallback Plex admin user {UserName} to Admin role: {Description}", user.UserName, err.Description);
                                            }
                                            return new JsonResult(new
                                            {
                                                errorMessage = "Failed to assign admin permissions to the fallback Plex admin user"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var err in createResult.Errors)
                                    {
                                        _log.LogError("Failed to auto-create Plex admin user {UserName}: {Description}", plexUserName, err.Description);
                                    }
                                }
                            }
                        }
                    }
                }

                if (user == null)
                {
                    _log.LogWarning(
                        "Plex OAuth account {PlexUserId} ({PlexUserName}) could not be matched to an authorized Plex user or linked as the configured Plex server owner.",
                        account.user.id, plexUserName);
                    return new JsonResult(new
                    {
                        errorMessage = "This Plex account is not authorized to access Ombi"
                    });
                }
            }

            user.MediaServerToken = account.user.authentication_token;
            await _userManager.UpdateAsync(user);
 
            return await CreateToken(true, user);
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
