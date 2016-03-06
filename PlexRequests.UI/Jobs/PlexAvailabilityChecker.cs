#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexAvailabilityChecker.cs
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
using System.Linq;
using System.Web.Hosting;
using FluentScheduler;
using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Jobs
{
    public class PlexAvailabilityChecker : IAvailabilityChecker, ITask, IRegisteredObject
    {
        public PlexAvailabilityChecker(ISettingsService<PlexSettings> plexSettings, ISettingsService<AuthenticationSettings> auth, IRequestService request)
        {
            Plex = plexSettings;
            Auth = auth;
            RequestService = request;
            HostingEnvironment.RegisterObject(this);
        }
        private readonly object _lock = new object();

        private bool _shuttingDown;
        private ISettingsService<PlexSettings> Plex { get; } 
        private ISettingsService<AuthenticationSettings> Auth { get; } 
        private IRequestService RequestService { get; set; }
        public void CheckAndUpdate(string searchTerm, int id)
        {
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();

            var api = new PlexApi();
            var results = api.SearchContent(authSettings.PlexAuthToken, searchTerm, plexSettings.FullUri);

            var result = results.Video.FirstOrDefault(x => x.Title == searchTerm);
            var originalRequest = RequestService.Get(id);

            originalRequest.Available = result != null;
            RequestService.UpdateRequest(id, originalRequest);
        }

        public void CheckAndUpdateAll()
        {
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();
            var requests = RequestService.GetAll();
            var api = new PlexApi();


            foreach (var r in requests)
            {
                var results = api.SearchContent(authSettings.PlexAuthToken, r.Title, plexSettings.FullUri);
                var result = results.Video.FirstOrDefault(x => x.Title == r.Title);
                var originalRequest = RequestService.Get(r.Id);

                originalRequest.Available = result != null;
                RequestService.UpdateRequest(r.Id, originalRequest);
            }
        }

        public void Execute()
        {
            lock (_lock)
            {
                if (_shuttingDown)
                    return;

                CheckAndUpdateAll();
            }

        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }
    }
}