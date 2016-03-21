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

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers.Exceptions;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;

namespace PlexRequests.Services
{
    public class PlexAvailabilityChecker : IAvailabilityChecker
    {
        public PlexAvailabilityChecker(ISettingsService<PlexSettings> plexSettings, ISettingsService<AuthenticationSettings> auth, IRequestService request, IPlexApi plex)
        {
            Plex = plexSettings;
            Auth = auth;
            RequestService = request;
            PlexApi = plex;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private ISettingsService<AuthenticationSettings> Auth { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IPlexApi PlexApi { get; set; }


        public void CheckAndUpdateAll(long check)
        {
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();
            var requests = RequestService.GetAll();

            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!ValidateSettings(plexSettings, authSettings, requestedModels))
            {
                return;
            }

            var modifiedModel = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                var results = PlexApi.SearchContent(authSettings.PlexAuthToken, r.Title, plexSettings.FullUri);
                var result = results.Video.FirstOrDefault(x => x.Title == r.Title);
                var originalRequest = RequestService.Get(r.Id);

                originalRequest.Available = result != null;
                modifiedModel.Add(originalRequest);
            }

            RequestService.BatchUpdate(modifiedModel);
        }

        /// <summary>
        /// Determines whether the specified search term is available.
        /// </summary>
        /// <param name="title">The search term.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationSettingsException">The settings are not configured for Plex or Authentication</exception>
        public bool IsAvailable(string title, string year)
        {
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();

            if (!ValidateSettings(plexSettings, authSettings))
            {
                throw new ApplicationSettingsException("The settings are not configured for Plex or Authentication");
            }
            if (!string.IsNullOrEmpty(year))
            {
                var results = PlexApi.SearchContent(authSettings.PlexAuthToken, title, plexSettings.FullUri);
                var result = results.Video?.FirstOrDefault(x => x.Title.Contains(title) && x.Year == year);
                var directoryTitle = results.Directory?.Title == title && results.Directory?.Year == year;
                return result?.Title != null || directoryTitle;
            }
            else
            {
                var results = PlexApi.SearchContent(authSettings.PlexAuthToken, title, plexSettings.FullUri);
                var result = results.Video?.FirstOrDefault(x => x.Title.Contains(title));
                var directoryTitle = results.Directory?.Title == title;
                return result?.Title != null || directoryTitle;
            }

        }

        private bool ValidateSettings(PlexSettings plex, AuthenticationSettings auth, IEnumerable<RequestedModel> requests)
        {
            if (plex.Ip == null || auth.PlexAuthToken == null || requests == null)
            {
                Log.Warn("A setting is null, Ensure Plex is configured correctly, and we have a Plex Auth token.");
                return false;
            }
            if (!requests.Any())
            {
                Log.Info("We have no requests to check if they are available on Plex.");
                return false;
            }
            return true;
        }

        private bool ValidateSettings(PlexSettings plex, AuthenticationSettings auth)
        {
            if (plex?.Ip == null || auth?.PlexAuthToken == null)
            {
                Log.Warn("A setting is null, Ensure Plex is configured correctly, and we have a Plex Auth token.");
                return false;
            }
            return true;
        }
    }
}