using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
using Ombi.Api.Plex;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Models.External;

namespace Ombi.Controllers.V1.External
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public class EmbyController : Controller
    {

        public EmbyController(IEmbyApiFactory emby, ISettingsService<EmbySettings> embySettings)
        {
            EmbyApi = emby;
            EmbySettings = embySettings;
        }

        private IEmbyApiFactory EmbyApi { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }

        /// <summary>
        /// Signs into the Emby Api
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<EmbySettings> SignIn([FromBody] EmbySettings request)
        {
            // Check if settings exist since we allow anon...
            var settings = await EmbySettings.GetSettingsAsync();
            if (settings?.Servers?.Any() ?? false) return null;

            var client = await EmbyApi.CreateClient();
            request.Enable = true;
            var firstServer = request.Servers.FirstOrDefault();
            // Test that we can connect
            var result = await client.GetUsers(firstServer.FullUri, firstServer.ApiKey);

            if (result != null && result.Any())
            {
                firstServer.AdministratorId = result.FirstOrDefault(x => x.Policy.IsAdministrator)?.Id ?? string.Empty;
                await EmbySettings.SaveSettingsAsync(request);

                return request;
            }
            return null;
        }

        [HttpPost("info")]
        public async Task<PublicInfo> GetServerInfo([FromBody] EmbyServers server)
        {
            var client = await EmbyApi.CreateClient();
            var result = await client.GetPublicInformation(server.FullUri);
            return result;
        }

        /// <summary>
        /// Gets the emby users.
        /// </summary>
        /// <returns></returns>
        [HttpGet("users")]
        public async Task<IEnumerable<UsersViewModel>> EmbyUsers()
        {
            var vm = new List<UsersViewModel>();
            var s = await EmbySettings.GetSettingsAsync();
            var client = EmbyApi.CreateClient(s);
            foreach (var server in s?.Servers ?? new List<EmbyServers>())
            {
                var users = await client.GetUsers(server.FullUri, server.ApiKey);
                if (users != null && users.Any())
                {
                    vm.AddRange(users.Select(u => new UsersViewModel
                    {
                        Username = u.Name,
                        Id = u.Id
                    }));
                }
            }

            // Filter out any dupes
            return vm.DistinctBy(x => x.Id);
        }
    }
}
