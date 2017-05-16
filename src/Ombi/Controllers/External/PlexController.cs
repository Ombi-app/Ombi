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
                var servers = server.Server;

                settings.Servers = new List<PlexServers>();
                var serverNumber = 0;
                foreach (var s in servers)
                {
                    if (string.IsNullOrEmpty(s.LocalAddresses) || string.IsNullOrEmpty(s.Port))
                    {
                        continue;
                    }
                    settings.Servers.Add(new PlexServers
                    {
                        PlexAuthToken = result.user.authentication_token,
                        Id = new Random().Next(),
                        Ip = s.LocalAddresses.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(),
                        MachineIdentifier = s.MachineIdentifier,
                        Port = int.Parse(s.Port),
                        Ssl = s.Scheme != "http",
                        Name = $"Server{serverNumber++}"
                    });
                }

                await PlexSettings.SaveSettingsAsync(settings);
            }

            return result;
        }

        [HttpPost("Libraries")]
        public async Task<PlexLibraries> GetPlexLibraries([FromBody] PlexServers settings)
        {
            var libs = await PlexApi.GetLibrarySections(settings.PlexAuthToken, settings.FullUri);

            return libs;
        }

        
    }
}
