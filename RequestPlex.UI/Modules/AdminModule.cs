#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AdminModule.cs
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
using System.Dynamic;

using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Security;

using RequestPlex.Api;
using RequestPlex.Core;
using RequestPlex.Core.SettingModels;
using RequestPlex.UI.Models;

namespace RequestPlex.UI.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule(ISettingsService<RequestPlexSettings> service)
        {
#if !DEBUG
            this.RequiresAuthentication();
#endif
            Get["admin/"] = _ =>
            {
                dynamic model = new ExpandoObject();
                model.Errored = Request.Query.error.HasValue;
                model.Port = null;

                var settings = service.GetSettings();
                if (settings != null)
                {
                    model.Port = settings.Port;
                    model.PlexAuthToken = settings.PlexAuthToken;
                }

                return View["/Admin/Settings", model];
            };

            Post["admin/"] = _ =>
            {
                var model = this.Bind<RequestPlexSettings>();

                service.SaveSettings(model);


                return Context.GetRedirect("~/admin");
            };

            Post["admin/requestauth"] = _ =>
            {
                var user = this.Bind<PlexAuth>();

                if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.password))
                {
                    return Context.GetRedirect("~/admin?error=true");
                }

                var plex = new PlexApi();
                var model = plex.GetToken(user.username, user.password);
                var oldSettings = service.GetSettings();
                if (oldSettings != null)
                {
                    oldSettings.PlexAuthToken = model.user.authentication_token;
                    service.SaveSettings(oldSettings);
                }
                else
                {
                    var newModel = new RequestPlexSettings
                    {
                        PlexAuthToken = model.user.authentication_token
                    };
                    service.SaveSettings(newModel);
                }



                return Context.GetRedirect("~/admin");
            };

            Get["admin/getusers"] = _ =>
            {
                var api = new PlexApi();
                

                return View["/Admin/Settings"];
            };

        }
    }
}