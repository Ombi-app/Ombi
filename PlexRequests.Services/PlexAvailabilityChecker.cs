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
using System.Collections.Generic;
using System.Linq;

using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;

namespace PlexRequests.Services
{
    public class PlexAvailabilityChecker : IAvailabilityChecker
    {
        public PlexAvailabilityChecker(ISettingsService<PlexSettings> plexSettings, ISettingsService<AuthenticationSettings> auth, IRequestService request)
        {
            Plex = plexSettings;
            Auth = auth;
            RequestService = request;
        }

        private ISettingsService<PlexSettings> Plex { get; } 
        private ISettingsService<AuthenticationSettings> Auth { get; } 
        private IRequestService RequestService { get; }


        public void CheckAndUpdateAll(long check)
        {
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();
            var requests = RequestService.GetAll();
            var api = new PlexApi();

            var modifiedModel = new List<RequestedModel>();
            foreach (var r in requests)
            {
                var results = api.SearchContent(authSettings.PlexAuthToken, r.Title, plexSettings.FullUri);
                var result = results.Video.FirstOrDefault(x => x.Title == r.Title);
                var originalRequest = RequestService.Get(r.Id);

                originalRequest.Available = result != null;
                modifiedModel.Add(originalRequest);
            }

           RequestService.BatchUpdate(modifiedModel);
        }
    }
}