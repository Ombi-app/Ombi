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

using Dapper;

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
using PlexRequests.Store.Models.Plex;
using PlexRequests.Store.Repository;

using Quartz;

namespace PlexRequests.Services.Jobs
{
    public class PlexAvailabilityChecker : IJob, IAvailabilityChecker
    {
        public PlexAvailabilityChecker(ISettingsService<PlexSettings> plexSettings, IRequestService request, IPlexApi plex, ICacheProvider cache,
            INotificationService notify, IJobRecord rec, IRepository<UsersToNotify> users, IRepository<PlexEpisodes> repo, INotificationEngine e, IRepository<PlexContent> content)
        {
            Plex = plexSettings;
            RequestService = request;
            PlexApi = plex;
            Cache = cache;
            Notification = notify;
            Job = rec;
            UserNotifyRepo = users;
            EpisodeRepo = repo;
            NotificationEngine = e;
            PlexContent = content;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IRepository<PlexEpisodes> EpisodeRepo { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IPlexApi PlexApi { get; }
        private ICacheProvider Cache { get; }
        private INotificationService Notification { get; }
        private IJobRecord Job { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        private INotificationEngine NotificationEngine { get; }
        private IRepository<PlexContent> PlexContent { get; }

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
                        if (!plexSettings.EnableTvEpisodeSearching)
                        {
                            matchResult = IsTvShowAvailable(shows, r.Title, releaseDate, r.TvDbId, r.SeasonList);
                        }
                        else
                        {
                            matchResult = r.Episodes.Any() ?
                                r.Episodes.All(x => IsEpisodeAvailable(r.TvDbId, x.SeasonNumber, x.EpisodeNumber)) :
                                IsTvShowAvailable(shows, r.Title, releaseDate, r.TvDbId, r.SeasonList);
                        }
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
                NotificationEngine.NotifyUsers(modifiedModel, plexSettings.PlexAuthToken, NotificationType.RequestAvailable);
                RequestService.BatchUpdate(modifiedModel);
            }
        }

        public List<PlexMovie> GetPlexMovies()
        {
            var settings = Plex.GetSettings();
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
                        Url = PlexHelper.GetPlexMediaUrl(settings.MachineIdentifier, video.RatingKey)
                    }));
                }
            }
            return movies;
        }

        public bool IsMovieAvailable(PlexMovie[] plexMovies, string title, string year, string providerId = null)
        {
            var movie = GetMovie(plexMovies, title, year, providerId);
            return movie != null;
        }

        public PlexMovie GetMovie(PlexMovie[] plexMovies, string title, string year, string providerId = null)
        {
            if (plexMovies.Length == 0)
            {
                return null;
            }
            var advanced = !string.IsNullOrEmpty(providerId);
            foreach (var movie in plexMovies)
            {
                if (string.IsNullOrEmpty(movie.Title) || string.IsNullOrEmpty(movie.ReleaseYear))
                {
                    continue;
                }

                if (advanced)
                {
                    if (!string.IsNullOrEmpty(movie.ProviderId) &&
                        movie.ProviderId.Equals(providerId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return movie;
                    }
                }
                if (movie.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) &&
                    movie.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase))
                {
                    return movie;
                }
            }
            return null;
        }

        public List<PlexTvShow> GetPlexTvShows()
        {
            var settings = Plex.GetSettings();
            var shows = new List<PlexTvShow>();
            var libs = Cache.Get<List<PlexSearch>>(CacheKeys.PlexLibaries);
            if (libs != null)
            {
                var withDir = libs.Where(x => x.Directory != null);
                var tvLibs = withDir.Where(x =>
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
                        Seasons = x.Seasons?.Select(d => PlexHelper.GetSeasonNumberFromTitle(d.Title)).ToArray(),
                        Url = PlexHelper.GetPlexMediaUrl(settings.MachineIdentifier, x.RatingKey)

                    }));
                }
            }
            return shows;
        }

        public bool IsTvShowAvailable(PlexTvShow[] plexShows, string title, string year, string providerId = null, int[] seasons = null)
        {
            var show = GetTvShow(plexShows, title, year, providerId, seasons);
            return show != null;
        }


        public PlexTvShow GetTvShow(PlexTvShow[] plexShows, string title, string year, string providerId = null,
            int[] seasons = null)
        {
            var advanced = !string.IsNullOrEmpty(providerId);
            foreach (var show in plexShows)
            {
                if (advanced)
                {
                    if (show.ProviderId == providerId && seasons != null)
                    {
                        if (seasons.Any(season => show.Seasons.Contains(season)))
                        {
                            return show;
                        }
                        return null;
                    }
                    if (!string.IsNullOrEmpty(show.ProviderId) &&
                        show.ProviderId.Equals(providerId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return show;
                    }
                }
                if (show.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) &&
                    show.ReleaseYear.Equals(year, StringComparison.CurrentCultureIgnoreCase))
                {
                    return show;
                }
            }
            return null;
        }

        public bool IsEpisodeAvailable(string theTvDbId, int season, int episode)
        {
            var ep = EpisodeRepo.Custom(
                connection =>
                {
                    connection.Open();
                    var result = connection.Query<PlexEpisodes>("select * from PlexEpisodes where ProviderId = @ProviderId", new { ProviderId = theTvDbId });

                    return result;
                }).ToList();

            if (!ep.Any())
            {
                Log.Info("Episode cache info is not available. tvdbid: {0}, season: {1}, episode: {2}", theTvDbId, season, episode);
                return false;
            }
            foreach (var result in ep)
            {
                if (result.ProviderId.Equals(theTvDbId) && result.EpisodeNumber == episode && result.SeasonNumber == season)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the episode's db in the cache.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PlexEpisodes>> GetEpisodes()
        {
            var episodes = await EpisodeRepo.GetAllAsync();
            if (episodes == null)
            {
                return new HashSet<PlexEpisodes>();
            }
            return episodes;
        }

        /// <summary>
        /// Gets the episode's stored in the db and then filters on the TheTvDBId.
        /// </summary>
        /// <param name="theTvDbId">The tv database identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<PlexEpisodes>> GetEpisodes(int theTvDbId)
        {
            var ep = await EpisodeRepo.CustomAsync(async connection =>
               {
                   connection.Open();
                   var result = await connection.QueryAsync<PlexEpisodes>("select * from PlexEpisodes where ProviderId = @ProviderId", new { ProviderId = theTvDbId });

                   return result;
               });

            var plexEpisodeses = ep as PlexEpisodes[] ?? ep.ToArray();
            if (!plexEpisodeses.Any())
            {
                Log.Info("Episode db info is not available.");
                return new List<PlexEpisodes>();
            }

            return plexEpisodeses;
        }

        public List<PlexAlbum> GetPlexAlbums()
        {
            var settings = Plex.GetSettings();
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
                        Artist = x.ParentTitle,
                        Url = PlexHelper.GetPlexMediaUrl(settings.MachineIdentifier, x.RatingKey)
                    }));
                }
            }
            return albums;
        }

        public bool IsAlbumAvailable(PlexAlbum[] plexAlbums, string title, string year, string artist)
        {
            return plexAlbums.Any(x =>
                x.Title.Contains(title) &&
                x.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase));
        }

        public PlexAlbum GetAlbum(PlexAlbum[] plexAlbums, string title, string year, string artist)
        {
            return plexAlbums.FirstOrDefault(x =>
                x.Title.Contains(title) &&
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

                // TODO what the fuck was I thinking
                if (setCache)
                {
                    results = GetLibraries(plexSettings);
                    if (plexSettings.AdvancedSearch)
                    {
                        foreach (PlexSearch t in results)
                        {
                            foreach (Directory1 t1 in t.Directory)
                            {
                                var currentItem = t1;
                                var metaData = PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                                    currentItem.RatingKey);

                                // Get the seasons for each show
                                if (currentItem.Type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase))
                                {
                                    var seasons = PlexApi.GetSeasons(plexSettings.PlexAuthToken, plexSettings.FullUri,
                                        currentItem.RatingKey);

                                    // We do not want "all episodes" this as a season
                                    var filtered = seasons.Directory.Where( x => !x.Title.Equals("All episodes", StringComparison.CurrentCultureIgnoreCase));

                                    t1.Seasons.AddRange(filtered);
                                }

                                var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.Directory.Guid);
                                t1.ProviderId = providerId;
                            }
                            foreach (Video t1 in t.Video)
                            {
                                var currentItem = t1;
                                var metaData = PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                                    currentItem.RatingKey);
                                var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.Video.Guid);
                                t1.ProviderId = providerId;
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

            var libs = new List<PlexSearch>();
            if (sections != null)
            {
                foreach (var dir in sections.Directories ?? new List<Directory>())
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

        public void Execute(IJobExecutionContext context)
        {

            Job.SetRunning(true, JobNames.PlexChecker);
            try
            {
                CheckAndUpdateAll();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.PlexChecker);
                Job.SetRunning(false, JobNames.PlexChecker);
            }
        }
    }
}