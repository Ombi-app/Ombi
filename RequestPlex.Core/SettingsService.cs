#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SettingsService.cs
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
using System.Linq;

using Mono.Data.Sqlite;

using RequestPlex.Api;
using RequestPlex.Store;

namespace RequestPlex.Core
{
    public class SettingsService
    {

        public SettingsModel GetSettings()
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<SettingsModel>(db);

            var settings = repo.GetAll().FirstOrDefault();

            return settings;
        }

        public void AddRequest(int tmdbid, RequestType type)
        {
            var api = new TheMovieDbApi();
            var model = new RequestedModel();
            if (type == RequestType.Movie)
            {
                var movieInfo = api.GetMovieInformation(tmdbid).Result;

                model = new RequestedModel
                {
                    Tmdbid = movieInfo.Id,
                    Type = type,
                    Overview = movieInfo.Overview,
                    ImdbId = movieInfo.ImdbId,
                    PosterPath = "http://image.tmdb.org/t/p/w150/" + movieInfo.PosterPath,
                    Title = movieInfo.Title,
                    ReleaseDate = movieInfo.ReleaseDate ?? DateTime.MinValue,
                    Status = movieInfo.Status,
                    RequestedDate = DateTime.Now,
                    Approved = false
                };
            }
            else
            {
                var showInfo = api.GetTvShowInformation(tmdbid).Result;

                model = new RequestedModel
                {
                    Tmdbid = showInfo.Id,
                    Type = type,
                    Overview = showInfo.Overview,
                    PosterPath = "http://image.tmdb.org/t/p/w150/" + showInfo.PosterPath,
                    Title = showInfo.Name,
                    ReleaseDate = showInfo.FirstAirDate ?? DateTime.MinValue,
                    Status = showInfo.Status,
                    RequestedDate = DateTime.Now,
                    Approved = false
                };
            }
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);

            repo.Insert(model);
        }

        public bool CheckRequest(int tmdbid)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);

            return repo.GetAll().Any(x => x.Tmdbid == tmdbid);
        }

        public void DeleteRequest(int tmdbId)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);
            var entity = repo.GetAll().FirstOrDefault(x => x.Tmdbid == tmdbId);
            repo.Delete(entity);
        }

    }
}
