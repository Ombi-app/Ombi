﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Models.External;
using Ombi.Models.Identity;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public TokenController(OmbiUserManager um, IOptions<TokenAuthentication> ta, ITokenRepository token,
            IPlexOAuthManager oAuthManager, ILogger<TokenController> logger, 
            ISettingsService<AuthenticationSettings> auth, ISettingsService<CloudflareAuthenticationSettings> cfauth)
        {
            _userManager = um;
            _tokenAuthenticationOptions = ta.Value;
            _token = token;
            _plexOAuthManager = oAuthManager;
            _log = logger;
            _authSettings = auth;
            _cfsettings = cfauth;
            _ = updateCFKeys(true);
        }

        private readonly TokenAuthentication _tokenAuthenticationOptions;
        private readonly ITokenRepository _token;
        private readonly OmbiUserManager _userManager;
        private readonly IPlexOAuthManager _plexOAuthManager;
        private readonly ILogger<TokenController> _log;
        private readonly ISettingsService<AuthenticationSettings> _authSettings;
        private readonly ISettingsService<CloudflareAuthenticationSettings> _cfsettings;

        private CloudflareJWTJson _cfJson;

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(401)]
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
        [ProducesResponseType(200)]
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


        /// <summary>
        /// Returns the Token for the Ombi User if we can match the Plex user with a valid Ombi User
        /// </summary>
        [HttpGet("cfAuth")]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetTokenWithCFJwt([FromHeader(Name="Cf-Access-Jwt-Assertion")] string CFJWTAuthentication)
        {
            if (!_authSettings.GetSettings().EnableCloudflareAccess) {
                _log.LogError("EnableCloudflareAccess is disabled, API won't work!");
                return Unauthorized();
            }

            if (String.IsNullOrEmpty(CFJWTAuthentication)) {
                _log.LogError("Cf-Access-Jwt-Assertion Not found!");
                return Unauthorized();
            }
  
            updateCFKeys(false);
            
            IList<JsonWebKey> jwkList = new List<JsonWebKey>();
            foreach (Dictionary<string,string> key in _cfJson.keys) {
                jwkList.Add(JsonWebKey.Create(JsonSerializer.Serialize(key)));
            }
            
            CloudflareAuthenticationSettings cfSettings = _cfsettings.GetSettings();

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = cfSettings.issuer,
                ValidateIssuer = true,
                ValidAudience = cfSettings.audience,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwkList,
                TryAllIssuerSigningKeys	= true
            };
            string email = null;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal claimsP = handler.ValidateToken(CFJWTAuthentication, validationParameters, out var validatedSecurityToken);
                email = claimsP.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
            }
            catch
            {
                return Unauthorized();
            }
            if (String.IsNullOrEmpty(email)) {
                _log.LogError("cfJWT email not found!");
                return Unauthorized();
            }
            
            OmbiUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }
            
            _log.LogInformation(String.Format("Logging in user {0} with cfJWT", email));
            return await CreateToken(false, user);
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

            return new JsonResult(new
            {
                access_token = accessToken,
                expiration = token.ValidTo
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

            // Get the ombi user
            var user = await _userManager.FindByNameAsync(account.user.username);

            if (user == null)
            {
                // Could this be an email login?
                user = await _userManager.FindByEmailAsync(account.user.email);

                if (user == null)
                {
                    return new UnauthorizedResult();
                }
            }

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

        private string GetRequestIP()
        {
            string ip = null;

            if (Request.HttpContext?.Request?.Headers != null && Request.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedip = Request.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
                ip = forwardedip.TrimEnd(',').Split(",").Select(s => s.Trim()).FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(ip) && Request.HttpContext?.Connection?.RemoteIpAddress != null)
                ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip) && Request.HttpContext?.Request?.Headers != null && Request.HttpContext.Request.Headers.ContainsKey("REMOTE_ADDR"))
            {
                var remoteip = Request.HttpContext.Request.Headers["REMOTE_ADDR"].ToString();
                ip = remoteip.TrimEnd(',').Split(",").Select(s => s.Trim()).FirstOrDefault();
            }

            return ip;
        }

        private bool updateCFKeys(bool init) {
            if (!(init || ((DateTime.UtcNow - _cfJson.lastUpdate).TotalDays > 7))) {
                return false;
            }
            
            HttpClient client = new HttpClient();
            Task<string> res = Task.Run<string>(async () => await client.GetStringAsync(_cfsettings.GetSettings().certlink));
            _cfJson = JsonSerializer.Deserialize<CloudflareJWTJson>(res.Result);
            return true;
        }
    }
}
