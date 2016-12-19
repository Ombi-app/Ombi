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

using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;
using NLog;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers.Permissions;
using Ombi.UI.Helpers;
using Ombi.UI.Models;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class UserManagementSettingsModule : BaseModule
    {
        public UserManagementSettingsModule(ISettingsService<PlexRequestSettings> settingsService, ISettingsService<UserManagementSettings> umSettings, ISecurityExtensions security) : base("admin", settingsService, security)
        {
            UserManagementSettings = umSettings;

            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            Get["UserManagementSettings","/usermanagementsettings", true] = async(x,ct) => await Index();
            Post["/usermanagementsettings", true] = async(x,ct) => await Update();
        }
        
        private ISettingsService<UserManagementSettings> UserManagementSettings { get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private async Task<Negotiator> Index()
        {
            var model = await UserManagementSettings.GetSettingsAsync();

            return View["UserManagementSettings", model];
        }


        private async Task<Response> Update()
        {
            var settings = this.Bind<UserManagementSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                var error = valid.SendJsonError();
                Log.Info("Error validating User Management settings, message: {0}", error.Message);
                return Response.AsJson(error);
            }

            var result = await UserManagementSettings.SaveSettingsAsync(settings);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for User Management!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }
    }
}