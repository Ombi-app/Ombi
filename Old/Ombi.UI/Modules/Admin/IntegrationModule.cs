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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MarkdownSharp;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Core.StatusChecker;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.UI.Helpers;
using Ombi.UI.Models;
using Action = Ombi.Helpers.Analytics.Action;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class IntegrationModule : BaseModule
    {
        public IntegrationModule(ISettingsService<PlexRequestSettings> settingsService,  ISettingsService<WatcherSettings> watcher,
            ISettingsService<CouchPotatoSettings> cp,ISecurityExtensions security, IAnalytics a, ISettingsService<RadarrSettings> radarrSettings,
            ICacheProvider cache, IRadarrApi radarrApi, ISonarrApi sonarrApi) : base("admin", settingsService, security)
        {
           
            WatcherSettings = watcher;
            Analytics = a;
            CpSettings = cp;
            Cache = cache;
            RadarrApi = radarrApi;
            RadarrSettings = radarrSettings;
            SonarrApi = sonarrApi;

            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);


            Post["/sonarrrootfolders"] = _ => GetSonarrRootFolders();
            Post["/radarrrootfolders"] = _ => GetSonarrRootFolders();

            Get["/watcher", true] = async (x, ct) => await Watcher();
            Post["/watcher", true] = async (x, ct) => await SaveWatcher();
            
            Get["/radarr", true] = async (x, ct) => await Radarr();
            Post["/radarr", true] = async (x, ct) => await SaveRadarr();


            Post["/radarrprofiles"] = _ => GetRadarrQualityProfiles();
        }
        
        private ISettingsService<WatcherSettings> WatcherSettings { get; }
        private ISettingsService<CouchPotatoSettings> CpSettings { get; }
        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private IRadarrApi RadarrApi { get; }
        private ICacheProvider Cache { get; }
        private IAnalytics Analytics { get; }
        private ISonarrApi SonarrApi { get; }

        private async Task<Negotiator> Watcher()
        {
            var settings = await WatcherSettings.GetSettingsAsync();
           
            return View["Watcher", settings];
        }

        private async Task<Response> SaveWatcher()
        {
            var settings = this.Bind<WatcherSettings>();
            var valid = this.Validate(settings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            var cpSettings = await CpSettings.GetSettingsAsync().ConfigureAwait(false);

            if (cpSettings.Enabled)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "Cannot have Watcher and CouchPotato both enabled."
                    });
            }

            var radarrSettings = await RadarrSettings.GetSettingsAsync();

            if (radarrSettings.Enabled)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "Cannot have Radarr and CouchPotato both enabled."
                    });
            }

            settings.ApiKey = settings.ApiKey.Trim();
            var result = await WatcherSettings.SaveSettingsAsync(settings);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Watcher!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private async Task<Negotiator> Radarr()
        {
            var settings = await RadarrSettings.GetSettingsAsync();

            return View["Radarr", settings];
        }

        private async Task<Response> SaveRadarr()
        {
            var radarrSettings = this.Bind<RadarrSettings>();

            //Check Watcher and CP make sure they are not enabled
            var watcher = await WatcherSettings.GetSettingsAsync();
            if (watcher.Enabled)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Watcher is enabled, we cannot enable Watcher and Radarr" });
            }

            var cp = await CpSettings.GetSettingsAsync();
            if (cp.Enabled)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "CouchPotato is enabled, we cannot enable Radarr and CouchPotato" });
            }

            var valid = this.Validate(radarrSettings);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            radarrSettings.ApiKey = radarrSettings.ApiKey.Trim();
            var result = await RadarrSettings.SaveSettingsAsync(radarrSettings);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Radarr!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }

        private Response GetRadarrQualityProfiles()
        {
            var settings = this.Bind<RadarrSettings>();
            var profiles = RadarrApi.GetProfiles(settings.ApiKey, settings.FullUri);

            // set the cache
            if (profiles != null)
            {
                Cache.Set(CacheKeys.RadarrQualityProfiles, profiles);
            }

            return Response.AsJson(profiles);
        }

        private Response GetSonarrRootFolders()
        {
            var settings = this.Bind<SonarrSettings>();

            var rootFolders = SonarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);

            // set the cache
            if (rootFolders != null)
            {
                Cache.Set(CacheKeys.SonarrRootFolders, rootFolders);
            }

            return Response.AsJson(rootFolders);
        }

        private Response GetRadarrRootFolders()
        {
            var settings = this.Bind<RadarrSettings>();

            var rootFolders = RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);

            // set the cache
            if (rootFolders != null)
            {
                Cache.Set(CacheKeys.SonarrRootFolders, rootFolders);
            }

            return Response.AsJson(rootFolders);
        }

    }
}