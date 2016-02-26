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
                }

                return View["/Admin/Settings", model];
            };

            Post["admin/"] = _ =>
            {
                var portString = (string)Request.Form.portNumber;
                int port;

                if (!int.TryParse(portString, out port))
                {
                    return Context.GetRedirect("~/admin?error=true");
                }

                var s = new SettingsService();
                s.SaveSettings(port);


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
                plex.GetToken(user.username, user.password);


                return Context.GetRedirect("~/admin");
            };

        }
    }
}