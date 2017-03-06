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
using Dapper;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs.Interfaces;
using Ombi.Store.Models.Emby;
using Ombi.Store.Repository;
using Quartz;
using EmbyMediaType = Ombi.Api.Models.Emby.EmbyMediaType;

namespace Ombi.Services.Jobs
{
    public class EmbyContentCacher : IJob, IEmbyContentCacher
    {
        public EmbyContentCacher(ISettingsService<EmbySettings> embySettings, IRequestService request, IEmbyApi emby, ICacheProvider cache,
             IJobRecord rec, IRepository<EmbyEpisodes> repo, IRepository<EmbyContent> content)
        {
            Emby = embySettings;
            RequestService = request;
            EmbyApi = emby;
            Cache = cache;
            Job = rec;
            EpisodeRepo = repo;
            EmbyContent = content;
        }

        private ISettingsService<EmbySettings> Emby { get; }
        private IRepository<EmbyEpisodes> EpisodeRepo { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IEmbyApi EmbyApi { get; }
        private ICacheProvider Cache { get; }
        private IJobRecord Job { get; }
        private IRepository<EmbyContent> EmbyContent { get; }

        public void CacheContent()
        {
            var embySettings = Emby.GetSettings();
            if (!embySettings.Enable)
            {
                return;
            }
            if (!ValidateSettings(embySettings))
            {
                Log.Debug("Validation of emby settings failed.");
                return;
            }
            CachedLibraries(embySettings);
        }


        public List<EmbyMovieItem> GetMovies()
        {
            var settings = Emby.GetSettings();
            return EmbyApi.GetAllMovies(settings.ApiKey, settings.AdministratorId, settings.FullUri).Items;
        }

        public List<EmbySeriesItem> GetTvShows()
        {
            var settings = Emby.GetSettings();
            return EmbyApi.GetAllShows(settings.ApiKey, settings.AdministratorId, settings.FullUri).Items;
        }

        private void CachedLibraries(EmbySettings embySettings)
        {

            if (!ValidateSettings(embySettings))
            {
                Log.Warn("The settings are not configured");
            }

            try
            {
                var movies = GetMovies();

                foreach (var m in movies)
                {
                    if (m.Type.Equals("boxset", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var info = EmbyApi.GetCollection(m.Id, embySettings.ApiKey,
                            embySettings.AdministratorId, embySettings.FullUri);
                        foreach (var item in info.Items)
                        {
                            var movieInfo = EmbyApi.GetInformation(item.Id, EmbyMediaType.Movie, embySettings.ApiKey,
                                embySettings.AdministratorId, embySettings.FullUri).MovieInformation;
                            ProcessMovies(movieInfo);
                        }
                    }
                    else
                    {
                        var movieInfo = EmbyApi.GetInformation(m.Id, EmbyMediaType.Movie, embySettings.ApiKey,
           embySettings.AdministratorId, embySettings.FullUri).MovieInformation;

                        ProcessMovies(movieInfo);
                    }
                }

                var tv = GetTvShows();

                foreach (var t in tv)
                {
                    var tvInfo = EmbyApi.GetInformation(t.Id, EmbyMediaType.Series, embySettings.ApiKey,
                            embySettings.AdministratorId, embySettings.FullUri).SeriesInformation;
                    if (string.IsNullOrEmpty(tvInfo.ProviderIds?.Tvdb))
                    {
                        Log.Error("Provider Id on tv {0} is null", t.Name);
                        continue;
                    }


                    // Check if it exists
                    var item = EmbyContent.Custom(connection =>
                    {
                        connection.Open();
                        var media = connection.QueryFirstOrDefault<EmbyContent>("select * from EmbyContent where ProviderId = @ProviderId and type = @type", new { ProviderId = tvInfo.ProviderIds.Tvdb, type = 1 });
                        connection.Dispose();
                        return media;
                    });
                    if (item != null && item.EmbyId != t.Id)
                    {
                        // delete this item since the Id has changed
                        EmbyContent.Delete(item);
                        item = null;
                    }

                    if (item == null)
                    {
                        EmbyContent.Insert(new EmbyContent
                        {
                            ProviderId = tvInfo.ProviderIds.Tvdb,
                            PremierDate = tvInfo.PremiereDate,
                            Title = tvInfo.Name,
                            Type = Store.Models.Plex.EmbyMediaType.Series,
                            EmbyId = t.Id,
                            AddedAt = DateTime.UtcNow
                        });
                    }
                }

                //TODO Emby
                //var albums = GetPlexAlbums(results);
                //foreach (var a in albums)
                //{
                //    if (string.IsNullOrEmpty(a.ProviderId))
                //    {
                //        Log.Error("Provider Id on album {0} is null", a.Title);
                //        continue;
                //    }


                //    // Check if it exists
                //    var item = EmbyContent.Custom(connection =>
                //    {
                //        connection.Open();
                //        var media = connection.QueryFirstOrDefault<PlexContent>("select * from EmbyContent where ProviderId = @ProviderId and type = @type", new { a.ProviderId, type = 2 });
                //        connection.Dispose();
                //        return media;
                //    });

                //    if (item == null)
                //    {

                //        EmbyContent.Insert(new PlexContent
                //        {
                //            ProviderId = a.ProviderId,
                //            ReleaseYear = a.ReleaseYear ?? string.Empty,
                //            Title = a.Title,
                //            Type = Store.Models.Plex.PlexMediaType.Artist,
                //            Url = a.Url
                //        });
                //    }
                //}

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to obtain Emby libraries");
            }
        }



        private bool ValidateSettings(EmbySettings emby)
        {
            if (emby.Enable)
            {
                if (emby?.Ip == null || string.IsNullOrEmpty(emby?.ApiKey))
                {
                    Log.Warn("A setting is null, Ensure Emby is configured correctly, and we have a Emby Auth token.");
                    return false;
                }
            }
            return emby.Enable;
        }

        public void Execute(IJobExecutionContext context)
        {

            Job.SetRunning(true, JobNames.EmbyCacher);
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
                Job.Record(JobNames.EmbyCacher);
                Job.SetRunning(false, JobNames.EmbyCacher);
            }
        }

        private void ProcessMovies(EmbyMovieInformation movieInfo)
        {
            if (string.IsNullOrEmpty(movieInfo.ProviderIds.Imdb))
            {
                Log.Error("Provider Id on movie {0} is null", movieInfo.Name);
                return;
            }
            // Check if it exists
            var item = EmbyContent.Custom(connection =>
            {
                connection.Open();
                var media = connection.QueryFirstOrDefault<EmbyContent>("select * from EmbyContent where ProviderId = @ProviderId and type = @type", new { ProviderId = movieInfo.ProviderIds.Imdb, type = 0 });
                connection.Dispose();
                return media;
            });

            if (item != null && item.EmbyId != movieInfo.Id)
            {
                // delete this item since the Id has changed
                EmbyContent.Delete(item);
                item = null;
            }

            if (item == null)
            {
                // Doesn't exist, insert it
                EmbyContent.Insert(new EmbyContent
                {
                    ProviderId = movieInfo.ProviderIds.Imdb,
                    PremierDate = movieInfo.PremiereDate,
                    Title = movieInfo.Name,
                    Type = Store.Models.Plex.EmbyMediaType.Movie,
                    EmbyId = movieInfo.Id,
                    AddedAt = DateTime.UtcNow
                });
            }
        }
    }
}