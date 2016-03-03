#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SearchModule.cs
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
using Nancy;
using Nancy.Responses.Negotiation;

using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class SearchModule : NancyModule
    {
        public SearchModule(ICacheProvider cache) : base("search")
        {
            MovieApi = new TheMovieDbApi();
            TvApi = new TheTvDbApi();
            Cache = cache;
            
            Get["/"] = parameters => RequestLoad();

            Get["movie/{searchTerm}"] = parameters => SearchMovie((string)parameters.searchTerm);
            Get["tv/{searchTerm}"] = parameters => SearchTvShow((string)parameters.searchTerm);

            Get["movie/upcoming"] = parameters => UpcomingMovies();
            Get["movie/playing"] = parameters => CurrentlyPlayingMovies();

            Post["request/movie"] = parameters => RequestMovie((int)Request.Form.movieId);
            Post["request/tv"] = parameters => RequestTvShow((int)Request.Form.tvId, (bool)Request.Form.latest);
        }
        private TheMovieDbApi MovieApi { get; }
        private TheTvDbApi TvApi { get; }
        private ICacheProvider Cache { get; }
        private string AuthToken => Cache.GetOrSet(CacheKeys.TvDbToken, TvApi.Authenticate, 50);

        private Negotiator RequestLoad()
        { 
            return View["Search/Index"];
        }

        private Response SearchMovie(string searchTerm)
        {
            var movies = MovieApi.SearchMovie(searchTerm);
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response SearchTvShow(string searchTerm)
        {
            var tvShow = TvApi.SearchTv(searchTerm, AuthToken);

            if (tvShow?.data == null)
            {
                return Response.AsJson("");
            }
            var model = new List<SearchTvShowViewModel>();

            foreach (var t in tvShow.data)
            {
                model.Add(new SearchTvShowViewModel
                {
                    Added = t.added,
                    AirsDayOfWeek = t.airsDayOfWeek,
                    AirsTime = t.airsTime,
                    Aliases = t.aliases,
                    // We are constructing the banner with the id: 
                    // http://thetvdb.com/banners/_cache/posters/ID-1.jpg
                    Banner = t.id.ToString(), 
                    FirstAired = t.firstAired,
                    Genre = t.genre,
                    Id = t.id,
                    ImdbId = t.imdbId,
                    LastUpdated = t.lastUpdated,
                    Network = t.network,
                    NetworkId = t.networkId,
                    Overview = t.overview,
                    Rating = t.rating,
                    Runtime = t.runtime,
                    SeriesId = t.id,
                    SeriesName = t.seriesName,
                    SiteRating = t.siteRating,
                    Status = t.status,
                    Zap2ItId = t.zap2itId
                });
            }
            return Response.AsJson(model);
        }

        private Response UpcomingMovies()
        {
            var movies = MovieApi.GetUpcomingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response CurrentlyPlayingMovies()
        {
            var movies = MovieApi.GetCurrentPlayingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response RequestMovie(int movieId)
        {
            var s = new SettingsService(Cache);
            if (s.CheckRequest(movieId))
            {
                return Response.AsJson(new { Result = false, Message = "Movie has already been requested!" });
            }
            
            s.AddRequest(movieId, RequestType.Movie);
            return Response.AsJson(new { Result = true });
        }

        /// <summary>
        /// Requests the tv show.
        /// </summary>
        /// <param name="showId">The show identifier.</param>
        /// <param name="latest">if set to <c>true</c> [latest].</param>
        /// <returns></returns>
        private Response RequestTvShow(int showId, bool latest)
        {
            // Latest send to Sonarr and no need to store in DB
            var s = new SettingsService(Cache);
            if (s.CheckRequest(showId))
            {
                return Response.AsJson(new { Result = false, Message = "TV Show has already been requested!" });
            }
            s.AddRequest(showId, RequestType.TvShow);
            return Response.AsJson(new {Result = true });
        }
    }
}