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

using PlexRequests.Api;
using PlexRequests.Helpers;
using PlexRequests.Store;

namespace PlexRequests.Core
{
    public class SettingsService
    {

        public SettingsModel GetSettings(ICacheProvider cache)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<SettingsModel>(db);

            var settings = repo.GetAll().FirstOrDefault();

            return settings;
        }
        private ICacheProvider Cache { get; set; }

        public void AddRequest(int providerId, RequestType type)
        {

            var model = new RequestedModel();
            if (type == RequestType.Movie)
            {
                var movieApi = new TheMovieDbApi();
                var movieInfo = movieApi.GetMovieInformation(providerId).Result;

                model = new RequestedModel
                {
                    ProviderId = movieInfo.Id,
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
                var tvApi = new TheTvDbApi();
                var token = GetAuthToken(tvApi);

                var showInfo = tvApi.GetInformation(providerId, token);

                DateTime firstAir;
                DateTime.TryParse(showInfo.firstAired, out firstAir);

                model = new RequestedModel
                {
                    ProviderId = showInfo.id,
                    Type = type,
                    Overview = showInfo.overview,
                    PosterPath = "http://image.tmdb.org/t/p/w150/" + showInfo.banner, // This is incorrect
                    Title = showInfo.seriesName,
                    ReleaseDate = firstAir,
                    Status = showInfo.status,
                    RequestedDate = DateTime.Now,
                    Approved = false
                };
            }
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);

            repo.Insert(model);
        }

        public bool CheckRequest(int providerId)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);

            return repo.GetAll().Any(x => x.ProviderId == providerId);
        }

        public void DeleteRequest(int tmdbId)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);
            var entity = repo.GetAll().FirstOrDefault(x => x.ProviderId == tmdbId);
            repo.Delete(entity);
        }

        private string GetAuthToken(TheTvDbApi api)
        {
            return Cache.GetOrSet(CacheKeys.TvDbToken, api.Authenticate, 50);
        }
    }
}
