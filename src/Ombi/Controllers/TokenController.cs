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
using Ombi.Models;
using Ombi.Models.Identity;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Controllers
{
    [ApiV1]
    public class TokenController
    {
        public TokenController(UserManager<OmbiUser> um, IOptions<TokenAuthentication> ta,
            IApplicationConfigRepository config)
        {
            UserManager = um;
            TokenAuthenticationOptions = ta.Value;
            Config = config;
        }

        private TokenAuthentication TokenAuthenticationOptions { get; }
        private IApplicationConfigRepository Config { get; }
        private UserManager<OmbiUser> UserManager { get; }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] UserAuthModel model)
        {

            var user = await UserManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                return null;
            }

            // Verify Password
            if ((await UserManager.CheckPasswordAsync(user, model.Password)))
            {
                // Get the url
                var url = Config.Get(ConfigurationTypes.Url);
                var port = Config.Get(ConfigurationTypes.Port);

#if !DEBUG
                var audience = $"{url}:{port}";
#else

                var audience = $"http://localhost:52038/";
#endif
                var roles = await UserManager.GetRolesAsync(user);

                var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim("name", user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };
                claims.AddRange(roles.Select(role => new Claim("role", role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenAuthenticationOptions.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(5),
                    signingCredentials: creds,
                    audience: "Ombi", issuer:"Ombi"
                );

                return new JsonResult(new
                {
                    access_token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return null;
        }

        /// <summary>
        /// Refreshes the token.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] UserAuthModel model)
        {
            throw new NotImplementedException();
        }

    }
}