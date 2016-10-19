#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TheMovieDbApi.cs
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
using System.Threading.Tasks;

using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace PlexRequests.Api
{
    public class TheMovieDbApi : MovieBase
    {
        public TheMovieDbApi()
        {
            Client = new TMDbClient(ApiKey);
        }

        public TMDbClient Client { get; set; }
        public async Task<List<SearchMovie>> SearchMovie(string searchTerm)
        {
            var results = await Client.SearchMovieAsync(searchTerm);
            return results.Results;
        }

        [Obsolete("Should use TvMaze for TV")]
        public async Task<List<SearchTv>> SearchTv(string searchTerm)
        {
            var results = await Client.SearchTvShowAsync(searchTerm);
            return results.Results;
        }

        public async Task<List<SearchMovie>> GetCurrentPlayingMovies()
        {
            var movies = await Client.GetMovieNowPlayingListAsync();
            return movies.Results;
        }
        public async Task<List<SearchMovie>> GetUpcomingMovies()
        {
            var movies = await Client.GetMovieUpcomingListAsync();
            return movies.Results;
        }

        public async Task<Movie> GetMovieInformation(int tmdbId)
        {
            var movies = await Client.GetMovieAsync(tmdbId);
            return movies;
        }

        public async Task<Movie> GetMovieInformation(string imdbId)
        {
            var movies = await Client.GetMovieAsync(imdbId);
            return movies;
        }

        [Obsolete("Should use TvMaze for TV")]
        public async Task<TvShow> GetTvShowInformation(int tmdbId)
        {
            var show = await Client.GetTvShowAsync(tmdbId);
            return show;
        }
    }
}
