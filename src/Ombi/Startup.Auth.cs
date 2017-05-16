using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ombi.Auth;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models;

namespace Ombi
{
    public partial class Startup
    {

        public SymmetricSecurityKey signingKey;
        private void ConfigureAuth(IApplicationBuilder app)
        {

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("secretkey_secretkey123!"));

            var tokenProviderOptions = new TokenProviderOptions
            {
                Path = "/api/v1/token/",
                Audience = "DemoAudience",
                Issuer = "DemoIssuer",
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                IdentityResolver = GetIdentity
            };

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = "DemoIssuer",
                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = "DemoAudience",
                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero, 
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters,
            });

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(tokenProviderOptions));
        }


        private async Task<ClaimsIdentity> GetIdentity(string username, string password, IUserIdentityManager userIdentityManager)
        {
            var validLogin = await userIdentityManager.CredentialsValid(username, password);
            if (!validLogin)
            {
                return null;
            }

            var user = await userIdentityManager.GetUser(username);
            var claim = new ClaimsIdentity(new GenericIdentity(user.Username, "Token"), user.Claims);
            return claim;
        }
    }
}