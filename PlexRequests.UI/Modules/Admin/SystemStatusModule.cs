#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SystemStatusModule.cs
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
using System.Diagnostics;
using System.Threading.Tasks;
using MarkdownSharp;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Core.StatusChecker;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Permissions;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules.Admin
{
    public class SystemStatusModule : BaseModule
    {
        public SystemStatusModule(ISettingsService<PlexRequestSettings> settingsService, ICacheProvider cache, ISettingsService<SystemSettings> ss, ISecurityExtensions security) : base("admin", settingsService, security)
        {
            Cache = cache;
            SystemSettings = ss;

            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            Get["/status", true] = async (x, ct) => await Status();
            Post["/save", true] = async (x, ct) => await Save();

            Post["/autoupdate"] = x => AutoUpdate();
        }

        private ICacheProvider Cache { get; }
        private ISettingsService<SystemSettings> SystemSettings { get; }

        private async Task<Negotiator> Status()
        {
            var settings = await SystemSettings.GetSettingsAsync();
            var checker = new StatusChecker(SystemSettings);
            var status = await Cache.GetOrSetAsync(CacheKeys.LastestProductVersion, async () => await checker.GetStatus(), 30);
            var md = new Markdown(new MarkdownOptions { AutoNewLines = true, AutoHyperlink = true });
            status.ReleaseNotes = md.Transform(status.ReleaseNotes);

            settings.Status = status;

            settings.BranchDropdown = new List<BranchDropdown>
            {
                new BranchDropdown
                {
                    Name = EnumHelper<Branches>.GetDisplayValue(Branches.Stable),
                    Value = Branches.Stable,
                    Selected = settings.Branch == Branches.Stable
                },
                new BranchDropdown
                {
                    Name = EnumHelper<Branches>.GetDisplayValue(Branches.EarlyAccessPreview),
                    Value = Branches.EarlyAccessPreview,
                    Selected = settings.Branch == Branches.EarlyAccessPreview
                },
                new BranchDropdown
                {
                    Name = EnumHelper<Branches>.GetDisplayValue(Branches.Dev),
                    Value = Branches.Dev,
                    Selected = settings.Branch == Branches.Dev
                },
            };

            return View["Status", settings];
        }

        private async Task<Response> Save()
        {
            var settings = this.Bind<SystemSettings>();

            await SystemSettings.SaveSettingsAsync(settings);

            // Clear the cache
            Cache.Remove(CacheKeys.LastestProductVersion);
            
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully Saved your settings"});
        }

        private Response AutoUpdate()
        {
            var url = Request.Form["url"];

            var startInfo = Type.GetType("Mono.Runtime") != null
                                             ? new ProcessStartInfo("mono PlexRequests.Updater.exe") { Arguments = url }
                                             : new ProcessStartInfo("PlexRequests.Updater.exe") { Arguments = url };

            Process.Start(startInfo);

            Environment.Exit(0);
            return Nancy.Response.NoBody;
        }

    }
}