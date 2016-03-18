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
using System.Collections.Generic;
using System.Linq;

using Mono.Data.Sqlite;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Repository;

namespace PlexRequests.Core
{
    public class Setup
    {
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

            MigrateDb();
            return Db.DbConnection().ConnectionString;
        }

        public static string ConnectionString => Db.DbConnection().ConnectionString;

        private void CreateDefaultSettingsPage()
        {
            var defaultSettings = new PlexRequestSettings
            {
                RequireApproval = true,
                SearchForMovies = true,
                SearchForTvShows = true,
                WeeklyRequestLimit = 0
            };
            var s = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            s.SaveSettings(defaultSettings);
        }

        private void MigrateDb() // TODO: Remove when no longer needed
        {
            var repo = new GenericRepository<RequestedModel>(Db);
            var records = repo.GetAll();
            var requestedModels = records as RequestedModel[] ?? records.ToArray();
            if(!requestedModels.Any())
            { return; }

            var jsonRepo = new JsonRequestService(new RequestJsonRepository(Db, new MemoryCacheProvider()));
            var result = new List<long>();
            foreach (var r in requestedModels)
            {
                var id = jsonRepo.AddRequest(r);
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
