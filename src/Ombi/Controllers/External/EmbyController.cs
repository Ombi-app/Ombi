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
    [Admin]
    public class EmbyController : BaseV1ApiController
    {
        public EmbyController(IEmbyApi emby, ISettingsService<EmbySettings> embySettings)
        {
            EmbyApi = emby;
            EmbySettings = embySettings;
        }

        private IEmbyApi EmbyApi { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }

        [HttpPost]
        [AllowAnonymous]
        public async Task<EmbySettings> SignIn([FromBody] EmbySettings request)
        {
            // Check if settings exist
            var settings = await EmbySettings.GetSettingsAsync();
            if (settings != null && !string.IsNullOrEmpty(settings.ApiKey)) return null;

            request.Enable = true;
            // Test that we can connect
            var result = await EmbyApi.GetUsers(request.FullUri, request.ApiKey);

            if (result != null && result.Any())
            {
                request.AdministratorId = result.FirstOrDefault(x => x.Policy.IsAdministrator)?.Id ?? string.Empty;
                await EmbySettings.SaveSettingsAsync(request);

                return request;
            }
            return null;
        }
    }
}
