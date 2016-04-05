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
using System.Collections.Generic;
using System.Linq;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
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
        private IPlexApi PlexApi { get; }


        public void CheckAndUpdateAll(long check)
        {
            Log.Trace("This is check no. {0}", check);
            Log.Trace("Getting the settings");
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();
            Log.Trace("Getting all the requests");
            var requests = RequestService.GetAll();

            var requestedModels = requests as RequestedModel[] ?? requests.Where(x => !x.Available).ToArray();
            Log.Trace("Requests Count {0}", requestedModels.Length);

            if (!ValidateSettings(plexSettings, authSettings) || !requestedModels.Any())
            {
                Log.Info("Validation of the settings failed or there is no requests.");
                return;
            }

            var modifiedModel = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                Log.Trace("We are going to see if Plex has the following title: {0}", r.Title);
                PlexSearch results;
                try
                {
                    results = PlexApi.SearchContent(authSettings.PlexAuthToken, r.Title, plexSettings.FullUri);
                }
                catch (Exception e)
                {
                    Log.Error("We failed to search Plex for the following request:");
                    Log.Error(r.DumpJson());
                    Log.Error(e);
                    break; // Let's finish processing and not crash the process, there is a reason why we cannot connect.
                }

                if (results == null)
                {
                    Log.Trace("Could not find any matching result for this title.");
                    continue;
                }

                Log.Trace("Search results from Plex for the following request: {0}", r.Title);
                Log.Trace(results.DumpJson());
                bool matchResult;
                switch (r.Type)
                {
                    case RequestType.Movie:
                        matchResult = MovieTvSearch(results, r.Title, r.ReleaseDate.ToString("yyyy"));
                        break;
                    case RequestType.TvShow:
                        matchResult = MovieTvSearch(results, r.Title, r.ReleaseDate.ToString("yyyy"));
                        break;
                    case RequestType.Album:
                        matchResult = MusicSearch(results, r.Title, r.ArtistName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (matchResult)
                {
                    r.Available = true;
                    modifiedModel.Add(r);
                    continue;
                }

                Log.Trace("The result from Plex where the title's match was null, so that means the content is not yet in Plex.");
            }

            Log.Trace("Updating the requests now");
            Log.Trace("Requests that will be updates:");
            Log.Trace(modifiedModel.SelectMany(x => x.Title).DumpJson());

            if (modifiedModel.Any())
            {
                RequestService.BatchUpdate(modifiedModel);
            }
        }

        /// <summary>
        /// Determines whether the specified title is available.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="year">The year.</param>
        /// <param name="artist">The artist.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationSettingsException">The settings are not configured for Plex or Authentication</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">null</exception>
        public bool IsAvailable(string title, string year, string artist, PlexType type)
        {
            Log.Trace("Checking if the following {0} {1} is available in Plex", title, year);
            var plexSettings = Plex.GetSettings();
            var authSettings = Auth.GetSettings();

            if (!ValidateSettings(plexSettings, authSettings))
            {
                Log.Warn("The settings are not configured");
                throw new ApplicationSettingsException("The settings are not configured for Plex or Authentication");
            }
            var results = PlexApi.SearchContent(authSettings.PlexAuthToken, title, plexSettings.FullUri);

            switch (type)
            {
                case PlexType.Movie:
                    return MovieTvSearch(results, title, year);
                case PlexType.TvShow:
                    return MovieTvSearch(results, title, year);
                case PlexType.Music:
                    return MusicSearch(results, title, artist);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Searches the movies and TV shows on Plex.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="title">The title.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        private bool MovieTvSearch(PlexSearch results, string title, string year)
        {
            if (!string.IsNullOrEmpty(year))
            {
                var result = results.Video?.FirstOrDefault(x => x.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase) && x.Year == year);

                var directoryResult = false;
                if (results.Directory != null)
                {
                    if (results.Directory.Any(d => d.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && d.Year == year))
                    {
                        directoryResult = true;
                    }
                }
                return result?.Title != null || directoryResult;
            }
            else
            {
                var result = results.Video?.FirstOrDefault(x => x.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));
                var directoryResult = false;
                if (results.Directory != null)
                {
                    if (results.Directory.Any(d => d.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        directoryResult = true;
                    }
                }
                return result?.Title != null || directoryResult;
            }
        }

        /// <summary>
        /// Searches the music on Plex.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="title">The title.</param>
        /// <param name="artist">The artist.</param>
        /// <returns></returns>
        private bool MusicSearch(PlexSearch results, string title, string artist)
        {
            foreach (var r in results.Directory)
            {
                var titleMatch = r.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase);
                var artistMatch = r.ParentTitle.Equals(artist, StringComparison.CurrentCultureIgnoreCase);
                if (titleMatch && artistMatch)
                {
                    return true;
                }
            }
            return false;
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