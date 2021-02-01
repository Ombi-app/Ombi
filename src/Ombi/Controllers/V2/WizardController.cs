using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Models.V2;
using Ombi.Settings.Settings.Models;
using System.Threading.Tasks;

namespace Ombi.Controllers.V2
{
    [ServiceFilter(typeof(WizardActionFilter))]
    [AllowAnonymous]
    public class WizardController : V2Controller
    {
        private ISettingsService<CustomizationSettings> _customizationSettings { get; }

        public WizardController(ISettingsService<CustomizationSettings> customizationSettings)
        {
            _customizationSettings = customizationSettings;
        }

        [HttpPost("config")]
        [ApiExplorerSettings(IgnoreApi =true)]
        public async Task<IActionResult> OmbiConfig([FromBody] OmbiConfigModel config)
        {
            if (config == null)
            {
                return BadRequest();
            }

            var settings = await _customizationSettings.GetSettingsAsync();

            if (config.ApplicationName.HasValue())
            {
                settings.ApplicationName = config.ApplicationName;
            }

            if(config.ApplicationUrl.HasValue())
            {
                settings.ApplicationUrl = config.ApplicationUrl;
            }

            if(config.Logo.HasValue())
            {
                settings.Logo = config.Logo;
            }

            await _customizationSettings.SaveSettingsAsync(settings);

            return new OkObjectResult(settings);
        }
    }
}
