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
using System.Threading.Tasks;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Models;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Quartz;

using Action = PlexRequests.Helpers.Analytics.Action;

namespace PlexRequests.Services.Jobs
{
    public class PlexAvailabilityChecker : IJob, IAvailabilityChecker
    {
        public PlexAvailabilityChecker(ISettingsService<PlexSettings> plexSettings, IRequestService request, IPlexApi plex, ICacheProvider cache,
            INotificationService notify, IJobRecord rec, IRepository<UsersToNotify> users)
        {
            Plex = plexSettings;
            RequestService = request;
            PlexApi = plex;
            Cache = cache;
            Notification = notify;
            Job = rec;
            UserNotifyRepo = users;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IPlexApi PlexApi { get; }
        private ICacheProvider Cache { get; }
        private INotificationService Notification { get; }
        private IJobRecord Job { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        public void CheckAndUpdateAll()
        {
            var plexSettings = Plex.GetSettings();

            if (!ValidateSettings(plexSettings))
            {
                Log.Debug("Validation of the plex settings failed.");
                return;
            }

            var libraries = CachedLibraries(plexSettings, true); //force setting the cache (10 min intervals via scheduler)

            if (libraries == null || !libraries.Any())
            {
                Log.Debug("Did not find any libraries in Plex.");
                return;
            }

            var movies = GetPlexMovies().ToArray();
            var shows = GetPlexTvShows().ToArray();
            var albums = GetPlexAlbums().ToArray();

            var requests = RequestService.GetAll();
            var requestedModels = requests as RequestedModel[] ?? requests.Where(x => !x.Available).ToArray();

            if (!requestedModels.Any())
            {
                Log.Debug("There are no requests to check.");
                return;
            }

            var modifiedModel = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                var releaseDate = r.ReleaseDate == DateTime.MinValue ? string.Empty : r.ReleaseDate.ToString("yyyy");

                bool matchResult;
                switch (r.Type)
                {
                    case RequestType.Movie:
                        matchResult = IsMovieAvailable(movies, r.Title, releaseDate, r.ImdbId);
                        break;
                    case RequestType.TvShow:
                        matchResult = IsTvShowAvailable(shows, r.Title, releaseDate, r.TvDbId);
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

            }

            Log.Debug("Requests that will be updated count {0}", modifiedModel.Count);

            if (modifiedModel.Any())
            {
                NotifyUsers(modifiedModel, plexSettings.PlexAuthToken);
                RequestService.BatchUpdate(modifiedModel);
            }

            Job.Record(JobNames.PlexChecker);

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
                    movies.AddRange(lib.Video.Select(video => new PlexMovie
                    {
                        ReleaseYear = video.Year,
                        Title = video.Title,
                        ProviderId = video.ProviderId,
                    }));
                }
            }
            return movies;
        }

