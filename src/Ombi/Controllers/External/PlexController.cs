using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Models.External;

namespace Ombi.Controllers.External
{
    [Admin]
    public class PlexController : BaseV1ApiController
    {
        public PlexController(IPlexApi plexApi, ISettingsService<PlexSettings> plexSettings)
        {
            PlexApi = plexApi;
            PlexSettings = plexSettings;
        }

        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }

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

                settings.Enable = true;
                settings.Servers = new List<PlexServers> { new PlexServers{
PlexAuthToken = result.user.authentication_token,
                        Id = new Random().Next(),
                        Ip = servers.LocalAddresses.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(),
                        MachineIdentifier = servers.MachineIdentifier,
                        Port = int.Parse(servers.Port),
                        Ssl = servers.Scheme != "http",
                        Name = $"Server 1",
                }
                };
                //var serverNumber = 0;
                //foreach (var s in servers)
                //{
                //    if (string.IsNullOrEmpty(s.LocalAddresses) || string.IsNullOrEmpty(s.Port))
                //    {
                //        continue;
                //    }
                //    settings.Servers.Add(new PlexServers
                //    {
                //        PlexAuthToken = result.user.authentication_token,
                //        Id = new Random().Next(),
                //        Ip = s.LocalAddresses.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(),
                //        MachineIdentifier = s.MachineIdentifier,
                //        Port = int.Parse(s.Port),
                //        Ssl = s.Scheme != "http",
                //        Name = $"Server{serverNumber++}"
                //    });
                //}

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
        public async Task<PlexLibraries> GetPlexLibraries([FromBody] PlexServers settings)
        {
            var libs = await PlexApi.GetLibrarySections(settings.PlexAuthToken, settings.FullUri);

            return libs;
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


    }
}
