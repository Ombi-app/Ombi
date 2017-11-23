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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Api.Emby;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Core.Authentication
{
    public class OmbiUserManager : UserManager<OmbiUser>
    {
        public OmbiUserManager(IUserStore<OmbiUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<OmbiUser> passwordHasher, IEnumerable<IUserValidator<OmbiUser>> userValidators,
            IEnumerable<IPasswordValidator<OmbiUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<OmbiUser>> logger, IPlexApi plexApi,
            IEmbyApi embyApi, ISettingsService<EmbySettings> embySettings)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _plexApi = plexApi;
            _embyApi = embyApi;
            _embySettings = embySettings;
        }

        private readonly IPlexApi _plexApi;
        private readonly IEmbyApi _embyApi;
        private readonly ISettingsService<EmbySettings> _embySettings;

        public override async Task<bool> CheckPasswordAsync(OmbiUser user, string password)
        {
            if (user.UserType == UserType.LocalUser)
            {
                return await base.CheckPasswordAsync(user, password);
            }
            if (user.UserType == UserType.PlexUser)
            {
                return await CheckPlexPasswordAsync(user, password);
            }
            if (user.UserType == UserType.EmbyUser)
            {
                return await CheckEmbyPasswordAsync(user, password);
            }
            return false;
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
            var result = await _plexApi.SignIn(new UserRequest { password = password, login = user.UserName });
            if (result.user?.authentication_token != null)
            {
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
        private async Task<bool> CheckEmbyPasswordAsync(OmbiUser user, string password)
        {
            if (user.IsEmbyConnect)
            {
                var result = await _embyApi.LoginConnectUser(user.UserName, password);
                if (result.AccessToken.HasValue())
                {
                    return true;
                }
            }

            var embySettings = await _embySettings.GetSettingsAsync();
            foreach (var server in embySettings.Servers)
            {
                try
                {
                    var result = await _embyApi.LogIn(user.UserName, password, server.ApiKey, server.FullUri);
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
    }
}