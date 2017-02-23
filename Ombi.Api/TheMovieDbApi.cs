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
using NLog;
using NLog.Fluent;
using Ombi.Api.Models.Movie;
using RestSharp;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using Movie = TMDbLib.Objects.Movies.Movie;

namespace Ombi.Api
{
    public class TheMovieDbApi : MovieBase
    {
        public TheMovieDbApi()
        {
            Client = new TMDbClient(ApiKey);
            Api = new ApiRequest();
        }

        private ApiRequest Api { get; }
        public TMDbClient Client { get; set; }
        private const string BaseUrl = "https://api.themoviedb.org/3/";
        private static Logger Log = LogManager.GetCurrentClassLogger();
        public async Task<List<SearchMovie>> SearchMovie(string searchTerm)
        {
            var results = await Client.SearchMovie(searchTerm);
            return results?.Results ?? new List<SearchMovie>();
        }

        public async Task<List<MovieResult>> GetCurrentPlayingMovies()
        {
            var movies = await Client.GetMovieList(MovieListType.NowPlaying);
            return movies?.Results ?? new List<MovieResult>();
        }
        public async Task<List<MovieResult>> GetUpcomingMovies()
        {
            var movies = await Client.GetMovieList(MovieListType.Upcoming);
            return movies?.Results ?? new List<MovieResult>();
        }

        private async Task<Movie> GetMovie(int id)
        {
            return await Client.GetMovie(id);
        }

        public TmdbMovieDetails GetMovieInformationWithVideos(int tmdbId)
        {
            var request = new RestRequest { Resource = "movie/{movieId}", Method = Method.GET };
            request.AddUrlSegment("movieId", tmdbId.ToString());
            request.AddQueryParameter("api_key", ApiKey);
            request.AddQueryParameter("append_to_response", "videos"); // Get the videos

            try
            {

                var obj = Api.ExecuteJson<TmdbMovieDetails>(request, new Uri(BaseUrl));
                return obj;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public async Task<Movie> GetMovieInformation(int tmdbId)
        {
            var movies = await Client.GetMovie(tmdbId);
            return movies;
        }

        public async Task<Movie> GetMovieInformation(string imdbId)
        {
            var movies = await Client.GetMovie(imdbId);
            return movies ?? new Movie();
        }

        public async Task<List<Movie>> SearchActor(string searchTerm)
        {
            return await SearchActor(searchTerm, null);
        }

        public async Task<List<Movie>> SearchActor(string searchTerm, Func<int, string, string, Task<bool>> alreadyAvailable)
        {
            SearchContainer<SearchPerson> result = await Client.SearchPerson(searchTerm);
            
            var people = result?.Results ?? new List<SearchPerson>();
            var person = (people.Count != 0 ? people[0] : null);
            var movies = new List<Movie>();
            var counter = 0;
            try
            {
                if (person != null)
                {
                    var credits = await Client.GetPersonMovieCredits(person.Id);
                    //only get the first 10 movies and delay a bit between each request so we don't overload the API
                    foreach (var credit in credits.Cast)
                    {   if (counter == 10)
                            break;
                        if (alreadyAvailable == null || !(await alreadyAvailable(credit.Id, credit.Title, credit.ReleaseDate.Value.Year.ToString())))
                        {
                            movies.Add(await GetMovie(credit.Id));
                            counter++;
                        }
                        else
                        {
                            //if we didn't add the movie, delay longer since we know we'll be making additional API calls
                            await Task.Delay(75);
                        }
                        await Task.Delay(50);
                    }
                }
            }
            catch(Exception e)
            {
                Log.Log(LogLevel.Error, e);
            }
            return movies;
        }
    }
}
