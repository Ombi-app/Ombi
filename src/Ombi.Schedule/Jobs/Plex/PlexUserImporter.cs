using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexUserImporter : IPlexUserImporter
    {
        public PlexUserImporter(IPlexApi api, UserManager<OmbiUser> um, ILogger<PlexUserImporter> log,
            ISettingsService<PlexSettings> plexSettings, ISettingsService<UserManagementSettings> ums, IHubContext<NotificationHub> hub)
        {
            _api = api;
            _userManager = um;
            _log = log;
            _plexSettings = plexSettings;
            _userManagementSettings = ums;
            _notification = hub;
            _plexSettings.ClearCache();
            _userManagementSettings.ClearCache();
        }

        private readonly IPlexApi _api;
        private readonly UserManager<OmbiUser> _userManager;
        private readonly ILogger<PlexUserImporter> _log;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly ISettingsService<UserManagementSettings> _userManagementSettings;
        private readonly IHubContext<NotificationHub> _notification;


        public async Task Execute(IJobExecutionContext job)
        {
            var userManagementSettings = await _userManagementSettings.GetSettingsAsync();
            if (!userManagementSettings.ImportPlexUsers)
            {
                return;
            }
            var settings = await _plexSettings.GetSettingsAsync();
            if (!settings.Enable)
            {
                return;
            }


            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Plex User Importer Started");
            var allUsers = await _userManager.Users.Where(x => x.UserType == UserType.PlexUser).ToListAsync();
            foreach (var server in settings.Servers)
            {
                if (string.IsNullOrEmpty(server.PlexAuthToken))
                {
                    continue;
                }

                await ImportAdmin(userManagementSettings, server, allUsers);

                var users = await _api.GetUsers(server.PlexAuthToken);

                foreach (var plexUser in users.User)
                {
                    // Check if we should import this user
                    if (userManagementSettings.BannedPlexUserIds.Contains(plexUser.Id))
                    {
                        // Do not import these, they are not allowed into the country.
                        continue;
                    }

                    // Check if this Plex User already exists
                    // We are using the Plex USERNAME and Not the TITLE, the Title is for HOME USERS
                    var existingPlexUser = allUsers.FirstOrDefault(x => x.ProviderUserId == plexUser.Id);
                    if (existingPlexUser == null)
                    {

                        if (!plexUser.Username.HasValue())
                        {
                            _log.LogInformation("Could not create Plex user since the have no username, PlexUserId: {0}", plexUser.Id);
                            continue;
                        }

                        if ((plexUser.Email.HasValue()) && await _userManager.FindByEmailAsync(plexUser.Email) != null)
                        {
                            _log.LogWarning($"Cannot add user {plexUser.Username} because their email address is already in Ombi, skipping this user");
                            continue;
                        }
                        // Create this users
                        // We do not store a password against the user since they will authenticate via Plex
                        var newUser = new OmbiUser
                        {
                            UserType = UserType.PlexUser,
                            UserName = plexUser?.Username ?? plexUser.Id,
                            ProviderUserId = plexUser.Id,
                            Email = plexUser?.Email ?? string.Empty,
                            Alias = string.Empty,
                            MovieRequestLimit = userManagementSettings.MovieRequestLimit,
                            EpisodeRequestLimit = userManagementSettings.EpisodeRequestLimit,
                            StreamingCountry = userManagementSettings.DefaultStreamingCountry
                        };
                        _log.LogInformation("Creating Plex user {0}", newUser.UserName);
                        var result = await _userManager.CreateAsync(newUser);
                        if (!LogResult(result))
                        {
                            continue;
                        }
                        if (userManagementSettings.DefaultRoles.Any())
                        {
                            // Get the new user object to avoid any concurrency failures
                            var dbUser =
                                await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == newUser.UserName);
                            foreach (var defaultRole in userManagementSettings.DefaultRoles)
                            {
                                await _userManager.AddToRoleAsync(dbUser, defaultRole);
                            }
                        }
                    }
                    else
                    {
                        // Do we need to update this user?
                        existingPlexUser.Email = plexUser.Email;
                        existingPlexUser.UserName = plexUser.Username;

                        await _userManager.UpdateAsync(existingPlexUser);
                    }
                }
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Plex User Importer Finished");
        }

        private async Task ImportAdmin(UserManagementSettings settings, PlexServers server, List<OmbiUser> allUsers)
        {
            if (!settings.ImportPlexAdmin)
            {
                return;
            }

            var plexAdmin = (await _api.GetAccount(server.PlexAuthToken)).user;

            // Check if the admin is already in the DB
            var adminUserFromDb = allUsers.FirstOrDefault(x =>
                x.ProviderUserId.Equals(plexAdmin.id, StringComparison.CurrentCultureIgnoreCase));

            if (adminUserFromDb != null)
            {
                // Let's update the user
                adminUserFromDb.Email = plexAdmin.email;
                adminUserFromDb.UserName = plexAdmin.username;
                adminUserFromDb.ProviderUserId = plexAdmin.id;
                await _userManager.UpdateAsync(adminUserFromDb);
                return;
            }

            var newUser = new OmbiUser
            {
                UserType = UserType.PlexUser,
                UserName = plexAdmin.username ?? plexAdmin.id,
                ProviderUserId = plexAdmin.id,
                Email = plexAdmin.email ?? string.Empty,
                Alias = string.Empty,
                StreamingCountry = settings.DefaultStreamingCountry
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!LogResult(result))
            {
                return;
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, OmbiRoles.Admin);
            LogResult(roleResult);
        }

        private bool LogResult(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                foreach (var identityError in result.Errors)
                {
                    _log.LogError(LoggingEvents.PlexUserImporter, identityError.Description);
                }
            }
            return result.Succeeded;
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _userManager?.Dispose();
                //_plexSettings?.Dispose();
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