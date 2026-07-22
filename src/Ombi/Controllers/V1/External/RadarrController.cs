using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Api.External.ExternalApis.Radarr;
using Ombi.Api.External.ExternalApis.Radarr.Models;
using Ombi.Api.External.ExternalApis.Radarr.Models.V3;
using Ombi.Attributes;
using Ombi.Core.Authentication;
using Ombi.Core.Helpers;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Controllers.V1.External
{
    [Authorize]
    [ApiV1]
    [ApiController]
    [Produces("application/json")]
    public class RadarrController : ControllerBase
    {

        public RadarrController(
            ISettingsService<RadarrSettings> settings,
            ISettingsService<Radarr4KSettings> radarr4kSettings,
            IRadarrV3Api radarrV3Api,
            ICurrentUser currentUser,
            OmbiUserManager userManager,
            IRepository<UserSelectableQualityProfile> selectableProfiles)
        {
            _radarrSettings = settings;
            _radarr4KSettings = radarr4kSettings;
            _radarrV3Api = radarrV3Api;
            _currentUser = currentUser;
            _userManager = userManager;
            _selectableProfiles = selectableProfiles;
        }

        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<Radarr4KSettings> _radarr4KSettings;
        private readonly IRadarrV3Api _radarrV3Api;
        private readonly ICurrentUser _currentUser;
        private readonly OmbiUserManager _userManager;
        private readonly IRepository<UserSelectableQualityProfile> _selectableProfiles;
        /// <summary>
        /// Gets the Radarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Profiles")]
        [PowerUser]
        public async Task<IActionResult> GetProfiles([FromBody] RadarrSettings settings)
        {
            return Ok(await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri));
        }

        [HttpGet("enabled")]
        [PowerUser]
        public async Task<bool> Enabled()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            return settings.Enabled;
        }

        /// <summary>
        /// Gets the Radarr root folders.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("RootFolders")]
        [PowerUser]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders([FromBody] RadarrSettings settings)
        {
            return await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Radarr profiles using the saved settings
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles")]
        [PowerUser]
        public async Task<IActionResult> GetProfiles()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return Ok(await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri));
            }
            return Ok(new List<RadarrV3QualityProfile>());
        }

        /// <summary>
        /// Gets the Radarr 4K profiles using the saved settings
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles/4k")]
        [PowerUser]
        public async Task<IActionResult> GetProfiles4K()
        {
            var settings = await _radarr4KSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return Ok(await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri));
            }
            return Ok(new List<RadarrV3QualityProfile>());
        }

        [HttpGet("Profiles/selectable")]
        [Authorize(Roles = OmbiRoles.Admin + "," + OmbiRoles.PowerUser + "," + OmbiRoles.SelectRadarrQualityProfile)]
        public async Task<IActionResult> GetSelectableProfiles()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            var profiles = settings.Enabled
                ? (await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri))
                    .Select(x => new RadarrQualityProfileModel { Id = x.id, Name = x.name })
                : new List<RadarrQualityProfileModel>();
            return Ok(await FilterSelectableProfiles(profiles, false));
        }

        [HttpGet("Profiles/4k/selectable")]
        [Authorize(Roles = OmbiRoles.Admin + "," + OmbiRoles.PowerUser + "," + OmbiRoles.SelectRadarrQualityProfile)]
        public async Task<IActionResult> GetSelectableProfiles4K()
        {
            var settings = await _radarr4KSettings.GetSettingsAsync();
            var profiles = settings.Enabled
                ? (await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri))
                    .Select(x => new RadarrQualityProfileModel { Id = x.id, Name = x.name })
                : new List<RadarrQualityProfileModel>();
            return Ok(await FilterSelectableProfiles(profiles, true));
        }

        private async Task<IEnumerable<RadarrQualityProfileModel>> FilterSelectableProfiles(IEnumerable<RadarrQualityProfileModel> profiles, bool is4K)
        {
            var user = await _currentUser.GetUser();
            if (await _userManager.IsInRoleAsync(user, OmbiRoles.Admin) || await _userManager.IsInRoleAsync(user, OmbiRoles.PowerUser))
            {
                return profiles;
            }

            var allowed = await _selectableProfiles.GetAll()
                .Where(x => x.UserId == user.Id && x.Application == SelectableQualityProfileApplication.Radarr && x.Is4K == is4K)
                .Select(x => x.QualityProfileId)
                .ToListAsync();
            return profiles.Where(x => allowed.Contains(x.Id));
        }

        /// <summary>
        /// Gets the Radarr root folders using the saved settings.
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("RootFolders")]
        [PowerUser]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
            }
            return null;
        }

        /// <summary>
        /// Gets the Radarr 4K root folders using the saved settings.
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("RootFolders/4k")]
        [PowerUser]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders4K()
        {
            var settings = await _radarr4KSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
            }
            return null;
        }

        /// <summary>
        /// Gets the Radarr tags
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags([FromBody] SonarrSettings settings)
        {
            return await _radarrV3Api.GetTags(settings.ApiKey, settings.FullUri);
        }


        /// <summary>
        /// Gets the Radarr tags
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await _radarrV3Api.GetTags(settings.ApiKey, settings.FullUri);
            }

            return null;
        }
    }

    public class RadarrQualityProfileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}