        public bool IsMovieAvailable(PlexMovie[] plexMovies, string title, string year, string providerId = null)
        {
            var advanced = !string.IsNullOrEmpty(providerId);
            foreach (var movie in plexMovies)
            {
                if (advanced)
                {
                    if (!string.IsNullOrEmpty(movie.ProviderId) && 
                        movie.ProviderId.Equals(providerId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                if (movie.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) &&
                    movie.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
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
                    shows.AddRange(lib.Directory.Select(x => new PlexTvShow // shows are in the directory list
                    {
                        Title = x.Title,
                        ReleaseYear = x.Year,
                        ProviderId = x.ProviderId,
                    }));
                }
            }
            return shows;
        }

        public bool IsTvShowAvailable(PlexTvShow[] plexShows, string title, string year, string providerId = null)
        {
            var advanced = !string.IsNullOrEmpty(providerId);
            foreach (var show in plexShows)
            {
                if (advanced)
                {
                    if (!string.IsNullOrEmpty(show.ProviderId) && 
                        show.ProviderId.Equals(providerId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                if (show.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) &&
                    show.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsEpisodeAvailable(string theTvDbId, int season, int episode)
        {
            var episodes = Cache.Get<List<PlexEpisodeModel>>(CacheKeys.PlexEpisodes);
            foreach (var result in episodes)
            {
                if (result.Episodes.ProviderId.Equals(theTvDbId) && result.Episodes.EpisodeNumber == episode && result.Episodes.SeasonNumber == season)
                {
                    return true;
                }
            }
            return false;
        }

        public List<PlexAlbum> GetPlexAlbums()
        {
            var albums = new List<PlexAlbum>();
            var libs = Cache.Get<List<PlexSearch>>(CacheKeys.PlexLibaries);
            if (libs != null)
            {
                var albumLibs = libs.Where(x =>
                        x.Directory.Any(y =>
                            y.Type.Equals(PlexMediaType.Artist.ToString(), StringComparison.CurrentCultureIgnoreCase)
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

        private List<PlexSearch> CachedLibraries(PlexSettings plexSettings, bool setCache)
        {
            var results = new List<PlexSearch>();

            if (!ValidateSettings(plexSettings))
            {
                Log.Warn("The settings are not configured");
                return results; // don't error out here, just let it go! let it goo!!!
            }

            try
            {
                if (setCache)
                {
                    results = GetLibraries(plexSettings);
                    if (plexSettings.AdvancedSearch)
                    {
                        for (var i = 0; i < results.Count; i++)
                        {
                            for (var j = 0; j < results[i].Directory.Count; j++)
                            {
                                var currentItem = results[i].Directory[j];
                                var metaData = PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                                    currentItem.RatingKey);
                                var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.Directory.Guid);
                                results[i].Directory[j].ProviderId = providerId;
                            }
                            for (var j = 0; j < results[i].Video.Count; j++)
                            {
                                var currentItem = results[i].Video[j];
                                var metaData = PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                                    currentItem.RatingKey);
                                var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.Video.Guid);
                                results[i].Video[j].ProviderId = providerId;
                            }
                        }
                    }
                    if (results != null)
                    {
                        Cache.Set(CacheKeys.PlexLibaries, results, CacheKeys.TimeFrameMinutes.SchedulerCaching);
                    }
                }
                else
                {
                    results = Cache.GetOrSet(CacheKeys.PlexLibaries, () =>
                    GetLibraries(plexSettings), CacheKeys.TimeFrameMinutes.SchedulerCaching);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to obtain Plex libraries");
            }

            return results;
        }

        private List<PlexSearch> GetLibraries(PlexSettings plexSettings)
        {
            var sections = PlexApi.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri);

            List<PlexSearch> libs = new List<PlexSearch>();
            if (sections != null)
            {
                foreach (var dir in sections.Directories)
                {
                    var lib = PlexApi.GetLibrary(plexSettings.PlexAuthToken, plexSettings.FullUri, dir.Key);
                    if (lib != null)
                    {
                        libs.Add(lib);
                    }
                }
            }

            return libs;
        }

        private bool ValidateSettings(PlexSettings plex)
        {
            if (plex?.Ip == null || plex?.PlexAuthToken == null)
            {
                Log.Warn("A setting is null, Ensure Plex is configured correctly, and we have a Plex Auth token.");
                return false;
            }
            return true;
        }

        private void NotifyUsers(IEnumerable<RequestedModel> modelChanged, string apiKey)
        {
            try
            {
                var plexUser = PlexApi.GetUsers(apiKey);
                var userAccount = PlexApi.GetAccount(apiKey);

                var adminUsername = userAccount.Username ?? string.Empty;

                var users = UserNotifyRepo.GetAll().ToList();
                Log.Debug("Notifying Users Count {0}", users.Count);
                foreach (var model in modelChanged)
                {
                    var selectedUsers = users.Select(x => x.Username).Intersect(model.RequestedUsers);
                    foreach (var user in selectedUsers)
                    {
                        Log.Info("Notifying user {0}", user);
                        if (user == adminUsername)
                        {
                            Log.Info("This user is the Plex server owner");
                            PublishUserNotification(userAccount.Username, userAccount.Email, model.Title);
                            return;
                        }

                        var email = plexUser.User.FirstOrDefault(x => x.Username == user);
                        if (email == null)
                        {
                            Log.Info("There is no email address for this Plex user, cannot send notification");
                            // We do not have a plex user that requested this!
                            continue;
                        }

                        Log.Info("Sending notification to: {0} at: {1}, for title: {2}", email.Username, email.Email, model.Title);
                        PublishUserNotification(email.Username, email.Email, model.Title);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void PublishUserNotification(string username, string email, string title)
        {
            var notificationModel = new NotificationModel
            {
                User = username,
                UserEmail = email,
                NotificationType = NotificationType.RequestAvailable,
                Title = title
            };

            // Send the notification to the user.
            Notification.Publish(notificationModel);
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                CheckAndUpdateAll();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}