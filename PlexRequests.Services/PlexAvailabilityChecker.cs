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
using PlexRequests.Services.Models;

namespace PlexRequests.Services
{
    public class PlexAvailabilityChecker : IAvailabilityChecker
    {
        public PlexAvailabilityChecker(ISettingsService<PlexSettings> plexSettings, ISettingsService<AuthenticationSettings> auth, IRequestService request, IPlexApi plex, ICacheProvider cache)
        {
            Plex = plexSettings;
            Auth = auth;
            RequestService = request;
            PlexApi = plex;
            Cache = cache;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private ISettingsService<AuthenticationSettings> Auth { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IPlexApi PlexApi { get; }
        private ICacheProvider Cache { get; }

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

            var libraries = CachedLibraries(authSettings, plexSettings, true); //force setting the cache (10 min intervals via scheduler)
            var movies = GetPlexMovies().ToArray();
            var shows = GetPlexTvShows().ToArray();
            var albums = GetPlexAlbums().ToArray();

            var modifiedModel = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                Log.Trace("We are going to see if Plex has the following title: {0}", r.Title);

                if (libraries == null)
                {
                    libraries = new List<PlexSearch>() { PlexApi.SearchContent(authSettings.PlexAuthToken, r.Title, plexSettings.FullUri) };
                    if (libraries == null)
                    {
                        Log.Trace("Could not find any matching result for this title.");
                        continue;
                    }
                }

                Log.Trace("Search results from Plex for the following request: {0}", r.Title);
                //Log.Trace(results.DumpJson());

                var releaseDate = r.ReleaseDate == DateTime.MinValue ? string.Empty : r.ReleaseDate.ToString("yyyy");

                bool matchResult;
                switch (r.Type)
                {
                    case RequestType.Movie:
                        matchResult = IsMovieAvailable(movies, r.Title, releaseDate);
                        break;
                    case RequestType.TvShow:
                        matchResult = IsTvShowAvailable(shows, r.Title, releaseDate);
                        break;
                    case RequestType.Album:
                        matchResult = IsAlbumAvailable(albums, r.Title, r.ReleaseDate.Year.ToString(), r.ArtistName);
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

        public List<PlexMovie> GetPlexMovies()
        {
            var movies = new List<PlexMovie>();
            var libs = Cache.Get<List<PlexSearch>>(CacheKeys.PlexLibaries);
            if (libs != null)
            {
                var movieLibs = libs.Where(x =>
                        x.Video.Any(y =>
                            y.Type.Equals(PlexMediaType.Movie.ToString(), StringComparison.CurrentCultureIgnoreCase)
                        )
                    ).ToArray();

                foreach (var lib in movieLibs)
                {
                    movies.AddRange(lib.Video.Select(x => new PlexMovie() // movies are in the Video list
                    {
                        Title = x.Title,
                        ReleaseYear = x.Year
                    }));
                }
            }
            return movies;
        }

        public bool IsMovieAvailable(PlexMovie[] plexMovies, string title, string year)
        {
            return plexMovies.Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && x.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase));
        }

        public List<PlexTvShow> GetPlexTvShows()
        {
            var shows = new List<PlexTvShow>();
            var libs = Cache.Get<List<PlexSearch>>(CacheKeys.PlexLibaries);
            if (libs != null)
            {
                var tvLibs = libs.Where(x =>
                        x.Directory.Any(y =>
                            y.Type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase)
                        )
                    ).ToArray();

                foreach (var lib in tvLibs)
                {
                    shows.AddRange(lib.Directory.Select(x => new PlexTvShow() // shows are in the directory list
                    {
                        Title = x.Title,
                        ReleaseYear = x.Year
                    }));
                }
            }
            return shows;
        }

        public bool IsTvShowAvailable(PlexTvShow[] plexShows, string title, string year)
        {
            return plexShows.Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && x.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase));
        }

        public List<PlexAlbum> GetPlexAlbums()
        {
            var albums = new List<PlexAlbum>();
            var libs = Cache.Get<List<PlexSearch>>(CacheKeys.PlexLibaries);
            if (libs != null)
            {
                var albumLibs = libs.Where(x =>
                        x.Directory.Any(y =>
                            y.Type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase)
                        )
                    ).ToArray();

                foreach (var lib in albumLibs)
                {
                    albums.AddRange(lib.Directory.Select(x => new PlexAlbum()
                    {
                        Title = x.Title,
                        ReleaseYear = x.Year,
                        Artist = x.ParentTitle
                    }));
                }
            }
            return albums;
        }

        public bool IsAlbumAvailable(PlexAlbum[] plexAlbums, string title, string year, string artist)
        {
            return plexAlbums.Any(x => 
                x.Title.Contains(title) && 
                //x.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase) &&
                x.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase));
        }

        private List<PlexSearch> CachedLibraries(AuthenticationSettings authSettings, PlexSettings plexSettings, bool setCache)
        {
            Log.Trace("Obtaining library sections from Plex for the following request");

            List<PlexSearch> results = new List<PlexSearch>();

            if (!ValidateSettings(plexSettings, authSettings))
            {
                Log.Warn("The settings are not configured");
                return results; // don't error out here, just let it go!
            }

            if (setCache)
            {
               results = GetLibraries(authSettings, plexSettings);
               Cache.Set(CacheKeys.PlexLibaries, results, 10);
            } 
            else
            {
                results = Cache.GetOrSet(CacheKeys.PlexLibaries, () => {
                    return GetLibraries(authSettings, plexSettings);
                }, 10);
            }
            return results;
        }

        private List<PlexSearch> GetLibraries(AuthenticationSettings authSettings, PlexSettings plexSettings)
        {
            var sections = PlexApi.GetLibrarySections(authSettings.PlexAuthToken, plexSettings.FullUri);

            List<PlexSearch> libs = new List<PlexSearch>();
            if (sections != null)
            {
                foreach (var dir in sections.Directories)
                {
                    Log.Trace("Obtaining results from Plex for the following library section: {0}", dir.Title);
                    var lib = PlexApi.GetLibrary(authSettings.PlexAuthToken, plexSettings.FullUri, dir.Key);
                    if (lib != null)
                    {
                        libs.Add(lib);
                    }
                }
            }

            return libs;
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