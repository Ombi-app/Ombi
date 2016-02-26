using System.Dynamic;

using Nancy;
using Nancy.Extensions;
using Nancy.Security;

using RequestPlex.Core;

namespace RequestPlex.UI.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule()
        {
            this.RequiresAuthentication();
            Get["admin/"] = _ =>
            {
                dynamic model = new ExpandoObject();
                model.Errored = Request.Query.error.HasValue;
                
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

        }
    }
}