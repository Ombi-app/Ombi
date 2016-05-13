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
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Sonarr;
using PlexRequests.Helpers;

using RestSharp;
using Newtonsoft.Json.Linq;

using PlexRequests.Helpers.Exceptions;

namespace PlexRequests.Api
{
    public class SonarrApi : ISonarrApi
    {
        public SonarrApi()
        {
            Api = new ApiRequest();
        }
        private ApiRequest Api { get; set; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public List<SonarrProfile> GetProfiles(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/profile", Method = Method.GET };

            request.AddHeader("X-Api-Key", apiKey);
			var policy = RetryHandler.RetryAndWaitPolicy (new TimeSpan[] { 
				TimeSpan.FromSeconds (2),
				TimeSpan.FromSeconds(5),
				TimeSpan.FromSeconds(10)
			}, (exception, timespan) => Log.Error (exception, "Exception when calling GetProfiles for Sonarr, Retrying {0}", timespan));

			var obj = policy.Execute(() => Api.ExecuteJson<List<SonarrProfile>>(request, baseUrl));

            return obj;
        }

        public SonarrAddSeries AddSeries(int tvdbId, string title, int qualityId, bool seasonFolders, string rootPath, int seasonCount, int[] seasons, string apiKey, Uri baseUrl)
        {
            Log.Debug("Adding series {0}", title);
            Log.Debug("Seasons = {0}, out of {1} seasons", seasons.DumpJson(), seasonCount);
            var request = new RestRequest
            {
                Resource = "/api/Series?",
                Method = Method.POST
            };

            var options = new SonarrAddSeries
            {
                seasonFolder = seasonFolders,
                title = title,
                qualityProfileId = qualityId,
                tvdbId = tvdbId,
                titleSlug = title,
                seasons = new List<Season>(),
                rootFolderPath = rootPath
            };

            for (var i = 1; i <= seasonCount; i++)
            {
                var season = new Season
                {
                    seasonNumber = i,
                    monitored = seasons.Length == 0 || seasons.Any(x => x == i)
                };
                options.seasons.Add(season);
            }

            Log.Debug("Sonarr API Options:");
            Log.Debug(options.DumpJson());

            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(options);

            SonarrAddSeries result;
            try
            {
				var policy = RetryHandler.RetryAndWaitPolicy (new TimeSpan[] { 
					TimeSpan.FromSeconds (2),
					TimeSpan.FromSeconds(5),
					TimeSpan.FromSeconds(10)
				}, (exception, timespan) => Log.Error (exception, "Exception when calling AddSeries for Sonarr, Retrying {0}", timespan));

				result = policy.Execute(() => Api.ExecuteJson<SonarrAddSeries>(request, baseUrl));
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                var error = Api.ExecuteJson<List<SonarrError>>(request, baseUrl);
                var messages = error?.Select(x => x.errorMessage).ToList();
                messages?.ForEach(x => Log.Error(x));
                result = new SonarrAddSeries { ErrorMessages = messages };
            }

            return result;
        }

        public SystemStatus SystemStatus(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/system/status", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);

			var policy = RetryHandler.RetryAndWaitPolicy (new TimeSpan[] { 
				TimeSpan.FromSeconds (2),
				TimeSpan.FromSeconds(5),
				TimeSpan.FromSeconds(10)
			}, (exception, timespan) => Log.Error (exception, "Exception when calling SystemStatus for Sonarr, Retrying {0}", timespan));

			var obj = policy.Execute(() => Api.ExecuteJson<SystemStatus>(request, baseUrl));

            return obj;
        }

        public List<Series> GetSeries(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/series", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);
            try
            {
				var policy = RetryHandler.RetryAndWaitPolicy (new TimeSpan[] { 
					TimeSpan.FromSeconds (5),
					TimeSpan.FromSeconds(10),
					TimeSpan.FromSeconds(30)
				}, (exception, timespan) => Log.Error (exception, "Exception when calling GetSeries for Sonarr, Retrying {0}", timespan));

				return policy.Execute(() => Api.ExecuteJson<List<Series>>(request, baseUrl));
			}
            catch (ApiRequestException)
            {
                Log.Error("There has been an API exception when getting the Sonarr Series");
                return null;
            }
        }
    }
}