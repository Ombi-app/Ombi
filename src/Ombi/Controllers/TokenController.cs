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
            IApplicationConfigRepository config, IAuditRepository audit, ITokenRepository token)
        {
            _userManager = um;
            _tokenAuthenticationOptions = ta.Value;
            _config = config;
            _audit = audit;
            _token = token;
        }

        private readonly TokenAuthentication _tokenAuthenticationOptions;
        private IApplicationConfigRepository _config;
        private readonly IAuditRepository _audit;
        private readonly ITokenRepository _token;
        private readonly UserManager<OmbiUser> _userManager;

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] UserAuthModel model)
        {
            await _audit.Record(AuditType.None, AuditArea.Authentication,
                $"Username {model.Username} attempting to authenticate");

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                return new UnauthorizedResult();
            }

            // Verify Password
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);

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
                    expires: DateTime.UtcNow.AddHours(5),
                    signingCredentials: creds,
                    audience: "Ombi", issuer:"Ombi"
                );
                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                if (model.RememberMe)
                {
                    // Save the token so we can refresh it later
                    await _token.CreateToken(new Tokens() {Token = accessToken, User = user});
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
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefresh token)
        {

            // Check if token exists
            var dbToken = _token.GetToken(token.Token).FirstOrDefault();
            if (dbToken == null)
            {
                return new UnauthorizedResult();
            }
                

            throw new NotImplementedException();
        }

        public class TokenRefresh
        {
            public string Token { get; set; }
            public string Userename { get; set; }
        }

    }
}