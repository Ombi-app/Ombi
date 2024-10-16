#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: OmbiUserManager.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Api.Emby;
using Ombi.Api.Jellyfin;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Authentication
{
    public class OmbiUserManager : UserManager<OmbiUser>
    {
        public OmbiUserManager(IUserStore<OmbiUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<OmbiUser> passwordHasher, IEnumerable<IUserValidator<OmbiUser>> userValidators,
            IEnumerable<IPasswordValidator<OmbiUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<OmbiUser>> logger, IPlexApi plexApi,
            IEmbyApiFactory embyApi, ISettingsService<EmbySettings> embySettings,
            IJellyfinApiFactory jellyfinApi, ISettingsService<JellyfinSettings> jellyfinSettings,
            ISettingsService<AuthenticationSettings> auth, IRepository<PlexWatchlistUserError> userError)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _plexApi = plexApi;
            _embyApi = embyApi;
            _jellyfinApi = jellyfinApi;
            _embySettings = embySettings;
            _jellyfinSettings = jellyfinSettings;
            _authSettings = auth;
            _userError = userError;
            _logger = logger;
        }

        private readonly IPlexApi _plexApi;
        private readonly IEmbyApiFactory _embyApi;
        private readonly IJellyfinApiFactory _jellyfinApi;
        private readonly ISettingsService<EmbySettings> _embySettings;
        private readonly ISettingsService<JellyfinSettings> _jellyfinSettings;
        private readonly ISettingsService<AuthenticationSettings> _authSettings;
        private readonly IRepository<PlexWatchlistUserError> _userError;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private string _clientIpAddress;
        public string ClientIpAddress { get => _clientIpAddress; set => _clientIpAddress = value; }

        public override async Task<bool> CheckPasswordAsync(OmbiUser user, string password)
        {
            var requiresPassword = await RequiresPassword(user);
            if (!requiresPassword)
            {
                // Let them through!
                return true;
            }
            if (user.UserType == UserType.LocalUser)
            {
                return await base.CheckPasswordAsync(user, password);
            }
            if (user.UserType == UserType.PlexUser)
            {
                return await CheckPlexPasswordAsync(user, password);
            }
            if (user.UserType == UserType.EmbyUser || user.UserType == UserType.EmbyConnectUser)
            {
                return await CheckEmbyPasswordAsync(user, password, ClientIpAddress);
            }
            if (user.UserType == UserType.JellyfinUser)
            {
                return await CheckJellyfinPasswordAsync(user, password);
            }
            return false;
        }

        public async Task<bool> RequiresPassword(OmbiUser user)
        {
            var authSettings = await _authSettings.GetSettingsAsync();
            if (authSettings.AllowNoPassword)
            {
                var roles = await GetRolesAsync(user);
                if (roles.Contains(OmbiRoles.Admin) || roles.Contains(OmbiRoles.PowerUser))
                {
                    // We require a password
                    return true;
                }
                return false;
            }
            return true;
        }

        public async Task<OmbiUser> GetOmbiUserFromPlexToken(string plexToken)
        {
            var plexAccount = await _plexApi.GetAccount(plexToken);
            
            // Check for a ombi user
            if (plexAccount?.user == null)
            {
                return null;
            }

            var potentialOmbiUser = await Users.FirstOrDefaultAsync(x =>
                x.ProviderUserId == plexAccount.user.id);
            // Update ombi user with the token

            if (potentialOmbiUser != null)
            {
                potentialOmbiUser.MediaServerToken = plexAccount.user.authentication_token;
                await UpdateAsync(potentialOmbiUser);
            }

            return potentialOmbiUser;            

        }
        
        public async Task<ICollection<OmbiUser>> GetPlexUsersWithValidTokens() 
        {
            var plexUsersWithTokens = Users.Where(x => x.UserType == UserType.PlexUser && x.MediaServerToken != null).ToList();
            _logger.LogDebug($"Found {plexUsersWithTokens.Count} users with tokens");
            var result = new List<OmbiUser>();

            foreach (var user in plexUsersWithTokens)
            {
                // Check if the user has errors and the token is the same (not refreshed)
                var failedUser = await _userError.GetAll().Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                if (failedUser != null)
                {
                    if (failedUser.MediaServerToken.Equals(user.MediaServerToken))
                    {
                        _logger.LogWarning($"Skipping user '{user.UserName}' as they failed previously and the token has not yet been refreshed. They need to re-authenticate with Ombi");
                        continue;
                    }
                    else
                    {
                        // remove that guy
                        await _userError.Delete(failedUser);
                        failedUser = null;
                    }
                }
                if (failedUser == null)
                {
                    result.Add(user);
                }
            }
            return result;

        }

        /// <summary>
        /// Sign the user into plex and make sure we can get the authentication token.
        /// <remarks>We do not check if the user is in the owners "friends" since they must have a local user account to get this far</remarks>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> CheckPlexPasswordAsync(OmbiUser user, string password)
        {
            var login = user.EmailLogin ? user.Email : user.UserName;
            var result = await _plexApi.SignIn(new UserRequest { password = password, login = login });
            if (result.user?.authentication_token != null)
            {
                // Update ombi user with the token
                user.MediaServerToken = result.user?.authentication_token;
                await UpdateAsync(user);
                
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sign the user into Emby
        /// <remarks>We do not check if the user is in the owners "friends" since they must have a local user account to get this far.
        /// We also have to try and authenticate them with every server, the first server that work we just say it was a success</remarks>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> CheckEmbyPasswordAsync(OmbiUser user, string password, string clientIpAddress="")
        {
            var embySettings = await _embySettings.GetSettingsAsync();
            var client = _embyApi.CreateClient(embySettings);

            if (user.IsEmbyConnect)
            {
                var result = await client.LoginConnectUser(user.UserName, password);
                if (result.AccessToken.HasValue())
                {
                    // We cannot update the email address in the user importer due to there is no way 
                    // To get this info from Emby Connect without the username and password.
                    // So we do it here!
                    var email = user.Email ?? string.Empty;
                    if (!email.Equals(result.User?.Email))
                    {
                        user.Email = result.User?.Email;
                        await UpdateAsync(user);
                    }

                    return true;
                }
            }

            foreach (var server in embySettings.Servers)
            {
                try
                {
                    var result = await client.LogIn(user.UserName, password, server.ApiKey, server.FullUri, clientIpAddress);
                    if (result != null)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Emby Login Failed");
                }
            }
            return false;
        }

        /// <summary>
        /// Sign the user into Jellyfin
        /// <remarks>We do not check if the user is in the owners "friends" since they must have a local user account to get this far.
        /// We also have to try and authenticate them with every server, the first server that work we just say it was a success</remarks>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> CheckJellyfinPasswordAsync(OmbiUser user, string password)
        {
            var jellyfinSettings = await _jellyfinSettings.GetSettingsAsync();
            var client = _jellyfinApi.CreateClient(jellyfinSettings);

            foreach (var server in jellyfinSettings.Servers)
            {
                try
                {
                    var result = await client.LogIn(user.UserName, password, server.ApiKey, server.FullUri);
                    if (result != null)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Jellyfin Login Failed");
                }
            }
            return false;
        }
    }
}
