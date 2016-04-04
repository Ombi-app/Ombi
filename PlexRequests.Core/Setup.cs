#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Setup.cs
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

using Mono.Data.Sqlite;
using NLog;
using PlexRequests.Api;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlexRequests.Core
{
    public class Setup
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static DbConfiguration Db { get; set; }
        public string SetupDb()
        {
            Db = new DbConfiguration(new SqliteFactory());
            var created = Db.CheckDb();
            TableCreation.CreateTables(Db.DbConnection());

            if (created)
            {
                CreateDefaultSettingsPage();
            }
            
            var version = CheckSchema();
            if (version > 0)
            {
                if (version > 1300 && version <= 1699)
                {
                    MigrateDbFrom1300();
                }
            }

            return Db.DbConnection().ConnectionString;
        }

        public static string ConnectionString => Db.DbConnection().ConnectionString;


        private int CheckSchema()
        {
            var checker = new StatusChecker();
            var status = checker.GetStatus();

            var connection = Db.DbConnection();
            var schema = connection.GetSchemaVersion();
            if (schema == null)
            {
                connection.CreateSchema(status.DBVersion); // Set the default.
                schema = connection.GetSchemaVersion();
            }

            var version = schema.SchemaVersion;
            if (version == 0)
            {
                connection.UpdateSchemaVersion(status.DBVersion);
                try
                {
                    TableCreation.AlterTable(Db.DbConnection(), "RequestBlobs", "ADD COLUMN", "MusicId", false, "TEXT");
                }
                catch (Exception e)
                {
                    Log.Error("Tried updating the schema to version 1");
                    Log.Error(e);
                    return -1;
                }
            }
            return version;
        }

        private void CreateDefaultSettingsPage()
        {
            var defaultSettings = new PlexRequestSettings
            {
                RequireTvShowApproval = true,
                RequireMovieApproval = true,
                SearchForMovies = true,
                SearchForTvShows = true,
                WeeklyRequestLimit = 0
            };
            var s = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            s.SaveSettings(defaultSettings);
        }

        public async void CacheQualityProfiles()
        {
            var mc = new MemoryCacheProvider();

            try
            {
                CacheSonarrQualityProfiles(mc);
                CacheCouchPotatoQualityProfiles(mc);
                // we don't need to cache sickrage profiles, those are static
                // TODO: cache headphones profiles?
            }
            catch (Exception)
            {
                Log.Error("Failed to cache quality profiles on startup!");
            }
        }

        private async void CacheSonarrQualityProfiles(MemoryCacheProvider cacheProvider)
        {
            var sonarrSettingsService = new SettingsServiceV2<SonarrSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var sonarrSettings = sonarrSettingsService.GetSettings();
            if (sonarrSettings.Enabled) {
                cacheProvider.GetOrSet(CacheKeys.SonarrQualityProfiles, () =>
                {
                    SonarrApi sonarrApi = new SonarrApi();
                    return sonarrApi.GetProfiles(sonarrSettings.ApiKey, sonarrSettings.FullUri);
    
                });
            }
        }

        private async void CacheCouchPotatoQualityProfiles(MemoryCacheProvider cacheProvider)
        {
            var cpSettingsService = new SettingsServiceV2<CouchPotatoSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var cpSettings = cpSettingsService.GetSettings();
            if (cpSettings.Enabled)
            {
                cacheProvider.GetOrSet(CacheKeys.CouchPotatoQualityProfiles, () =>
                {
                    CouchPotatoApi cpApi = new CouchPotatoApi();
                    return cpApi.GetProfiles(cpSettings.FullUri, cpSettings.ApiKey);

                });
            }
        }

        private void MigrateDbFrom1300() // TODO: Remove in v1.7
        {

            var result = new List<long>();
            RequestedModel[] requestedModels;
            var repo = new GenericRepository<RequestedModel>(Db, new MemoryCacheProvider());
            try
            {
                var records = repo.GetAll();
                requestedModels = records as RequestedModel[] ?? records.ToArray();
            }
            catch (SqliteException)
            {
                // There is no requested table so they do not have an old version of the DB
                return;
            }

            if (!requestedModels.Any())
            { return; }

            var jsonRepo = new JsonRequestService(new RequestJsonRepository(Db, new MemoryCacheProvider()));

            var api = new TvMazeApi();

            foreach (var r in requestedModels.Where(x => x.Type == RequestType.TvShow))
            {
                var show = api.ShowLookupByTheTvDbId(r.ProviderId);

                var model = new RequestedModel
                {
                    Title = show.name,
                    PosterPath = show.image?.medium,
                    Type = RequestType.TvShow,
                    ProviderId = show.externals.thetvdb ?? 0,
                    ReleaseDate = r.ReleaseDate,
                    AdminNote = r.AdminNote,
                    Approved = r.Approved,
                    Available = r.Available,
                    ImdbId = show.externals.imdb,
                    Issues = r.Issues,
                    OtherMessage = r.OtherMessage,
                    Overview = show.summary.RemoveHtml(),
                    RequestedUsers = r.AllUsers, // should pull in the RequestedBy property and merge with RequestedUsers
                    RequestedDate = r.ReleaseDate,
                    Status = show.status
                };
                var id = jsonRepo.AddRequest(model);
                result.Add(id);
            }

            foreach (var source in requestedModels.Where(x => x.Type == RequestType.Movie))
            {
                var id = jsonRepo.AddRequest(source);
                result.Add(id);
            }


            if (result.Any(x => x == -1))
            {
                throw new SqliteException("Could not migrate the DB!");
            }


            if (result.Count != requestedModels.Length)
            {
                throw new SqliteException("Could not migrate the DB! count is different");
            }


            // Now delete the old requests
            foreach (var oldRequest in requestedModels)
            {
                repo.Delete(oldRequest);
            }

        }
    }
}
