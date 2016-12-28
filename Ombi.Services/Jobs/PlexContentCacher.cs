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
using Dapper;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Ombi.Services.Models;
using Ombi.Store.Models;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Quartz;
using PlexMediaType = Ombi.Api.Models.Plex.PlexMediaType;

namespace Ombi.Services.Jobs
{
    public class PlexContentCacher : IJob, IPlexContentCacher
    {
        public PlexContentCacher(ISettingsService<PlexSettings> plexSettings, IRequestService request, IPlexApi plex, ICacheProvider cache,
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

        public void CacheContent()
        {
            var plexSettings = Plex.GetSettings();

            if (!ValidateSettings(plexSettings))
            {
                Log.Debug("Validation of the plex settings failed.");
                return;
            }
            var libraries = CachedLibraries(plexSettings); 

            if (libraries == null || !libraries.Any())
            {
                Log.Debug("Did not find any libraries in Plex.");
                return;
            }
        }


        public List<PlexMovie> GetPlexMovies(List<PlexSearch> libs)
        {
            var settings = Plex.GetSettings();
            var movies = new List<PlexMovie>();
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

        public List<PlexTvShow> GetPlexTvShows(List<PlexSearch> libs)
        {
            var settings = Plex.GetSettings();
            var shows = new List<PlexTvShow>();
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
                        Url = PlexHelper.GetPlexMediaUrl(settings.MachineIdentifier, x.RatingKey),

                    }));
                }
            }
            return shows;
        }

        


        public List<PlexAlbum> GetPlexAlbums(List<PlexSearch> libs)
        {
            var settings = Plex.GetSettings();
            var albums = new List<PlexAlbum>();
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
                        ProviderId = x.ProviderId,
                        ReleaseYear = x.Year,
                        Artist = x.ParentTitle,
                        Url = PlexHelper.GetPlexMediaUrl(settings.MachineIdentifier, x.RatingKey)
                    }));
                }
            }
            return albums;
        }



        private List<PlexSearch> CachedLibraries(PlexSettings plexSettings)
        {
            var results = new List<PlexSearch>();

            if (!ValidateSettings(plexSettings))
            {
                Log.Warn("The settings are not configured");
                return results; // don't error out here, just let it go! let it goo!!!
            }

            try
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
                                var filtered = seasons.Directory.Where(x => !x.Title.Equals("All episodes", StringComparison.CurrentCultureIgnoreCase));

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

                    var movies = GetPlexMovies(results);

                    //// Time to destroy the plex movies from the DB
                    //PlexContent.Custom(connection =>
                    //{
                    //    connection.Open();
                    //    connection.Query("delete from PlexContent where type = @type", new { type = 0 });
                    //    return new List<PlexContent>();
                    //});

                    foreach (var m in movies)
                    {
                        if (string.IsNullOrEmpty(m.ProviderId))
                        {
                            Log.Error("Provider Id on movie {0} is null",m.Title);
                            continue;
                        }

                        // Check if it exists
                        var item = PlexContent.Custom(connection =>
                        {
                            connection.Open();
                            var media =  connection.QueryFirstOrDefault<PlexContent>("select * from PlexContent where ProviderId = @ProviderId and type = @type", new { m.ProviderId, type = 0 });
                            connection.Dispose();
                            return media;
                        });

                        if (item == null)
                        { 
                            // Doesn't exist, insert it
                            PlexContent.Insert(new PlexContent
                            {
                                ProviderId = m.ProviderId,
                                ReleaseYear = m.ReleaseYear ?? string.Empty,
                                Title = m.Title,
                                Type = Store.Models.Plex.PlexMediaType.Movie,
                                Url = m.Url
                            });
                        }
                    }

                    var tv = GetPlexTvShows(results);
                    //// Time to destroy the plex tv from the DB
                    //PlexContent.Custom(connection =>
                    //{
                    //    connection.Open();
                    //    connection.Query("delete from PlexContent where type = @type", new { type = 1 });
                    //    return new List<PlexContent>();
                    //});
                    foreach (var t in tv)
                    {
                        if (string.IsNullOrEmpty(t.ProviderId))
                        {
                            Log.Error("Provider Id on tv {0} is null", t.Title);
                            continue;
                        }


                        // Check if it exists
                        var item = PlexContent.Custom(connection =>
                        {
                            connection.Open();
                            var media = connection.QueryFirstOrDefault<PlexContent>("select * from PlexContent where ProviderId = @ProviderId and type = @type", new { t.ProviderId, type = 1 });
                            connection.Dispose();
                            return media;
                        });

                        if (item == null)
                        {
                            PlexContent.Insert(new PlexContent
                            {
                                ProviderId = t.ProviderId,
                                ReleaseYear = t.ReleaseYear ?? string.Empty,
                                Title = t.Title,
                                Type = Store.Models.Plex.PlexMediaType.Show,
                                Url = t.Url,
                                Seasons = ByteConverterHelper.ReturnBytes(t.Seasons)
                            });
                        }
                    }

                    var albums = GetPlexAlbums(results);
                    //// Time to destroy the plex movies from the DB
                    //PlexContent.Custom(connection =>
                    //{
                    //    connection.Open();
                    //    connection.Query("delete from PlexContent where type = @type", new { type = 2 });
                    //    return new List<PlexContent>();
                    //});

                    foreach (var a in albums)
                    {
                        if (string.IsNullOrEmpty(a.ProviderId))
                        {
                            Log.Error("Provider Id on album {0} is null", a.Title);
                            continue;
                        }


                        // Check if it exists
                        var item = PlexContent.Custom(connection =>
                        {
                            connection.Open();
                            var media = connection.QueryFirstOrDefault<PlexContent>("select * from PlexContent where ProviderId = @ProviderId and type = @type", new { a.ProviderId, type = 2 });
                            connection.Dispose();
                            return media;
                        });

                        if (item == null)
                        {

                            PlexContent.Insert(new PlexContent
                            {
                                ProviderId = a.ProviderId,
                                ReleaseYear = a.ReleaseYear ?? string.Empty,
                                Title = a.Title,
                                Type = Store.Models.Plex.PlexMediaType.Artist,
                                Url = a.Url
                            });
                        }
                    }
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

            Job.SetRunning(true, JobNames.PlexCacher);
            try
            {
                CacheContent();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.PlexCacher);
                Job.SetRunning(false, JobNames.PlexCacher);
            }
        }
    }
}