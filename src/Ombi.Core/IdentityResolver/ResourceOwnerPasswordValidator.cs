using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.IdentityResolver
{
    public class OmbiOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public OmbiOwnerPasswordValidator(UserManager<OmbiUser> um, IPlexApi plexApi, IEmbyApi embyApi,
            ISettingsService<PlexSettings> settings, ISettingsService<OmbiSettings> ombiSettings,
            ISettingsService<EmbySettings> embySettings, IAuditRepository log)
        {
            UserManager = um;
            PlexApi = plexApi;
            PlexSettings = settings;
            OmbiSettings = ombiSettings;
            EmbyApi = embyApi;
            EmbySettings = embySettings;
            Audit = log;
        }

        private UserManager<OmbiUser> UserManager { get; }
        private IPlexApi PlexApi { get; }
        private IEmbyApi EmbyApi{ get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private ISettingsService<OmbiSettings> OmbiSettings { get; }
        private IAuditRepository Audit { get; }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            await Audit.Record(AuditType.None, AuditArea.Authentication, $"User {context.UserName} attempted to login", context.UserName);
            var users = UserManager.Users;
            if (await LocalUser(context, users))
            {
                return;
            }
            var ombi = await OmbiSettings.GetSettingsAsync();
            if (ombi.AllowExternalUsersToAuthenticate)
            {
                if (await PlexUser(context, users))
                {
                    return;
                }
                if (await EmbyUser(context, users))
                {
                    return;
                }
            }

            await Audit.Record(AuditType.Fail, AuditArea.Authentication, $"User {context.UserName} failed to login", context.UserName);
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Username or password is incorrect");
        }

        private async Task<bool> PlexUser(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users)
        {
            var signInResult = await PlexApi.SignIn(new UserRequest {login = context.UserName, password = context.Password});
            if (signInResult?.user == null)
            {
                return false;
            }

            // Do we have a local user?
            return await GetUserDetails(context, users, UserType.PlexUser);
        }

        private async Task<bool> EmbyUser(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users)
        {
            var embySettings = await EmbySettings.GetSettingsAsync();
            var signInResult = await EmbyApi.LogIn(context.UserName, context.Password, embySettings.ApiKey,
                embySettings.FullUri);

            if (string.IsNullOrEmpty(signInResult?.Name))
            {
                return false;
            }

            return await GetUserDetails(context, users, UserType.EmbyUser);
        }

        private async Task<bool> GetUserDetails(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users, UserType userType)
        {
            var user = await users.FirstOrDefaultAsync(x => x.UserName == context.UserName && x.UserType == userType);
            if (user != null)
            {
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
            
            // Create the user?            
            return true;
        }

        public async Task<bool> LocalUser(ResourceOwnerPasswordValidationContext context, IQueryable<OmbiUser> users)
        {
            var user = await users.FirstOrDefaultAsync(x => x.UserName == context.UserName && x.UserType == UserType.LocalUser);

            if (user == null)
            {
                return false;
            }

            var passwordValid = await UserManager.CheckPasswordAsync(user, context.Password);
            if (!passwordValid)
            {
                await Audit.Record(AuditType.Fail, AuditArea.Authentication, $"User {context.UserName} failed to login", context.UserName);
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

            await Audit.Record(AuditType.Success, AuditArea.Authentication, $"User {context.UserName} has logged in", context.UserName);
            return true;
        }
    }
}