using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Emby;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;

namespace Ombi.Controllers.External
{
    /// <summary>
    /// 
    /// </summary>
    [Admin]
    [ApiV1]
    public class EmbyController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emby"></param>
        /// <param name="embySettings"></param>
        public EmbyController(IEmbyApi emby, ISettingsService<EmbySettings> embySettings)
        {
            EmbyApi = emby;
            EmbySettings = embySettings;
        }

        private IEmbyApi EmbyApi { get; }
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

            request.Enable = true;
            var firstServer = request.Servers.FirstOrDefault();
            // Test that we can connect
            var result = await EmbyApi.GetUsers(firstServer.FullUri, firstServer.ApiKey);

            if (result != null && result.Any())
            {
                firstServer.AdministratorId = result.FirstOrDefault(x => x.Policy.IsAdministrator)?.Id ?? string.Empty;
                await EmbySettings.SaveSettingsAsync(request);

                return request;
            }
            return null;
        }
    }
}
