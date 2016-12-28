#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AboutModule.cs
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;
using NLog;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.UI.Models;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class AboutModule : BaseModule
    {
        public AboutModule(ISettingsService<PlexRequestSettings> settingsService,
            ISettingsService<SystemSettings> systemService, ISecurityExtensions security,
            IStatusChecker statusChecker) : base("admin", settingsService, security)
        {
            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            SettingsService = systemService;
            StatusChecker = statusChecker;
            
            Get["/about", true] = async (x,ct) => await Index();
            Post["/about", true] = async (x,ct) => await ReportIssue();
        }

        private ISettingsService<SystemSettings> SettingsService { get; }
        private IStatusChecker StatusChecker { get; }
        
        
        private async Task<Negotiator> Index()
        {
            var vm = new AboutAdminViewModel();

            var systemSettings = await SettingsService.GetSettingsAsync();

            var type = Type.GetType("Mono.Runtime");
            if (type != null) // mono
            {
                vm.Os = "Mono";
                var displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                {

                    vm.SystemVersion = displayName.Invoke(null, null).ToString();
                }
            }
            else
            {
                // Windows
                vm.Os = OperatingSystemHelper.GetOs();
                vm.SystemVersion = Environment.Version.ToString();
            }

            vm.ApplicationVersion = AssemblyHelper.GetFileVersion();
            vm.Branch = EnumHelper<Branches>.GetDisplayValue(systemSettings.Branch);
            vm.LogLevel = LogManager.Configuration.LoggingRules.FirstOrDefault(x => x.NameMatches("database"))?.Levels?.FirstOrDefault()?.Name ?? "Unknown";

            return View["About", vm];
        }

        private async Task<Response> ReportIssue()
        {
            var title = Request.Form["title"];
            var body = Request.Form["body"];

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(body))
            {
                return
                    Response.AsJson(
                        new
                        {
                            result = false,
                            message = "The title or issue body is empty! Please give me a bit more detail :)"
                        });
            }

            var result = await StatusChecker.ReportBug(title,body);
            return Response.AsJson(new {result = true, url = result.HtmlUrl.ToString()});
        }
    }
}