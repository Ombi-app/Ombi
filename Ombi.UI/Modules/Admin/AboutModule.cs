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
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Linker;
using Nancy.Responses.Negotiation;
using NLog;
using Octokit;
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
            IStatusChecker statusChecker, IResourceLinker linker) : base("admin", settingsService, security)
        {
            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            SettingsService = systemService;
            StatusChecker = statusChecker;
            Linker = linker;
            
            Get["AboutPage","/about", true] = async (x,ct) => await Index();
            Post["/about", true] = async (x,ct) => await ReportIssue();

            Get["/OAuth", true] = async (x, ct) => await OAuth();
            Get["/authorize", true] = async (x, ct) => await Authorize();
        }

        private ISettingsService<SystemSettings> SettingsService { get; }
        private IStatusChecker StatusChecker { get; }
        private IResourceLinker Linker { get; }
        
        
        private async Task<Negotiator> Index()
        {
            var vm = await GetModel();
            return View["About", vm];
        }

        private async Task<AboutAdminViewModel> GetModel()
        {
            var vm = new AboutAdminViewModel();
            var oAuth = Session[SessionKeys.OAuthToken]?.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(oAuth))
            {
                vm.OAuthEnabled = true;
            }

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

            return vm;
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

            var model = await GetModel();
            body = CreateReportBody(model, body);
            var token = Session[SessionKeys.OAuthToken].ToString();
            var result = await StatusChecker.ReportBug(title, body, token);
            return Response.AsJson(new {result = true, url = result.HtmlUrl.ToString()});
        }

        private async Task<Response> OAuth()
        {
            var path = Request.Url.Path;

            Request.Url.Path = path.Replace("oauth", "authorize");
            var uri = await StatusChecker.OAuth(Request.Url.ToString(), Session);

            return Response.AsJson(new { uri = uri.ToString()});
        }

        public async Task<Response> Authorize()
        {
            var code = Request.Query["code"].ToString();
            var state = Request.Query["state"].ToString();

            var expectedState = Session[SessionKeys.CSRF] as string;
            if (state != expectedState)
            {
                throw new InvalidOperationException("SECURITY FAIL!");
            }
            Session[SessionKeys.CSRF] = null;

            var token = await StatusChecker.OAuthAccessToken(code);
            Session[SessionKeys.OAuthToken] = token.AccessToken;

            return Context.GetRedirect(Linker.BuildRelativeUri(Context, "AboutPage").ToString());
        }

        private string CreateReportBody(AboutAdminViewModel model, string body)
        {
            var sb = new StringBuilder();

            sb.AppendLine("#### Ombi Version");
            sb.AppendLine($"V {model.ApplicationVersion}");
            sb.AppendLine("#### Update Branch:");
            sb.AppendLine(model.Branch);
            sb.AppendLine("#### Operating System:");
            sb.AppendLine(model.Os);
            sb.AppendLine(body);

            return sb.ToString();

            //            <!--- //!! Please use the Support / bug report template, otherwise we will close the Github issue !! 
            //(Pleas submit a feature request over here: http://feathub.com/tidusjar/Ombi) //--->

            //#### Ombi Version:

            //V 1.XX.XX

            //#### Update Branch:

            //Stable/Early Access Preview/development

            //#### Operating System:

            //(Place text here)

            //#### Mono Version (only if your not on windows)

            //(Place text here)

            //#### Applicable Logs (from `/logs/` directory or the Admin page):

            //```

            //(Logs go here. Don't remove the ``` tags for showing your logs correctly. Please make sure you remove any personal information from the logs)

            //```

            //#### Problem Description:

            //(Place text here)

            //#### Reproduction Steps:

            //Please include any steps to reproduce the issue, this the request that is causing the problem etc.

        }
    }
}