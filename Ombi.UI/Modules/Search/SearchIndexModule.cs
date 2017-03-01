using Nancy.Responses.Negotiation;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.UI.Modules.Search
{
    public class SearchIndexModule : BaseAuthModule
    {
        protected ISettingsService<PlexSettings> PlexService { get; }
        protected ISettingsService<PlexRequestSettings> PrService { get; }
        protected ISettingsService<EmbySettings> EmbySettings { get; }
        protected ISettingsService<CustomizationSettings> CustomizationSettings { get; }

        public SearchIndexModule(ISettingsService<PlexRequestSettings> prSettings, ISettingsService<EmbySettings> embySettings,
            ISettingsService<PlexSettings> plexService, ISecurityExtensions security,
            ISettingsService<CustomizationSettings> cus) : base("search", prSettings, security)
        {
            PlexService = plexService;
            PrService = prSettings;
            EmbySettings = embySettings;
            CustomizationSettings = cus;

            Get["SearchIndex", "/", true] = async (x, ct) => await RequestLoad();
        }
        private async Task<Negotiator> RequestLoad()
        {
            var settings = await PrService.GetSettingsAsync();
            var custom = await CustomizationSettings.GetSettingsAsync();
            var emby = await EmbySettings.GetSettingsAsync();
            var plex = await PlexService.GetSettingsAsync();
            var searchViewModel = new SearchLoadViewModel
            {
                Settings = settings,
                CustomizationSettings = custom,
                Emby = emby.Enable,
                Plex = plex.Enable
            };

            return View["Search/Index", searchViewModel];
        }

    }
}
