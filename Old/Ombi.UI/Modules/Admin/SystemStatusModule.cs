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
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MarkdownSharp;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Ombi.Api.Interfaces;
using Ombi.Common.Processes;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Core.StatusChecker;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.UI.Models;
using Action = Ombi.Helpers.Analytics.Action;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class SystemStatusModule : BaseModule
    {
        public SystemStatusModule(ISettingsService<PlexRequestSettings> settingsService, ICacheProvider cache, ISettingsService<SystemSettings> ss,
            ISecurityExtensions security, IAnalytics a, IAppveyorApi appveyor) : base("admin", settingsService, security)
        {
            Cache = cache;
            SystemSettings = ss;
            Analytics = a;
            AppveyorApi = appveyor;

            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            Get["/status", true] = async (x, ct) => await Status();
            Post["/save", true] = async (x, ct) => await Save();

            Post["/autoupdate"] = x => AutoUpdate();
            Get["/changes", true] = async (x, ct) => await GetLatestChanges();
        }

        private ICacheProvider Cache { get; }
        private ISettingsService<SystemSettings> SystemSettings { get; }
        private IAnalytics Analytics { get; }
        private IAppveyorApi AppveyorApi { get; }

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
                    Name =EnumHelper<Branches>.GetBranchValue<BranchAttribute>(Branches.Stable).DisplayName,
                    Value = Branches.Stable,
                    Selected = settings.Branch == Branches.Stable
                },
                new BranchDropdown
                {
                    Name = EnumHelper<Branches>.GetBranchValue<BranchAttribute>(Branches.EarlyAccessPreview).DisplayName,
                    Value = Branches.EarlyAccessPreview,
                    Selected = settings.Branch == Branches.EarlyAccessPreview
                },
                new BranchDropdown
                {
                    Name = EnumHelper<Branches>.GetBranchValue<BranchAttribute>(Branches.Dev).DisplayName,
                    Value = Branches.Dev,
                    Selected = settings.Branch == Branches.Dev
                },
            };

            return View["Status", settings];
        }

        public async Task<Response> GetLatestChanges()
        {
            var settings = await SystemSettings.GetSettingsAsync();
            var branchName = EnumHelper<Branches>.GetBranchValue<BranchAttribute>(settings.Branch).BranchName;
            var changes = AppveyorApi.GetProjectHistory(branchName);
            var currentVersion =  AssemblyHelper.GetProductVersion();
            var model = new List<RecentUpdatesModel>();

            foreach (var build in changes.builds)
            {
                model.Add(new RecentUpdatesModel
                {
                    Date = build.finished,
                    Message = BuildAppveyorMessage(build.message, build.messageExtended),
                    Version = build.version,
                    Installed = currentVersion.Equals(build.version, StringComparison.CurrentCultureIgnoreCase) ,
                    Branch = branchName
                });
            }

            return Response.AsJson(model);
        }

        private string BuildAppveyorMessage(string message, string extended)
        {
            return extended == null ? message : $"{message} {extended}";
        }

        private async Task<Response> Save()
        {

            var settings = this.Bind<SystemSettings>();

            Analytics.TrackEventAsync(Category.Admin, Action.Update, $"Updated Branch {EnumHelper<Branches>.GetBranchValue<BranchAttribute>(settings.Branch).DisplayName}", Username, CookieHelper.GetAnalyticClientId(Cookies));
            await SystemSettings.SaveSettingsAsync(settings);

            // Clear the cache
            Cache.Remove(CacheKeys.LastestProductVersion);
            
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully Saved your settings"});
        }

        private Response AutoUpdate()
        {
            Analytics.TrackEventAsync(Category.Admin, Action.Update, "AutoUpdate", Username, CookieHelper.GetAnalyticClientId(Cookies));

            var url = Request.Form["url"];
            var args = (string)Request.Form["args"].ToString();
            var lowered = args.ToLower();
            var appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SystemStatusModule)).Location ?? string.Empty) ?? string.Empty, Path.Combine("UpdateService", "Ombi.Updater.exe"));

            if (!string.IsNullOrEmpty(lowered))
            {
                if (lowered.Contains("ombi.exe"))
                {
                    lowered = lowered.Replace("ombi.exe", "");
                }
            }

            var startArgs = string.IsNullOrEmpty(lowered) || lowered == "Nancy.DynamicDictionaryValue".ToLower() ? appPath : $"{lowered} Ombi.Updater.exe";

            var startInfo = Type.GetType("Mono.Runtime") != null
                                             ? new ProcessStartInfo(startArgs) { Arguments = $"{url} {lowered}", }
                                             : new ProcessStartInfo(startArgs) { Arguments = $"{url} {lowered}" };

            Process.Start(startInfo);

            //Environment.Exit(0);
            return Nancy.Response.NoBody;
        }

    }
}