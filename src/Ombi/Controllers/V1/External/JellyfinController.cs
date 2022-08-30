using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Jellyfin;
using Ombi.Api.Jellyfin.Models;
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
    public class JellyfinController : Controller
    {

        public JellyfinController(IJellyfinApiFactory jellyfin, ISettingsService<JellyfinSettings> jellyfinSettings)
        {
            JellyfinApi = jellyfin;
            JellyfinSettings = jellyfinSettings;
        }

        private IJellyfinApiFactory JellyfinApi { get; }
        private ISettingsService<JellyfinSettings> JellyfinSettings { get; }

        /// <summary>
        /// Signs into the Jellyfin Api
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<JellyfinSettings> SignIn([FromBody] JellyfinSettings request)
        {
            // Check if settings exist since we allow anon...
            var settings = await JellyfinSettings.GetSettingsAsync();
            if (settings?.Servers?.Any() ?? false) return null;

            var client = await JellyfinApi.CreateClient();
            request.Enable = true;
            var firstServer = request.Servers.FirstOrDefault();
            // Test that we can connect
            var result = await client.GetUsers(firstServer?.FullUri, firstServer?.ApiKey);

            if (result != null && result.Any())
            {
                firstServer.AdministratorId = result.FirstOrDefault(x => x.Policy.IsAdministrator)?.Id ?? string.Empty;
                await JellyfinSettings.SaveSettingsAsync(request);

                return request;
            }
            return null;
        }

        [HttpPost("info")]
        public async Task<PublicInfo> GetServerInfo([FromBody] JellyfinServers server)
        {
            var client = await JellyfinApi.CreateClient();
            var result = await client.GetPublicInformation(server.FullUri);
            return result;
        }

        [HttpPost("Library")]
        public async Task<JellyfinItemContainer<MediaFolders>> GetLibaries([FromBody] JellyfinServers server)
        {
            var client = await JellyfinApi.CreateClient();
            var result = await client.GetLibraries(server.ApiKey, server.FullUri);
            var mediaFolders = new JellyfinItemContainer<MediaFolders>
            {
                TotalRecordCount = result.Count,
                Items = new List<MediaFolders>()
            };

            foreach (var folder in result)
            {
                var toAdd = new MediaFolders
                {
                    Name = folder?.Name,
                    Id = folder?.ItemId,
                    ServerId = server.ServerId
                };

                var types = folder?.LibraryOptions?.TypeOptions?.Select(x => x.Type).ToList();

                if (!types.Any())
                {
                    continue;
                }

                if (types.Where(x => x.Equals("Movie", System.StringComparison.InvariantCultureIgnoreCase)
                                     || x.Equals("Episode", System.StringComparison.InvariantCultureIgnoreCase)).Count() >= 2)
                {
                    toAdd.CollectionType = "mixed";
                }
                else if (types.Any(x => x.Equals("Movie", StringComparison.InvariantCultureIgnoreCase)))
                {
                    toAdd.CollectionType = "movies";
                }
                else if (types.Any(x => x.Equals("Episode", StringComparison.InvariantCultureIgnoreCase)))
                {
                    toAdd.CollectionType = "tvshows";
                }

                mediaFolders.Items.Add(toAdd);
            }
            return mediaFolders;
        }

        /// <summary>
        /// Gets the jellyfin users.
        /// </summary>
        /// <returns></returns>
        [HttpGet("users")]
        public async Task<IEnumerable<UsersViewModel>> JellyfinUsers()
        {
            var vm = new List<UsersViewModel>();
            var s = await JellyfinSettings.GetSettingsAsync();
            var client = JellyfinApi.CreateClient(s);
            foreach (var server in s?.Servers ?? new List<JellyfinServers>())
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
