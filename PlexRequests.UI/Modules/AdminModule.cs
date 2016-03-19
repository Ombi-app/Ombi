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
using System.Linq;

using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Nancy.Validation;

using NLog;

using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Notification;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class AdminModule : NancyModule
    {
        private ISettingsService<PlexRequestSettings> RpService { get; }
        private ISettingsService<CouchPotatoSettings> CpService { get; }
        private ISettingsService<AuthenticationSettings> AuthService { get; }
        private ISettingsService<PlexSettings> PlexService { get; }
        private ISettingsService<SonarrSettings> SonarrService { get; }
        private ISettingsService<EmailNotificationSettings> EmailService { get; }
        private ISettingsService<PushbulletNotificationSettings> PushbulletService { get; }
        private IPlexApi PlexApi { get; }
        private ISonarrApi SonarrApi { get; }
        private PushbulletApi PushbulletApi { get; }
        private ICouchPotatoApi CpApi { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public AdminModule(ISettingsService<PlexRequestSettings> rpService,
            ISettingsService<CouchPotatoSettings> cpService,
            ISettingsService<AuthenticationSettings> auth,
            ISettingsService<PlexSettings> plex,
            ISettingsService<SonarrSettings> sonarr,
            ISonarrApi sonarrApi,
            ISettingsService<EmailNotificationSettings> email,
            IPlexApi plexApi,
            ISettingsService<PushbulletNotificationSettings> pbSettings,
            PushbulletApi pbApi,
            ICouchPotatoApi cpApi) : base("admin")
        {
            RpService = rpService;
            CpService = cpService;
            AuthService = auth;
            PlexService = plex;
            SonarrService = sonarr;
            SonarrApi = sonarrApi;
            EmailService = email;
            PlexApi = plexApi;
            PushbulletService = pbSettings;
            PushbulletApi = pbApi;
            CpApi = cpApi;

#if !DEBUG
            this.RequiresAuthentication();
#endif
            Get["/"] = _ => Admin();

            Get["/authentication"] = _ => Authentication();
            Post["/authentication"] = _ => SaveAuthentication();

            Post["/"] = _ => SaveAdmin();

            Post["/requestauth"] = _ => RequestAuthToken();

            Get["/getusers"] = _ => GetUsers();

            Get["/couchpotato"] = _ => CouchPotato();
            Post["/couchpotato"] = _ => SaveCouchPotato();

            Get["/plex"] = _ => Plex();
            Post["/plex"] = _ => SavePlex();

            Get["/sonarr"] = _ => Sonarr();
            Post["/sonarr"] = _ => SaveSonarr();

            Post["/sonarrprofiles"] = _ => GetSonarrQualityProfiles();
            Post["/cpprofiles"] = _ => GetCpProfiles();

            Get["/emailnotification"] = _ => EmailNotifications();
            Post["/emailnotification"] = _ => SaveEmailNotifications();
            Get["/status"] = _ => Status();

            Get["/pushbulletnotification"] = _ => PushbulletNotifications();
            Post["/pushbulletnotification"] = _ => SavePushbulletNotifications();
        }

        private Negotiator Authentication()
        {
            var settings = AuthService.GetSettings();

            return View["/Authentication", settings];
        }

        private Response SaveAuthentication()
        {
            var model = this.Bind<AuthenticationSettings>();

            var result = AuthService.SaveSettings(model);
            if (result)
            {
                return Context.GetRedirect("~/admin/authentication");
            }
            return Context.GetRedirect("~/error"); //TODO create error page
        }

        private Negotiator Admin()
        {
            var settings = RpService.GetSettings();
            Log.Trace("Getting Settings:");
            Log.Trace(settings.DumpJson());

            return View["Settings", settings];
        }

        private Response SaveAdmin()
        {
            var model = this.Bind<PlexRequestSettings>();

            RpService.SaveSettings(model);


            return Context.GetRedirect("~/admin");
        }

        private Response RequestAuthToken()
        {
            var user = this.Bind<PlexAuth>();

            if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.password))
            {
                return Response.AsJson(new { Result = false, Message = "Please provide a valid username and password" });
            }

            var model = PlexApi.SignIn(user.username, user.password);

            if (model?.user == null)
            {
                return Response.AsJson(new { Result = false, Message = "Incorrect username or password!" });
            }

            var oldSettings = AuthService.GetSettings();
            if (oldSettings != null)
            {
                oldSettings.PlexAuthToken = model.user.authentication_token;
                AuthService.SaveSettings(oldSettings);
            }
            else
            {
                var newModel = new AuthenticationSettings
                {
                    PlexAuthToken = model.user.authentication_token
                };
                AuthService.SaveSettings(newModel);
            }

            return Response.AsJson(new { Result = true, AuthToken = model.user.authentication_token });
        }


        private Response GetUsers()
        {
            var settings = AuthService.GetSettings();

            var token = settings?.PlexAuthToken;
            if (token == null)
            {
                return Response.AsJson(string.Empty);
            }

            var users = PlexApi.GetUsers(token);
            if (users == null)
            {
                return Response.AsJson(string.Empty);
            }
            if (users.User == null || users.User?.Length == 0)
            {
                return Response.AsJson(string.Empty);
            }

            var usernames = users.User.Select(x => x.Username);
            return Response.AsJson(usernames);
        }

        private Negotiator CouchPotato()
        {
            dynamic model = new ExpandoObject();
            var settings = CpService.GetSettings();
            model = settings;

            return View["CouchPotato", model];
        }



        private Response SaveCouchPotato()
        {
            var couchPotatoSettings = this.Bind<CouchPotatoSettings>();
            var valid = this.Validate(couchPotatoSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            var result = CpService.SaveSettings(couchPotatoSettings);
            return Response.AsJson(result 
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for CouchPotato!" } 
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Negotiator Plex()
        {
            var settings = PlexService.GetSettings();

            return View["Plex", settings];
        }

        private Response SavePlex()
        {
            var plexSettings = this.Bind<PlexSettings>();
            var valid = this.Validate(plexSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }


            var result = PlexService.SaveSettings(plexSettings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Plex!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Negotiator Sonarr()
        {
            var settings = SonarrService.GetSettings();

            return View["Sonarr", settings];
        }

        private Response SaveSonarr()
        {
            var sonarrSettings = this.Bind<SonarrSettings>();

            var valid = this.Validate(sonarrSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            var result = SonarrService.SaveSettings(sonarrSettings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Sonarr!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Response GetSonarrQualityProfiles()
        {
            var settings = this.Bind<SonarrSettings>();
            var profiles = SonarrApi.GetProfiles(settings.ApiKey, settings.FullUri);

            return Response.AsJson(profiles);
        }


        private Negotiator EmailNotifications()
        {
            var settings = EmailService.GetSettings();
            return View["EmailNotifications", settings];
        }

        private Response SaveEmailNotifications()
        {
            var settings = this.Bind<EmailNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            Log.Trace(settings.DumpJson());

            var result = EmailService.SaveSettings(settings);
            
            NotificationService.Subscribe(new EmailMessageNotification(EmailService));

            Log.Info("Saved email settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Email Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Negotiator Status()
        {
            var checker = new StatusChecker();
            var status = checker.GetStatus();
            return View["Status", status];
        }

        private Negotiator PushbulletNotifications()
        {
            var settings = PushbulletService.GetSettings();
            return View["PushbulletNotifications", settings];
        }

        private Response SavePushbulletNotifications()
        {
            var settings = this.Bind<PushbulletNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            Log.Trace(settings.DumpJson());

            var result = PushbulletService.SaveSettings(settings);

            NotificationService.Subscribe(new PushbulletNotification(PushbulletApi, PushbulletService));

            Log.Info("Saved email settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Pushbullet Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Response GetCpProfiles()
        {
            var settings = this.Bind<CouchPotatoSettings>();
            var profiles = CpApi.GetProfiles(settings.FullUri, settings.ApiKey);

            return Response.AsJson(profiles);
        }
    }
}