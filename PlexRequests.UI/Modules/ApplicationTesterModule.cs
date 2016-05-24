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
using Nancy.Validation;
using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class ApplicationTesterModule : BaseAuthModule
    {

        public ApplicationTesterModule(ICouchPotatoApi cpApi, ISonarrApi sonarrApi, IPlexApi plexApi,
            ISettingsService<AuthenticationSettings> authSettings, ISickRageApi srApi, IHeadphonesApi hpApi, ISettingsService<PlexRequestSettings> pr) : base("test", pr)
        {
            this.RequiresAuthentication();
            
            CpApi = cpApi;
            SonarrApi = sonarrApi;
            PlexApi = plexApi;
            AuthSettings = authSettings;
            SickRageApi = srApi;
            HeadphonesApi = hpApi;

            Post["/cp"] = _ => CouchPotatoTest();
            Post["/sonarr"] = _ => SonarrTest();
            Post["/plex"] = _ => PlexTest();
            Post["/sickrage"] = _ => SickRageTest();
            Post["/headphones"] = _ => HeadphonesTest();

        }
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ISonarrApi SonarrApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private IPlexApi PlexApi { get; }
        private ISickRageApi SickRageApi { get; }
        private IHeadphonesApi HeadphonesApi { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }

        private Response CouchPotatoTest()
        {
            var couchPotatoSettings = this.Bind<CouchPotatoSettings>();
            var valid = this.Validate(couchPotatoSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            try
            {
                var status = CpApi.GetStatus(couchPotatoSettings.FullUri, couchPotatoSettings.ApiKey);
                return status.success
               ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Connected to CouchPotato successfully!" })
               : Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to CouchPotato, please check your settings." });

            }
			catch (Exception e) // Exceptions are expected if we cannot connect so we will just log and swallow them.
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
            var valid = this.Validate(sonarrSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            try
            {
                var status = SonarrApi.SystemStatus(sonarrSettings.ApiKey, sonarrSettings.FullUri);
                return status?.version != null
               ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Connected to Sonarr successfully!" })
               : Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to Sonarr, please check your settings." });

            }
			catch (Exception e) // Exceptions are expected, if we cannot connect so we will just log and swallow them.
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
            var valid = this.Validate(plexSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
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
			catch (Exception e) // Exceptions are expected, if we cannot connect so we will just log and swallow them.
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

        private Response SickRageTest()
        {
            var sickRageSettings = this.Bind<SickRageSettings>();
            var valid = this.Validate(sickRageSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            try
            {
                var status = SickRageApi.Ping(sickRageSettings.ApiKey, sickRageSettings.FullUri);
                return status?.result == "success"
                    ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Connected to SickRage successfully!" })
               : Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to SickRage, please check your settings." });

            }
			catch (Exception e) // Exceptions are expected, if we cannot connect so we will just log and swallow them.
            {
                Log.Warn("Exception thrown when attempting to get SickRage's status: ");
                Log.Warn(e);
                var message = $"Could not connect to SickRage, please check your settings. <strong>Exception Message:</strong> {e.Message}";
                if (e.InnerException != null)
                {
                    message = $"Could not connect to SickRage, please check your settings. <strong>Exception Message:</strong> {e.InnerException.Message}";
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = message });
            }
        }

        private Response HeadphonesTest()
        {
            var settings = this.Bind<HeadphonesSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            try
            {
                var result = HeadphonesApi.GetVersion(settings.ApiKey, settings.FullUri);
                if (!string.IsNullOrEmpty(result.latest_version))
                {
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Result = true,
                            Message = "Connected to Headphones successfully!"
                        });
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not connect to Headphones, please check your settings." });
            }
			catch (Exception e)
            {
                Log.Warn("Exception thrown when attempting to get Headphones's status: ");
                Log.Warn(e);
                var message = $"Could not connect to Headphones, please check your settings. <strong>Exception Message:</strong> {e.Message}";
                if (e.InnerException != null)
                {
                    message = $"Could not connect to Headphones, please check your settings. <strong>Exception Message:</strong> {e.InnerException.Message}";
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = message }); ;
            }
        }
    }
}