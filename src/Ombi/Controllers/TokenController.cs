using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Models.Identity;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using StackExchange.Profiling.Helpers;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    public class TokenController
    {
        public TokenController(OmbiUserManager um, IOptions<TokenAuthentication> ta, IAuditRepository audit, ITokenRepository token)
        {
            _userManager = um;
            _tokenAuthenticationOptions = ta.Value;
            _audit = audit;
            _token = token;
        }

        private readonly TokenAuthentication _tokenAuthenticationOptions;
        private readonly IAuditRepository _audit;
        private readonly ITokenRepository _token;
        private readonly OmbiUserManager _userManager;

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] UserAuthModel model)
        {
            await _audit.Record(AuditType.None, AuditArea.Authentication,
                $"UserName {model.Username} attempting to authenticate");

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                // Could this be an email login?
                user = await _userManager.FindByEmailAsync(model.Username);

                if (user == null)
                {
                    return new UnauthorizedResult();
                }
            }

            // Verify Password
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains(OmbiRoles.Disabled))
                {
                    return new UnauthorizedResult();
                }

                user.LastLoggedIn = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };
                claims.AddRange(roles.Select(role => new Claim("role", role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenAuthenticationOptions.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(5),
                    signingCredentials: creds,
                    audience: "Ombi", issuer:"Ombi"
                );
                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                if (model.RememberMe)
                {
                    // Save the token so we can refresh it later
                    //await _token.CreateToken(new Tokens() {Token = accessToken, User = user});
                }

                return new JsonResult(new
                {
                    access_token = accessToken,
                    expiration = token.ValidTo
                });
            }

            return new UnauthorizedResult();
        }

        /// <summary>
        /// Refreshes the token.
        /// </summary>
        /// <param name="token">The model.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("refresh")]
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

    }
}