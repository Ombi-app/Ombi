using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class CustomPageController : ControllerBase
    {
        public CustomPageController(ISettingsService<CustomPageSettings> settings)
        {
            _settings = settings;
        }

        private readonly ISettingsService<CustomPageSettings> _settings;

        /// <summary>
        /// Gets the Custom Page Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<CustomPageSettings> CustomPageSettings()
        {
            return await Get();
        }

        /// <summary>
        /// Saves the Custom Page Settings.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = OmbiRoles.EditCustomPage)]
        public async Task<bool> CustomPageSettings([FromBody] CustomPageSettings page)
        {
            return await Save(page);
        }
        private async Task<CustomPageSettings> Get()
        {
            return await _settings.GetSettingsAsync();
        }

        private async Task<bool> Save(CustomPageSettings settingsModel)
        {
            return await _settings.SaveSettingsAsync(settingsModel);
        }
    }
}