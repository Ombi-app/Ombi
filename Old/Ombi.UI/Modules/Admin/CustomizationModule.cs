#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomizationModule.cs
//    Created By: Jamie Rees
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.UI.Models;
using Ombi.UI.Models.Admin;
using Ombi.UI.Models.UI;
using TMDbLib.Utilities;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class CustomizationModule : BaseModule
    {
        public CustomizationModule(ISettingsService<PlexRequestSettings> settingsService,
            ISettingsService<CustomizationSettings> cust, ISecurityExtensions security)
            : base("admin", settingsService, security)
        {
            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            Settings = cust;

            Get["/customization", true] = async (x, ct) => await Index();
            Post["/customization", true] = async (x, ct) => await Save();
        }

        private ISettingsService<CustomizationSettings> Settings { get; }

        private async Task<Negotiator> Index()
        {
            var model = await Settings.GetSettingsAsync();

            var viewModel = new CustomizationViewModel
            {
                Settings = model,
                SortOptions = new List<Dropdown<SortOptions>>()
            };




            foreach (var value in EnumHelper<SortOptions>.GetValues(SortOptions.LatestRelease))
            {
                viewModel.SortOptions.Add(new Dropdown<SortOptions>
                {
                    Value = value,
                    Name = EnumHelper<SortOptions>.GetDisplayDescription(value),
                    Selected = model.DefaultSort == (int) value
                });
            }

            foreach (var value in EnumHelper<FilterOptions>.GetValues(FilterOptions.Available))
            {
                viewModel.FilterOptions.Add(new Dropdown<FilterOptions>
                {
                    Value = value,
                    Name = EnumHelper<FilterOptions>.GetDisplayDescription(value),
                    Selected = model.DefaultFilter == (int) value
                });
            }

            foreach (var value in EnumHelper<Languages>.GetValues(Languages.en))
            {
                viewModel.LanguageDropdown.Add(new Dropdown<Languages>
                {
                    Value = value,
                    Name = EnumHelper<Languages>.GetDisplayValue(value),
                    Selected = model.DefaultLang == (int) value
                });
            }

            return View["customization", viewModel];
        }

        private async Task<Response> Save()
        {
            try
            {

            var model = this.Bind<CustomizationSettings>();

            var result = await Settings.SaveSettingsAsync(model);

            return Response.AsJson(result
               ? new JsonResponseModel { Result = true }
               : new JsonResponseModel { Result = false, Message = "We could not save to the database, please try again" });
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}