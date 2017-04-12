using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models;

namespace Ombi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : BaseV1ApiController
    {
        public SettingsController(ISettingsResolver resolver)
        {
            SettingsResolver = resolver;
        }

        private ISettingsResolver SettingsResolver { get; }

        [HttpGet("ombi")]
        public async Task<OmbiSettings> OmbiSettings()
        {
            var settings = SettingsResolver.Resolve<OmbiSettings>();

            return await settings.GetSettingsAsync();
        }

        [HttpPost("ombi")]
        public async Task<bool> OmbiSettings([FromBody]OmbiSettings ombi)
        {
            var settings = SettingsResolver.Resolve<OmbiSettings>();

            return await settings.SaveSettingsAsync(ombi);
        }


    }
}
