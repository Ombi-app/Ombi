using System;
using System.Threading.Tasks;

using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class DonationLinkModule : BaseAuthModule
    {
        public DonationLinkModule(ICacheProvider provider, ISettingsService<PlexRequestSettings> pr) : base("customDonation", pr)
        {
            Cache = provider;

            Get["/", true] = async (x, ct) => await GetCustomDonationUrl(pr);
        }

        private ICacheProvider Cache { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        private async Task<Response> GetCustomDonationUrl(ISettingsService<PlexRequestSettings> pr)
        {
            PlexRequestSettings settings = pr.GetSettings();
            try
            {
                if (settings.EnableCustomDonationUrl)
                {
                    JObject o = new JObject();
                    o["url"] = "\""+ settings.CustomDonationUrl + "\"";
                    o["message"] = "\"" + settings.CustomDonationMessage + "\"";
                    return Response.AsJson(o);
                }
                else
                {
                    JObject o = new JObject();
                    o["url"] = "https://www.paypal.me/PlexRequestsNet";
                    return Response.AsJson(o);
                }
            }
            catch (Exception e)
            {
                Log.Warn("Exception Thrown when attempting to check the custom donation url");
                Log.Warn(e);
                JObject o = new JObject();
                o["url"] = "https://www.paypal.me/PlexRequestsNet";
                o["message"] = "\"" + "Donate to Library Maintainer" + "\"";
                return Response.AsJson(o);
            }
        }
    }
    
}
