#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CouchPotatoApi.cs
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
using Newtonsoft.Json;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Radarr;
using Ombi.Api.Models.Sonarr;
using Ombi.Helpers;
using RestSharp;

namespace Ombi.Api
{
    public class RadarrApi : IRadarrApi
    {
        public RadarrApi()
        {
            Api = new ApiRequest();
        }
        private ApiRequest Api { get; set; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public List<SonarrProfile> GetProfiles(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/profile", Method = Method.GET };

            request.AddHeader("X-Api-Key", apiKey);
            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetProfiles for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                TimeSpan.FromSeconds (2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

            var obj = policy.Execute(() => Api.ExecuteJson<List<SonarrProfile>>(request, baseUrl));

            return obj;
        }

        public RadarrAddMovie AddMovie(int tmdbId, string title, int year, int qualityId, string rootPath, string apiKey, Uri baseUrl, bool searchNow = false)
        {
            var request = new RestRequest
            {
                Resource = "/api/movie",
                Method = Method.POST
            };

            var options = new RadarrAddMovie
            {
                title = title,
                tmdbId = tmdbId,
                qualityProfileId = qualityId,
                rootFolderPath = rootPath,
                titleSlug = title,
                monitored = true,
                year = year
            };

            if (searchNow)
            {
                options.addOptions = new RadarrAddOptions
                {
                    searchForMovie = true
                };
            }


            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(options);

            RadarrAddMovie result;
            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling AddSeries for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                    TimeSpan.FromSeconds (2)
                });

                var response = policy.Execute(() => Api.Execute(request, baseUrl));
                if (response.Content.Contains("\"message\":"))
                {
                    var error = JsonConvert.DeserializeObject < RadarrError>(response.Content);
                    return new RadarrAddMovie {Error =  error};
                }
                if (response.Content.Contains("\"errorMessage\":"))
                {
                    var error = JsonConvert.DeserializeObject<List<SonarrError>>(response.Content).FirstOrDefault();
                    return new RadarrAddMovie {Error = new RadarrError {message = error?.errorMessage}};
                }
                return JsonConvert.DeserializeObject < RadarrAddMovie>(response.Content);
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
            }
            return null;
        }

     
        public SystemStatus SystemStatus(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/system/status", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling SystemStatus for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                TimeSpan.FromSeconds (2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

            var obj = policy.Execute(() => Api.ExecuteJson<SystemStatus>(request, baseUrl));

            return obj;
        }


        public List<RadarrMovieResponse> GetMovies(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/movie", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling SystemStatus for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                TimeSpan.FromSeconds (2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

            var obj = policy.Execute(() => Api.Execute(request, baseUrl));

            return JsonConvert.DeserializeObject<List<RadarrMovieResponse>>(obj.Content);
        }
    }
}