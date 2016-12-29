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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Json;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;
using NLog;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Exceptions;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.Services.Notification;
using Ombi.Store.Models;
using Ombi.Store.Repository;
using Ombi.UI.Helpers;
using Ombi.UI.Models;
using Quartz;
using Action = Ombi.Helpers.Analytics.Action;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class AdminModule : BaseModule
    {
        private ISettingsService<PlexRequestSettings> PrService { get; }
        private ISettingsService<CouchPotatoSettings> CpService { get; }
        private ISettingsService<AuthenticationSettings> AuthService { get; }
        private ISettingsService<PlexSettings> PlexService { get; }
        private ISettingsService<SonarrSettings> SonarrService { get; }
        private ISettingsService<SickRageSettings> SickRageService { get; }
        private ISettingsService<EmailNotificationSettings> EmailService { get; }
        private ISettingsService<PushbulletNotificationSettings> PushbulletService { get; }
        private ISettingsService<PushoverNotificationSettings> PushoverService { get; }
        private ISettingsService<HeadphonesSettings> HeadphonesService { get; }
        private ISettingsService<NewletterSettings> NewsLetterService { get; }
        private ISettingsService<LogSettings> LogService { get; }
        private IPlexApi PlexApi { get; }
        private ISonarrApi SonarrApi { get; }
        private IPushbulletApi PushbulletApi { get; }
        private IPushoverApi PushoverApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private IRepository<LogEntity> LogsRepo { get; }
        private INotificationService NotificationService { get; }
        private ICacheProvider Cache { get; }
        private ISettingsService<SlackNotificationSettings> SlackSettings { get; }
        private ISettingsService<LandingPageSettings> LandingSettings { get; }
        private ISettingsService<ScheduledJobsSettings> ScheduledJobSettings { get; }
        private ISlackApi SlackApi { get; }
        private IJobRecord JobRecorder { get; }
        private IAnalytics Analytics { get; }
        private IRecentlyAdded RecentlyAdded { get; }
        private ISettingsService<NotificationSettingsV2> NotifySettings { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public AdminModule(ISettingsService<PlexRequestSettings> prService,
            ISettingsService<CouchPotatoSettings> cpService,
            ISettingsService<AuthenticationSettings> auth,
            ISettingsService<PlexSettings> plex,
            ISettingsService<SonarrSettings> sonarr,
            ISettingsService<SickRageSettings> sickrage,
            ISonarrApi sonarrApi,
            ISettingsService<EmailNotificationSettings> email,
            IPlexApi plexApi,
            ISettingsService<PushbulletNotificationSettings> pbSettings,
            PushbulletApi pbApi,
            ICouchPotatoApi cpApi,
            ISettingsService<PushoverNotificationSettings> pushoverSettings,
            ISettingsService<NewletterSettings> newsletter,
            IPushoverApi pushoverApi,
            IRepository<LogEntity> logsRepo,
            INotificationService notify,
            ISettingsService<HeadphonesSettings> headphones,
            ISettingsService<LogSettings> logs,
            ICacheProvider cache, ISettingsService<SlackNotificationSettings> slackSettings,
            ISlackApi slackApi, ISettingsService<LandingPageSettings> lp,
            ISettingsService<ScheduledJobsSettings> scheduler, IJobRecord rec, IAnalytics analytics,
             ISettingsService<NotificationSettingsV2> notifyService, IRecentlyAdded recentlyAdded
             , ISecurityExtensions security) : base("admin", prService, security)
        {
            PrService = prService;
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
            SickRageService = sickrage;
            LogsRepo = logsRepo;
            PushoverService = pushoverSettings;
            PushoverApi = pushoverApi;
            NotificationService = notify;
            HeadphonesService = headphones;
            NewsLetterService = newsletter;
            LogService = logs;
            Cache = cache;
            SlackSettings = slackSettings;
            SlackApi = slackApi;
            LandingSettings = lp;
            ScheduledJobSettings = scheduler;
            JobRecorder = rec;
            Analytics = analytics;
            NotifySettings = notifyService;
            RecentlyAdded = recentlyAdded;

            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);
            
            Get["/"] = _ => Admin();

            Get["/authentication", true] = async (x, ct) => await Authentication();
            Post["/authentication", true] = async (x, ct) => await SaveAuthentication();

            Post["/", true] = async (x, ct) => await SaveAdmin();

            Post["/requestauth"] = _ => RequestAuthToken();

            Get["/getusers"] = _ => GetUsers();

            Get["/couchpotato"] = _ => CouchPotato();
            Post["/couchpotato"] = _ => SaveCouchPotato();

            Get["/plex"] = _ => Plex();
            Post["/plex", true] = async (x, ct) => await SavePlex();

            Get["/sonarr"] = _ => Sonarr();
            Post["/sonarr"] = _ => SaveSonarr();

            Get["/sickrage"] = _ => Sickrage();
            Post["/sickrage"] = _ => SaveSickrage();

            Post["/sonarrprofiles"] = _ => GetSonarrQualityProfiles();
            Post["/cpprofiles", true] = async (x, ct) => await GetCpProfiles();
            Post["/cpapikey"] = x => GetCpApiKey();

            Get["/emailnotification"] = _ => EmailNotifications();
            Post["/emailnotification"] = _ => SaveEmailNotifications();
            Post["/testemailnotification", true] = async (x, ct) => await TestEmailNotifications();

            Get["/pushbulletnotification"] = _ => PushbulletNotifications();
            Post["/pushbulletnotification"] = _ => SavePushbulletNotifications();
            Post["/testpushbulletnotification", true] = async (x, ct) => await TestPushbulletNotifications();

            Get["/pushovernotification"] = _ => PushoverNotifications();
            Post["/pushovernotification"] = _ => SavePushoverNotifications();
            Post["/testpushovernotification", true] = async (x, ct) => await TestPushoverNotifications();

            Get["/logs"] = _ => Logs();
            Get["/loglevel"] = _ => GetLogLevels();
            Post["/loglevel"] = _ => UpdateLogLevels(Request.Form.level);
            Get["/loadlogs"] = _ => LoadLogs();

            Get["/headphones"] = _ => Headphones();
            Post["/headphones"] = _ => SaveHeadphones();

            Get["/newsletter"] = _ => Newsletter();
            Post["/newsletter"] = _ => SaveNewsletter();

            Post["/createapikey"] = x => CreateApiKey();



            Post["/testslacknotification", true] = async (x, ct) => await TestSlackNotification();

            Get["/slacknotification"] = _ => SlackNotifications();
            Post["/slacknotification"] = _ => SaveSlackNotifications();

            Get["/landingpage", true] = async (x, ct) => await LandingPage();
            Post["/landingpage", true] = async (x, ct) => await SaveLandingPage();

            Get["/scheduledjobs", true] = async (x, ct) => await GetScheduledJobs();
            Post["/scheduledjobs", true] = async (x, ct) => await SaveScheduledJobs();

            Post["/clearlogs", true] = async (x, ct) => await ClearLogs();

            Get["/notificationsettings", true] = async (x, ct) => await NotificationSettings();
            Post["/notificationsettings"] = x => SaveNotificationSettings();

            Post["/recentlyAddedTest"] = x => RecentlyAddedTest();
        }

        private async Task<Negotiator> Authentication()
        {
            var settings = await AuthService.GetSettingsAsync();

            return View["/Authentication", settings];
        }

        private async Task<Response> SaveAuthentication()
        {
            var model = this.Bind<AuthenticationSettings>();

            var result = await AuthService.SaveSettingsAsync(model);
            if (result)
            {
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    return Context.GetRedirect($"~/{BaseUrl}/admin/authentication");
                }
                return Context.GetRedirect("~/admin/authentication");
            }
            if (!string.IsNullOrEmpty(BaseUrl))
            {
                return Context.GetRedirect($"~/{BaseUrl}/error"); //TODO create error page
            }
            return Context.GetRedirect("~/error"); //TODO create error page
        }

        private Negotiator Admin()
        {
            var settings = PrService.GetSettings();

            return View["Settings", settings];
        }

        private async Task<Response> SaveAdmin()
        {
            var model = this.Bind<PlexRequestSettings>();
            var valid = this.Validate(model);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            model.Wizard = true;

            if (!string.IsNullOrWhiteSpace(model.BaseUrl))
            {
                if (model.BaseUrl.StartsWith("/", StringComparison.CurrentCultureIgnoreCase) || model.BaseUrl.StartsWith("\\", StringComparison.CurrentCultureIgnoreCase))
                {
                    model.BaseUrl = model.BaseUrl.Remove(0, 1);
                }
            }
            if (!model.CollectAnalyticData)
            {
                Analytics.TrackEventAsync(Category.Admin, Action.Save, "CollectAnalyticData turned off", Username, CookieHelper.GetAnalyticClientId(Cookies));
            }
            var result = await PrService.SaveSettingsAsync(model);

            Analytics.TrackEventAsync(Category.Admin, Action.Save, "PlexRequestSettings", Username, CookieHelper.GetAnalyticClientId(Cookies));
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true }
                : new JsonResponseModel { Result = false, Message = "We could not save to the database, please try again" });
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

            var oldSettings = PlexService.GetSettings();
            if (oldSettings != null)
            {
                oldSettings.PlexAuthToken = model.user.authentication_token;
                PlexService.SaveSettings(oldSettings);
            }
            else
            {
                var newModel = new PlexSettings
                {
                    PlexAuthToken = model.user.authentication_token
                };
                PlexService.SaveSettings(newModel);
            }

            return Response.AsJson(new { Result = true, AuthToken = model.user.authentication_token });
        }


        private Response GetUsers()
        {
            var settings = PlexService.GetSettings();

            var token = settings?.PlexAuthToken;
            if (token == null)
            {
                return Response.AsJson(new { Result = true, Users = string.Empty });
            }

            try
            {
                var users = PlexApi.GetUsers(token);
                if (users == null)
                {
                    return Response.AsJson(string.Empty);
                }
                if (users.User == null || users.User?.Length == 0)
                {
                    return Response.AsJson(string.Empty);
                }

                var usernames = users.User.Select(x => x.Title);
                return Response.AsJson(new { Result = true, Users = usernames });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                if (ex is WebException || ex is ApiRequestException)
                {
                    return Response.AsJson(new { Result = false, Message = "Could not load the user list! We have connectivity problems connecting to Plex, Please ensure we can access Plex.Tv, The error has been logged." });
                }

                return Response.AsJson(new { Result = false, Message = ex.Message });
            }
        }

        private Negotiator CouchPotato()
        {
            var settings = CpService.GetSettings();

            return View["CouchPotato", settings];
        }

        private Response SaveCouchPotato()
        {
            var couchPotatoSettings = this.Bind<CouchPotatoSettings>();
            var valid = this.Validate(couchPotatoSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            couchPotatoSettings.ApiKey = couchPotatoSettings.ApiKey.Trim();
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

        private async Task<Response> SavePlex()
        {
            var plexSettings = this.Bind<PlexSettings>();
            var valid = this.Validate(plexSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            if (string.IsNullOrEmpty(plexSettings.MachineIdentifier))
            {
                //Lookup identifier
                var server = PlexApi.GetServer(plexSettings.PlexAuthToken);
                plexSettings.MachineIdentifier =
                    server.Server.FirstOrDefault(x => x.AccessToken == plexSettings.PlexAuthToken)?.MachineIdentifier;
            }

            var result = await PlexService.SaveSettingsAsync(plexSettings);

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
            var sickRageEnabled = SickRageService.GetSettings().Enabled;
            if (sickRageEnabled)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "SickRage is enabled, we cannot enable Sonarr and SickRage" });
            }
            sonarrSettings.ApiKey = sonarrSettings.ApiKey.Trim();
            var result = SonarrService.SaveSettings(sonarrSettings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Sonarr!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Negotiator Sickrage()
        {
            var settings = SickRageService.GetSettings();

            return View["Sickrage", settings];
        }

        private Response SaveSickrage()
        {
            var sickRageSettings = this.Bind<SickRageSettings>();

            var valid = this.Validate(sickRageSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            var sonarrEnabled = SonarrService.GetSettings().Enabled;
            if (sonarrEnabled)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Sonarr is enabled, we cannot enable Sonarr and SickRage" });
            }
            sickRageSettings.ApiKey = sickRageSettings.ApiKey.Trim();
            var result = SickRageService.SaveSettings(sickRageSettings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for SickRage!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Response GetSonarrQualityProfiles()
        {
            var settings = this.Bind<SonarrSettings>();
            var profiles = SonarrApi.GetProfiles(settings.ApiKey, settings.FullUri);

            // set the cache
            if (profiles != null)
            {
                Cache.Set(CacheKeys.SonarrQualityProfiles, profiles);
            }

            return Response.AsJson(profiles);
        }


        private Negotiator EmailNotifications()
        {
            var settings = EmailService.GetSettings();
            return View["EmailNotifications", settings];
        }

        private async Task<Response> TestEmailNotifications()
        {
            var settings = this.Bind<EmailNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            var currentSettings = await EmailService.GetSettingsAsync();
            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationType.Test,
                DateTime = DateTime.Now,
                ImgSrc = "http://3.bp.blogspot.com/-EFM-XoKoZ0o/UznF567wCRI/AAAAAAAAALM/6ut7MCF2LrU/s1600/xkcd.png"
            };
            try
            {
                NotificationService.Subscribe(new EmailMessageNotification(EmailService));
                settings.Enabled = true;
                await NotificationService.Publish(notificationModel, settings);
                Log.Info("Sent email notification test");
            }
            catch (Exception)
            {
                Log.Error("Failed to subscribe and publish test Email Notification");
            }
            finally
            {
                if (!currentSettings.Enabled)
                {
                    NotificationService.UnSubscribe(new EmailMessageNotification(EmailService));
                }
            }
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully sent a test Email Notification!" });
        }

        private Response SaveEmailNotifications()
        {
            var settings = this.Bind<EmailNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            if (settings.Authentication)
            {
                if (string.IsNullOrEmpty(settings.EmailUsername) || string.IsNullOrEmpty(settings.EmailPassword))
                {
                    return Response.AsJson(new JsonResponseModel { Result = false, Message = "SMTP Authentication is enabled, please specify a username and password" });
                }
            }

            var result = EmailService.SaveSettings(settings);

            if (settings.Enabled)
            {
                NotificationService.Subscribe(new EmailMessageNotification(EmailService));
            }
            else
            {
                NotificationService.UnSubscribe(new EmailMessageNotification(EmailService));
            }

            Log.Info("Saved email settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Email Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
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

            var result = PushbulletService.SaveSettings(settings);
            if (settings.Enabled)
            {
                NotificationService.Subscribe(new PushbulletNotification(PushbulletApi, PushbulletService));
            }
            else
            {
                NotificationService.UnSubscribe(new PushbulletNotification(PushbulletApi, PushbulletService));
            }

            Log.Info("Saved email settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Pushbullet Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private async Task<Response> TestPushbulletNotifications()
        {
            var settings = this.Bind<PushbulletNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationType.Test,
                DateTime = DateTime.Now
            };
            var currentSettings = await PushbulletService.GetSettingsAsync();
            try
            {
                NotificationService.Subscribe(new PushbulletNotification(PushbulletApi, PushbulletService));
                settings.Enabled = true;
                await NotificationService.Publish(notificationModel, settings);
                Log.Info("Sent pushbullet notification test");
            }
            catch (Exception)
            {
                Log.Error("Failed to subscribe and publish test Pushbullet Notification");
            }
            finally
            {
                if (!currentSettings.Enabled)
                {
                    NotificationService.UnSubscribe(new PushbulletNotification(PushbulletApi, PushbulletService));
                }
            }
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully sent a test Pushbullet Notification!" });
        }

        private Negotiator PushoverNotifications()
        {
            var settings = PushoverService.GetSettings();
            return View["PushoverNotifications", settings];
        }

        private Response SavePushoverNotifications()
        {
            var settings = this.Bind<PushoverNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            var result = PushoverService.SaveSettings(settings);
            if (settings.Enabled)
            {
                NotificationService.Subscribe(new PushoverNotification(PushoverApi, PushoverService));
            }
            else
            {
                NotificationService.UnSubscribe(new PushoverNotification(PushoverApi, PushoverService));
            }

            Log.Info("Saved email settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Pushover Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private async Task<Response> TestPushoverNotifications()
        {
            var settings = this.Bind<PushoverNotificationSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationType.Test,
                DateTime = DateTime.Now
            };
            var currentSettings = await PushoverService.GetSettingsAsync();
            try
            {
                NotificationService.Subscribe(new PushoverNotification(PushoverApi, PushoverService));
                settings.Enabled = true;
                await NotificationService.Publish(notificationModel, settings);
                Log.Info("Sent pushover notification test");
            }
            catch (Exception)
            {
                Log.Error("Failed to subscribe and publish test Pushover Notification");
            }
            finally
            {
                if (!currentSettings.Enabled)
                {
                    NotificationService.UnSubscribe(new PushoverNotification(PushoverApi, PushoverService));
                }
            }
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully sent a test Pushover Notification!" });
        }

        private async Task<Response> GetCpProfiles()
        {
            var settings = this.Bind<CouchPotatoSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            var profiles = CpApi.GetProfiles(settings.FullUri, settings.ApiKey);

            // set the cache
            if (profiles != null)
            {
                Cache.Set(CacheKeys.CouchPotatoQualityProfiles, profiles);
            }

            // Save the first profile found (user might not press save...)
            settings.ProfileId = profiles?.list?.FirstOrDefault()?._id;
            await CpService.SaveSettingsAsync(settings);

            return Response.AsJson(profiles);
        }

        private Response GetCpApiKey()
        {
            var settings = this.Bind<CouchPotatoSettings>();

            if (string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password))
            {
                return Response.AsJson(new { Message = "Please enter a username and password to request the Api Key", Result = false });
            }
            var key = CpApi.GetApiKey(settings.FullUri, settings.Username, settings.Password);


            return Response.AsJson(key);
        }

        private Negotiator Logs()
        {
            var model = false;
            if (Request.Query["developer"] != null)
                model = true;
            return View["Logs", model];
        }

        private Response LoadLogs()
        {
            JsonSettings.MaxJsonLength = int.MaxValue;
            var allLogs = LogsRepo.GetAll().OrderByDescending(x => x.Id).Take(200);
            var model = new DatatablesModel<LogEntity> { Data = new List<LogEntity>() };
            foreach (var l in allLogs)
            {
                l.DateString = l.Date.ToString("G");
                model.Data.Add(l);
            }
            return Response.AsJson(model);
        }

        private Response GetLogLevels()
        {
            var levels = LogManager.Configuration.LoggingRules.FirstOrDefault(x => x.NameMatches("database"));
            return Response.AsJson(levels.Levels);
        }

        private Response UpdateLogLevels(int level)
        {
            var settings = LogService.GetSettings();
            Analytics.TrackEventAsync(Category.Admin, Action.Update, "Updated Log Levels", Username, CookieHelper.GetAnalyticClientId(Cookies), level);
            // apply the level
            var newLevel = LogLevel.FromOrdinal(level);
            LoggingHelper.ReconfigureLogLevel(newLevel);

            //save the log settings
            settings.Level = level;
            LogService.SaveSettings(settings);

            return Response.AsJson(new JsonResponseModel { Result = true, Message = $"The new log level is now {newLevel}" });
        }

        private Negotiator Headphones()
        {
            var settings = HeadphonesService.GetSettings();
            return View["Headphones", settings];
        }

        private Response SaveHeadphones()
        {
            var settings = this.Bind<HeadphonesSettings>();

            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                var error = valid.SendJsonError();
                Log.Info("Error validating Headphones settings, message: {0}", error.Message);
                return Response.AsJson(error);
            }
            settings.ApiKey = settings.ApiKey.Trim();
            var result = HeadphonesService.SaveSettings(settings);

            Log.Info("Saved headphones settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Headphones!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Negotiator Newsletter()
        {
            var settings = NewsLetterService.GetSettings();
            return View["NewsletterSettings", settings];
        }

        private Response SaveNewsletter()
        {
            var settings = this.Bind<NewletterSettings>();

            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                var error = valid.SendJsonError();
                Log.Info("Error validating Headphones settings, message: {0}", error.Message);
                return Response.AsJson(error);
            }
            settings.SendRecentlyAddedEmail = settings.SendRecentlyAddedEmail;
            var result = NewsLetterService.SaveSettings(settings);

            Log.Info("Saved headphones settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Newsletter!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }


        private Response CreateApiKey()
        {
            Analytics.TrackEventAsync(Category.Admin, Action.Create, "Created API Key", Username, CookieHelper.GetAnalyticClientId(Cookies));
            var apiKey = Guid.NewGuid().ToString("N");
            var settings = PrService.GetSettings();

            settings.ApiKey = apiKey;
            PrService.SaveSettings(settings);

            return Response.AsJson(apiKey);
        }

        private async Task<Response> TestSlackNotification()
        {
            var settings = this.BindAndValidate<SlackNotificationSettings>();
            if (!ModelValidationResult.IsValid)
            {
                return Response.AsJson(ModelValidationResult.SendJsonError());
            }
            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationType.Test,
                DateTime = DateTime.Now
            };

            var currentSlackSettings = await SlackSettings.GetSettingsAsync();
            try
            {
                NotificationService.Subscribe(new SlackNotification(SlackApi, SlackSettings));
                settings.Enabled = true;
                await NotificationService.Publish(notificationModel, settings);
                Log.Info("Sent slack notification test");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to subscribe and publish test Slack Notification");
            }
            finally
            {
                if (!currentSlackSettings.Enabled)
                {
                    NotificationService.UnSubscribe(new SlackNotification(SlackApi, SlackSettings));
                }
            }
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully sent a test Slack Notification! If you do not receive it please check the logs." });
        }

        private Negotiator SlackNotifications()
        {
            var settings = SlackSettings.GetSettings();
            return View["SlackNotifications", settings];
        }

        private Response SaveSlackNotifications()
        {
            var settings = this.BindAndValidate<SlackNotificationSettings>();
            if (!ModelValidationResult.IsValid)
            {
                return Response.AsJson(ModelValidationResult.SendJsonError());
            }

            var result = SlackSettings.SaveSettings(settings);
            if (settings.Enabled)
            {
                NotificationService.Subscribe(new SlackNotification(SlackApi, SlackSettings));
            }
            else
            {
                NotificationService.UnSubscribe(new SlackNotification(SlackApi, SlackSettings));
            }

            Log.Info("Saved slack settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Slack Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private async Task<Negotiator> LandingPage()
        {
            var settings = await LandingSettings.GetSettingsAsync();
            if (settings.NoticeEnd == DateTime.MinValue)
            {
                settings.NoticeEnd = DateTime.Now;
            }
            if (settings.NoticeStart == DateTime.MinValue)
            {
                settings.NoticeStart = DateTime.Now;
            }
            return View["LandingPage", settings];
        }

        private async Task<Response> SaveLandingPage()
        {
            var settings = this.Bind<LandingPageSettings>();

            Analytics.TrackEventAsync(Category.Admin, Action.Update, "Update Landing Page", Username, CookieHelper.GetAnalyticClientId(Cookies));
            var plexSettings = await PlexService.GetSettingsAsync();
            if (string.IsNullOrEmpty(plexSettings.Ip))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "We cannot enable the landing page if Plex is not setup!" });
            }

            if (settings.Enabled && settings.EnabledNoticeTime && string.IsNullOrEmpty(settings.NoticeMessage))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "If you are going to enabled the notice, then we need a message!" });
            }

            var result = await LandingSettings.SaveSettingsAsync(settings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true }
                : new JsonResponseModel { Result = false, Message = "Could not save to Db Please check the logs" });
        }

        private async Task<Negotiator> GetScheduledJobs()
        {
            var s = await ScheduledJobSettings.GetSettingsAsync();
            var allJobs = await JobRecorder.GetJobsAsync();

            var dict = new Dictionary<string, DateTime>();


            foreach (var j in allJobs)
            {
                DateTime dt;
                if (dict.TryGetValue(j.Name, out dt))
                {
                    // We already have the key... Somehow, we should have never got this record.
                }
                else
                {
                    dict.Add(j.Name,j.LastRun);
                }

            }

            var model = new ScheduledJobsViewModel
            {
                CouchPotatoCacher = s.CouchPotatoCacher,
                PlexAvailabilityChecker = s.PlexAvailabilityChecker,
                SickRageCacher = s.SickRageCacher,
                SonarrCacher = s.SonarrCacher,
                StoreBackup = s.StoreBackup,
                StoreCleanup = s.StoreCleanup,
                JobRecorder = dict,
                RecentlyAddedCron = s.RecentlyAddedCron,
                PlexContentCacher = s.PlexContentCacher,
                FaultQueueHandler = s.FaultQueueHandler,
                PlexEpisodeCacher = s.PlexEpisodeCacher,
                PlexUserChecker = s.PlexUserChecker,
                UserRequestLimitResetter = s.UserRequestLimitResetter
            };
            return View["SchedulerSettings", model];
        }

        private async Task<Response> SaveScheduledJobs()
        {

            Analytics.TrackEventAsync(Category.Admin, Action.Update, "Update ScheduledJobs", Username, CookieHelper.GetAnalyticClientId(Cookies));
            var settings = this.Bind<ScheduledJobsSettings>();

            if (!string.IsNullOrEmpty(settings.RecentlyAddedCron))
            {
                // Validate CRON
                var isValid = CronExpression.IsValidExpression(settings.RecentlyAddedCron);

                if (!isValid)
                {
                    return Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message =
                                $"CRON {settings.RecentlyAddedCron} is not valid. Please ensure you are using a valid CRON."
                    });
                }
            }
            var result = await ScheduledJobSettings.SaveSettingsAsync(settings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true }
                : new JsonResponseModel { Result = false, Message = "Could not save to Db Please check the logs" });
        }

        private async Task<Response> ClearLogs()
        {
            try
            {
                Analytics.TrackEventAsync(Category.Admin, Action.Delete, "Clear Logs", Username, CookieHelper.GetAnalyticClientId(Cookies));
                var allLogs = await LogsRepo.GetAllAsync();
                foreach (var logEntity in allLogs)
                {
                    await LogsRepo.DeleteAsync(logEntity);
                }
                return Response.AsJson(new JsonResponseModel { Result = true, Message = "Logs cleared successfully." });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = e.Message });
            }
        }

        private async Task<Negotiator> NotificationSettings()
        {
            var s = await NotifySettings.GetSettingsAsync();
            return View["NotificationSettings", s];
        }

        private Negotiator SaveNotificationSettings()
        {
            var model = this.Bind<NotificationSettingsV2>();
            return View["NotificationSettings", model];
        }

        private Response RecentlyAddedTest()
        {
            try
            {
                Log.Debug("Clicked TEST");
                RecentlyAdded.Test();
                return Response.AsJson(new JsonResponseModel { Result = true, Message = "Sent email to administrator" });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = e.Message });
            }
        }
    }
}