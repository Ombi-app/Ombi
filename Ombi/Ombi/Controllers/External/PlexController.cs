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
            if (!string.IsNullOrEmpty(settings?.PlexAuthToken)) return null;

            var result = await PlexApi.SignIn(request);
            if (!string.IsNullOrEmpty(result.user?.authentication_token))
            {
                var server = await PlexApi.GetServer(result.user.authentication_token);
                var firstServer = server.Server.FirstOrDefault();
                await PlexSettings.SaveSettingsAsync(new PlexSettings
                {
                    Enable = true,
                    PlexAuthToken = result.user.authentication_token,
                    Ip = firstServer.LocalAddresses,
                    MachineIdentifier = firstServer.MachineIdentifier,
                    Port = int.Parse(firstServer.Port),
                    Ssl = firstServer.Scheme != "http",
                });
            }

            return result;
        }

        
    }
}
