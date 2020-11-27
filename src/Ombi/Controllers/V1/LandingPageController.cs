using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Emby;
using Ombi.Api.Plex;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Models;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [AllowAnonymous]
    [Produces("application/json")]
    [ApiController]
    public class LandingPageController : ControllerBase
    {
        public LandingPageController(ISettingsService<PlexSettings> plex, ISettingsService<EmbySettings> emby,
            IPlexApi plexApi, IEmbyApiFactory embyApi)
        {
            _plexSettings = plex;
            _embySettings = emby;
            _plexApi = plexApi;
            _embyApi = embyApi;
        }

        private readonly IPlexApi _plexApi;
        private readonly IEmbyApiFactory _embyApi;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly ISettingsService<EmbySettings> _embySettings;


        [HttpGet]
        public async Task<MediaSeverAvailibilityViewModel> GetMediaServerStatus()
        {
            var model = new MediaSeverAvailibilityViewModel();

            var plex = await _plexSettings.GetSettingsAsync();
            if (plex.Enable)
            {

                foreach (var s in plex.Servers)
                {
                    try
                    {
                        var result = await _plexApi.GetStatus(s.PlexAuthToken, s.FullUri);
                        if (!string.IsNullOrEmpty(result.MediaContainer?.version))
                        {
                            model.ServersAvailable++;
                        }
                        else
                        {
                            model.ServersUnavailable++;
                        }
                    }
                    catch (Exception)
                    {
                        model.ServersUnavailable++;
                    }
                }
            }

            var emby = await _embySettings.GetSettingsAsync();
            if (emby.Enable)
            {
                var client = _embyApi.CreateClient(emby);
                foreach (var server in emby.Servers)
                {
                    try
                    {
                        var result = await client.GetUsers(server.FullUri, server.ApiKey);
                        if (result.Any())
                        {
                            model.ServersAvailable++;
                        }
                        else
                        {
                            model.ServersUnavailable++;
                        }
                    }
                    catch (Exception)
                    {
                        model.ServersUnavailable++;
                    }
                }
            }
            return model;
        }
    }
}