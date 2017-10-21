using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Models.External;

namespace Ombi.Controllers.External
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public class PlexController : Controller
    {
        public PlexController(IPlexApi plexApi, ISettingsService<PlexSettings> plexSettings,
            ILogger<PlexController> logger)
        {
            PlexApi = plexApi;
            PlexSettings = plexSettings;
            _log = logger;
        }

        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private readonly ILogger<PlexController> _log;

        /// <summary>
        /// Signs into the Plex API.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<PlexAuthentication> SignIn([FromBody] UserRequest request)
        {
            // Do we already have settings?
            var settings = await PlexSettings.GetSettingsAsync();
            if (!settings.Servers?.Any() ?? false) return null;

            var result = await PlexApi.SignIn(request);
            if (!string.IsNullOrEmpty(result.user?.authentication_token))
            {
                var server = await PlexApi.GetServer(result.user.authentication_token);
                var servers = server.Server.FirstOrDefault();
                if (servers == null)
                {
                    _log.LogWarning("Looks like we can't find any Plex Servers");
                }

                settings.Enable = true;
                settings.Servers = new List<PlexServers> {
                    new PlexServers
                    {
                            PlexAuthToken = result.user.authentication_token,
                            Id = new Random().Next(),
                            Ip = servers?.LocalAddresses?.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries)?.FirstOrDefault() ?? string.Empty,
                            MachineIdentifier = servers?.MachineIdentifier ?? string.Empty,
                            Port = int.Parse(servers?.Port ?? "0"),
                            Ssl = (servers?.Scheme ?? "http") != "http",
                            Name = "Server 1",
                    }
                };

                await PlexSettings.SaveSettingsAsync(settings);
            }

            return result;
        }

        /// <summary>
        /// Gets the plex libraries.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Libraries")]
        public async Task<PlexLibrariesResponse> GetPlexLibraries([FromBody] PlexServers settings)
        {
            try
            {
                var libs = await PlexApi.GetLibrarySections(settings.PlexAuthToken, settings.FullUri);

                return new PlexLibrariesResponse
                {
                    Successful = true,
                    Data = libs
                };
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Error thrown when attempting to obtain the plex libs");

                var message = e.InnerException != null ? $"{e.Message} - {e.InnerException.Message}" : e.Message;
                return new PlexLibrariesResponse
                {
                    Successful = false,
                    Message = message
                };
            }
        }

        /// <summary>
        /// Gets the plex servers.
        /// </summary>
        /// <param name="u">The u.</param>
        /// <returns></returns>
        [HttpPost("servers")]
        public async Task<PlexServersViewModel> GetServers([FromBody] UserRequest u)
        {
            try
            {
                var signIn = await PlexApi.SignIn(u);
                var servers = await PlexApi.GetServer(signIn?.user?.authentication_token);

                return new PlexServersViewModel { Servers = servers, Success = true };
            }
            catch (Exception e)
            {
                return new PlexServersViewModel
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }

        /// <summary>
        /// Gets the plex friends.
        /// </summary>
        /// <returns></returns>
        [HttpGet("friends")]
        public async Task<IEnumerable<UsersViewModel>> GetFriends()
        {
            var vm = new List<UsersViewModel>();
            var s = await PlexSettings.GetSettingsAsync();
            foreach (var server in s.Servers)
            {
                var users = await PlexApi.GetUsers(server.PlexAuthToken);
                if (users?.User != null && users.User.Any())
                {
                    vm.AddRange(users.User.Select(u => new UsersViewModel
                    {
                        Username = u.Username,
                        Id = u.Id
                    }));
                }
            }

            // Filter out any dupes
            return vm.DistinctBy(x => x.Id);
        }
    }
}
