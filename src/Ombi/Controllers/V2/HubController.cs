using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Engine.V2;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Ombi.Core;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Models;

namespace Ombi.Controllers.V2
{
    [ApiV2]
    [Authorize]
    [ApiController]
    public class HubController : ControllerBase
    {
        public HubController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        private readonly IHubContext<NotificationHub> _hub;

        /// <summary>
        /// Returns search results for both TV and Movies
        /// </summary>
        /// <returns></returns>
        [HttpGet("{searchTerm}")]
        public async Task MultiSearch(string searchTerm)
        {
            await _hub.Clients.All.SendAsync("Notification", searchTerm);
        }

        /// <summary>
        /// Returns search results for both TV and Movies
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin/{searchTerm}")]
        public async Task Admin(string searchTerm)
        {
            var admins = NotificationHub.UsersOnline.Where(x => x.Value.Roles.Contains(OmbiRoles.Admin)).Select(x => x.Key).ToList();
            await _hub.Clients.Clients(admins).SendAsync("Notification", searchTerm);
        }
    }
}