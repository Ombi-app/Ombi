using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using RequestPlex.Api;
using RequestPlex.Core;
using RequestPlex.Store;
using RequestPlex.UI.Models;

namespace RequestPlex.UI.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule()
        {
#if !DEBUG
            this.RequiresAuthentication();
#endif
            Get["admin/"] = _ =>
            {
                dynamic model = new ExpandoObject();
                model.Errored = Request.Query.error.HasValue;
                model.Port = null;
                var s = new SettingsService();
                var settings = s.GetSettings();
                if (settings != null)
                {
                    model.Port = settings.Port;
                    model.PlexAuthToken = settings.PlexAuthToken;
                }

                return View["/Admin/Settings", model];
            };

            Post["admin/"] = _ =>
            {
                var model = this.Bind<SettingsModel>();

                var s = new SettingsService();
                s.SaveSettings(model);


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
                var s = new SettingsService();
                var oldSettings = s.GetSettings();
                if (oldSettings != null)
                {
                    oldSettings.PlexAuthToken = model.user.authentication_token;
                    s.SaveSettings(oldSettings);
                }
                else
                {
                    var newModel = new SettingsModel
                    {
                        PlexAuthToken = model.user.authentication_token
                    };
                    s.SaveSettings(newModel);
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