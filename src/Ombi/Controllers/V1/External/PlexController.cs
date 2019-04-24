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
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Models.External;

namespace Ombi.Controllers.V1.External
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public class PlexController : Controller
    {
        public PlexController(IPlexApi plexApi, ISettingsService<PlexSettings> plexSettings,
            ILogger<PlexController> logger, IPlexOAuthManager manager)
        {
            PlexApi = plexApi;
            PlexSettings = plexSettings;
            _log = logger;
            _plexOAuthManager = manager;
        }

        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private readonly ILogger<PlexController> _log;
        private readonly IPlexOAuthManager _plexOAuthManager;

        /// <summary>
        /// Signs into the Plex API.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<PlexAuthentication> SignIn([FromBody] UserRequest request)
        {
            try
            {
                // Do we already have settings?
                _log.LogDebug("OK, signing into Plex");
                var settings = await PlexSettings.GetSettingsAsync();
                if (!settings.Servers?.Any() ?? false) return null;

                _log.LogDebug("This is our first time, good to go!");

                var result = await PlexApi.SignIn(request);

                _log.LogDebug("Attempting to sign in to Plex.Tv");
                if (!string.IsNullOrEmpty(result.user?.authentication_token))
                {
                    _log.LogDebug("Sign in successful");
                    _log.LogDebug("Getting servers");
                    var server = await PlexApi.GetServer(result.user.authentication_token);
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

                _log.LogDebug("Finished");
                return result;
            }
            catch (Exception e)
            {
                _log.LogCritical(e, "Error when trying to sign into Plex.tv");
                throw;
            }
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


        [HttpGet("Libraries/{machineId}")]
        [PowerUser]
        public async Task<PlexLibrariesLiteResponse> GetPlexLibraries(string machineId)
        {
            try
            {
                var s = await PlexSettings.GetSettingsAsync();
                var settings = s.Servers.FirstOrDefault(x => x.MachineIdentifier == machineId);
                var libs = await PlexApi.GetLibrariesForMachineId(settings.PlexAuthToken, machineId);

                return new PlexLibrariesLiteResponse
                {
                    Successful = true,
                    Data = libs.Server.Section
                };
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Error thrown when attempting to obtain the plex libs");

                var message = e.InnerException != null ? $"{e.Message} - {e.InnerException.Message}" : e.Message;
                return new PlexLibrariesLiteResponse
                {
                    Successful = false,
                    Message = message
                };
            }
        }

        [HttpPost("user")]
        [PowerUser]
        public async Task<IActionResult> AddUser([FromBody] PlexUserViewModel user)
        {
            var s = await PlexSettings.GetSettingsAsync();
            var server = s.Servers.FirstOrDefault(x => x.MachineIdentifier == user.MachineIdentifier);
            var result = await PlexApi.AddUser(user.Username, user.MachineIdentifier, server.PlexAuthToken,
                user.LibsSelected);
            if (result.HasError)
            {
                return Json(new
                {
                    Success = false,
                    Error = result.Error.Status
                });
            }
            else
            {
                return Json(new
                {
                    Success = true
                });
            }
        }

        /// <summary>
        /// Gets the plex servers.
        /// </summary>
        /// <returns></returns>
        [HttpGet("servers")]
        [PowerUser]
        public async Task<IActionResult> GetServers()
        {
            try
            {
                var s = await PlexSettings.GetSettingsAsync();
                var servers =  new List<PlexServersAddUserModel>();
                foreach (var plexServer in s.Servers)
                {
                    servers.Add(new PlexServersAddUserModel
                    {
                        ServerId = plexServer.Id,
                        MachineId = plexServer.MachineIdentifier,
                        ServerName = plexServer.Name
                    });
                }

                return Json(new
                {
                    Success = true,
                    Servers = servers
                });
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Error thrown when attempting to obtain the GetServers for Add User VM");
                return Json(new PlexServersViewModel
                {
                    Success = false,
                });
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

        [HttpPost("oauth")]
        [AllowAnonymous]
        public async Task<IActionResult> OAuth([FromBody]PlexOAuthViewModel wizard)
        {
            //https://app.plex.tv/auth#?forwardUrl=http://google.com/&clientID=Ombi-Test&context%5Bdevice%5D%5Bproduct%5D=Ombi%20SSO&pinID=798798&code=4lgfd
            // Plex OAuth
            // Redirect them to Plex

            Uri url;
            if (!wizard.Wizard)
            {
                url = await _plexOAuthManager.GetOAuthUrl(wizard.Pin.code);
            }
            else
            {
                var websiteAddress =$"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                url = await _plexOAuthManager.GetWizardOAuthUrl(wizard.Pin.code, websiteAddress);
            }

            if (url == null)
            {
                return new JsonResult(new
                {
                    error = "Application URL has not been set"
                });
            }

            return new JsonResult(new {url = url.ToString()});
        }
    }
}
