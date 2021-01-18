#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinUserImporter.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Jellyfin;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.Jellyfin
{
    public class JellyfinUserImporter : IJellyfinUserImporter
    {
        public JellyfinUserImporter(IJellyfinApiFactory api, UserManager<OmbiUser> um, ILogger<JellyfinUserImporter> log,
            ISettingsService<JellyfinSettings> jellyfinSettings, ISettingsService<UserManagementSettings> ums, IHubContext<NotificationHub> notification)
        {
            _apiFactory = api;
            _userManager = um;
            _log = log;
            _jellyfinSettings = jellyfinSettings;
            _userManagementSettings = ums;
            _notification = notification;
        }

        private readonly IJellyfinApiFactory _apiFactory;
        private readonly UserManager<OmbiUser> _userManager;
        private readonly ILogger<JellyfinUserImporter> _log;
        private readonly ISettingsService<JellyfinSettings> _jellyfinSettings;
        private readonly ISettingsService<UserManagementSettings> _userManagementSettings;
        private readonly IHubContext<NotificationHub> _notification;
        private IJellyfinApi Api { get; set; }

        public async Task Execute(IJobExecutionContext job)
        {
            var userManagementSettings = await _userManagementSettings.GetSettingsAsync();
            if (!userManagementSettings.ImportJellyfinUsers)
            {
                return;
            }
            var settings = await _jellyfinSettings.GetSettingsAsync();
            if (!settings.Enable)
            {
                return;
            }

            Api = _apiFactory.CreateClient(settings);

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, $"Jellyfin User Importer Started");
            var allUsers = await _userManager.Users.Where(x => x.UserType == UserType.JellyfinUser).ToListAsync();
            foreach (var server in settings.Servers)
            {
                if (string.IsNullOrEmpty(server.ApiKey))
                {
                    continue;
                }

                var jellyfinUsers = await Api.GetUsers(server.FullUri, server.ApiKey);
                foreach (var jellyfinUser in jellyfinUsers)
                {
                    // Check if we should import this user
                    if (userManagementSettings.BannedJellyfinUserIds.Contains(jellyfinUser.Id))
                    {
                        // Do not import these, they are not allowed into the country.
                        continue;
                    }
                    // Check if this Jellyfin User already exists
                    var existingJellyfinUser = allUsers.FirstOrDefault(x => x.ProviderUserId == jellyfinUser.Id);
                    if (existingJellyfinUser == null)
                    {

                        if (!jellyfinUser.Name.HasValue())
                        {
                            _log.LogInformation("Could not create Jellyfin user since the have no username, JellyfinUserId: {0}", jellyfinUser.Id);
                            continue;
                        }
                        // Create this users
                        var newUser = new OmbiUser
                        {
                            UserName = jellyfinUser.Name,
                            UserType = UserType.JellyfinUser,
                            ProviderUserId = jellyfinUser.Id,
                            MovieRequestLimit = userManagementSettings.MovieRequestLimit,
                            EpisodeRequestLimit = userManagementSettings.EpisodeRequestLimit,
                            StreamingCountry = userManagementSettings.DefaultStreamingCountry
                        };
                        _log.LogInformation("Creating Jellyfin user {0}", newUser.UserName);
                        var result = await _userManager.CreateAsync(newUser);
                        if (!result.Succeeded)
                        {
                            foreach (var identityError in result.Errors)
                            {
                                _log.LogError(LoggingEvents.JellyfinUserImporter, identityError.Description);
                            }
                            continue;
                        }
                        if (userManagementSettings.DefaultRoles.Any())
                        {
                            foreach (var defaultRole in userManagementSettings.DefaultRoles)
                            {
                                await _userManager.AddToRoleAsync(newUser, defaultRole);
                            }
                        }
                    }
                    else
                    {
                        // Do we need to update this user?
                        existingJellyfinUser.UserName = jellyfinUser.Name;

                        await _userManager.UpdateAsync(existingJellyfinUser);
                    }
                }
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Jellyfin User Importer Finished");
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _userManager?.Dispose();
                //_jellyfinSettings?.Dispose();
                //_userManagementSettings?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
