#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UpdateCheckerModule.cs
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
using System.Threading.Tasks;

using Nancy;

using NLog;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Core.StatusChecker;
using PlexRequests.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UpdateCheckerModule : BaseAuthModule
    {
        public UpdateCheckerModule(ICacheProvider provider, ISettingsService<PlexRequestSettings> pr, ISettingsService<SystemSettings> settings) : base("updatechecker", pr)
        {
            Cache = provider;
            SystemSettings = settings;

            Get["/", true] = async (x,ct) => await CheckLatestVersion();
        }

        private ICacheProvider Cache { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private ISettingsService<SystemSettings> SystemSettings { get; }

        private async Task<Response> CheckLatestVersion()
        {
            try
            {
                if (!IsAdmin)
                {
                    return Response.AsJson(new JsonUpdateAvailableModel { UpdateAvailable = false });
                }
#if DEBUG
                return Response.AsJson(new JsonUpdateAvailableModel {UpdateAvailable = false});
#endif
                var checker = new StatusChecker(SystemSettings);
                var release = await Cache.GetOrSetAsync(CacheKeys.LastestProductVersion, async() => await checker.GetStatus(), 30);

                return Response.AsJson(release.UpdateAvailable 
                    ? new JsonUpdateAvailableModel { UpdateAvailable = true} 
                    : new JsonUpdateAvailableModel { UpdateAvailable = false });
            }
            catch (Exception e)
            {
                Log.Warn("Exception Thrown when attempting to check the status");
                Log.Warn(e);
                return Response.AsJson(new JsonUpdateAvailableModel { UpdateAvailable = false });
            }
        }
    }
}