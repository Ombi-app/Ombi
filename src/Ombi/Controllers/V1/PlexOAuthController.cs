using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;

namespace Ombi.Controllers.V1
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiV1]
    [AllowAnonymous]
    [ApiController]
    public class PlexOAuthController : Controller
    {
        public PlexOAuthController(IPlexOAuthManager manager, IPlexApi plexApi, ISettingsService<PlexSettings> plexSettings,
            ILogger<PlexOAuthController> log)
        {
            _manager = manager;
            _plexApi = plexApi;
            _plexSettings = plexSettings;
            _log = log;
        }

        private readonly IPlexOAuthManager _manager;
        private readonly IPlexApi _plexApi;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly ILogger _log;

        [HttpGet("{pinId:int}")]
        public async Task<IActionResult> OAuthWizardCallBack([FromRoute] int pinId)
        {
            var accessToken = await _manager.GetAccessTokenFromPin(pinId);
            if (accessToken.IsNullOrEmpty())
            {
                return Json(new
                {
                    success = false,
                    error = "Authentication did not work. Please try again"
                });
            }
            var settings = await _plexSettings.GetSettingsAsync();
            var server = await _plexApi.GetServer(accessToken);
            var servers = server.Server.FirstOrDefault();
            if (servers == null)
            {
                _log.LogWarning("Looks like we can't find any Plex Servers");
            }
            _log.LogDebug("Adding first server");

            settings.Enable = true;
            settings.Servers = new List<PlexServers> {
                new PlexServers
                {
                    PlexAuthToken = accessToken,
                    Id = new Random().Next(),
                    Ip = servers?.LocalAddresses?.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries)?.FirstOrDefault() ?? string.Empty,
                    MachineIdentifier = servers?.MachineIdentifier ?? string.Empty,
                    Port = int.Parse(servers?.Port ?? "0"),
                    Ssl = (servers?.Scheme ?? "http") != "http",
                    Name = "Server 1",
                }
            };

            await _plexSettings.SaveSettingsAsync(settings);
            return Json(new { accessToken });
        }
    }
}
