#region Copyright

//  ************************************************************************
//    Copyright (c) 2016-2017 Ombi
//    File: SearchIndexModule.cs
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/

#endregion

using Nancy.Responses.Negotiation;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.UI.Models;
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
