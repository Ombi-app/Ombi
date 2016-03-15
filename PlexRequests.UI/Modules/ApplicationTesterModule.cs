#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApplicationTesterModule.cs
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

using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class ApplicationTesterModule : BaseModule
    {

        public ApplicationTesterModule(ICouchPotatoApi cpApi, ISonarrApi sonarrApi, IPlexApi plexApi,
            ISettingsService<AuthenticationSettings> authSettings) : base("test")
        {
            this.RequiresAuthentication();
            
            CpApi = cpApi;
            SonarrApi = sonarrApi;
            PlexApi = plexApi;
            AuthSettings = authSettings;

            Post["/cp"] = _ => CouchPotatoTest();
            Post["/sonarr"] = _ => SonarrTest();
            Post["/plex"] = _ => PlexTest();

        }
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ISonarrApi SonarrApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }

        private Response CouchPotatoTest()
        {
            var couchPotatoSettings = this.Bind<CouchPotatoSettings>();
            try
            {
                var status = CpApi.GetStatus(couchPotatoSettings.FullUri, couchPotatoSettings.ApiKey);
                return status.success
               ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Connected to CouchPotato successfully!" })
               : Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to CouchPotato, please check your settings." });

            }
            catch (ApplicationException e) // Exceptions are expected if we cannot connect so we will just log and swallow them.
            {
                Log.Warn("Exception thrown when attempting to get CP's status: ");
                Log.Warn(e);
                var message = $"Could not connect to CouchPotato, please check your settings. <strong>Exception Message:</strong> {e.Message}";
                if (e.InnerException != null)
                {
                    message = $"Could not connect to CouchPotato, please check your settings. <strong>Exception Message:</strong> {e.InnerException.Message}";
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = message });
            }
        }

        private Response SonarrTest()
        {
            var sonarrSettings = this.Bind<SonarrSettings>();
            try
            {
                var status = SonarrApi.SystemStatus(sonarrSettings.ApiKey, sonarrSettings.FullUri);
                return status != null
               ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Connected to Sonarr successfully!" })
               : Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to Sonarr, please check your settings." });

            }
            catch (ApplicationException e) // Exceptions are expected if we cannot connect so we will just log and swallow them.
            {
                Log.Warn("Exception thrown when attempting to get Sonarr's status: ");
                Log.Warn(e);
                var message = $"Could not connect to Sonarr, please check your settings. <strong>Exception Message:</strong> {e.Message}";
                if (e.InnerException != null)
                {
                    message = $"Could not connect to Sonarr, please check your settings. <strong>Exception Message:</strong> {e.InnerException.Message}";
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = message });
            }
        }

        private Response PlexTest()
        {
            var plexSettings = this.Bind<PlexSettings>();
            var settings = AuthSettings.GetSettings();
            if (settings?.PlexAuthToken == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Plex is not setup yet, you need to update your Authentication settings" });
            }
            try
            {
                var status = PlexApi.GetStatus(settings.PlexAuthToken, plexSettings.FullUri);
                return status != null
               ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Connected to Plex successfully!" })
               : Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to Plex, please check your settings." });

            }
            catch (ApplicationException e) // Exceptions are expected if we cannot connect so we will just log and swallow them.
            {
                Log.Warn("Exception thrown when attempting to get Plex's status: ");
                Log.Warn(e);
                var message = $"Could not connect to Plex, please check your settings. <strong>Exception Message:</strong> {e.Message}";
                if (e.InnerException != null)
                {
                    message = $"Could not connect to Plex, please check your settings. <strong>Exception Message:</strong> {e.InnerException.Message}";
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = message });
            }
        }
    }
}