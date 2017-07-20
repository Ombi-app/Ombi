using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Api.Emby.Models;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Entities;

namespace Ombi.Core.IdentityResolver
{
    public class OmbiOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public OmbiOwnerPasswordValidator(UserManager<OmbiUser> um, IPlexApi api,
            ISettingsService<PlexSettings> settings)
        {
            UserManager = um;
            Api = api;
            PlexSettings = settings;
        }

        private UserManager<OmbiUser> UserManager { get; }
        private IPlexApi Api { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var users = UserManager.Users;
            if (await LocalUser(context, users))
            {
                return;
            }
            if (await PlexUser(context, users))
            {
                return;
            }
            if (await EmbyUser(context, users))
            {
                return;
            }
        }

        private async Task<bool> PlexUser(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users)
        {
            var signInResult = await Api.SignIn(new UserRequest {login = context.UserName, password = context.Password});
            if (signInResult.user == null)
            {
                return false;
            }

            // Do we have a local user?
            var user = await users.FirstOrDefaultAsync(x => x.UserName == context.UserName && x.UserType == UserType.PlexUser);

            throw new NotImplementedException(); // TODO finish
        }

        private Task<bool> EmbyUser(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> LocalUser(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users)
        {
            var user = await users.FirstOrDefaultAsync(x => x.UserName == context.UserName && x.UserType == UserType.LocalUser);

            if (user == null)
            {
                //context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Username or password is incorrect");
                return false;
            }

            var passwordValid = await UserManager.CheckPasswordAsync(user, context.Password);
            if (!passwordValid)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Username or password is incorrect");
                return true;

            }

            var roles = await UserManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            context.Result = new GrantValidationResult(user.UserName, "password", claims);

            return true;
        }
    }
}