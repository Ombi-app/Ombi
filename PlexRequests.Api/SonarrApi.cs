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
            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetProfiles for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                TimeSpan.FromSeconds (2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

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
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling AddSeries for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                    TimeSpan.FromSeconds (2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                });

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

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling SystemStatus for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                TimeSpan.FromSeconds (2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

            var obj = policy.Execute(() => Api.ExecuteJson<SystemStatus>(request, baseUrl));

            return obj;
        }

        public List<Series> GetSeries(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/series", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);
            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetSeries for Sonarr, Retrying {0}", timespan), new TimeSpan[] {
                    TimeSpan.FromSeconds (5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30)
                });

                return policy.Execute(() => Api.ExecuteJson<List<Series>>(request, baseUrl));
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been an API exception when getting the Sonarr Series");
                return null;
            }
        }

        public Series GetSeries(string seriesId, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/series/{seriesId}", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);
            request.AddUrlSegment("seriesId", seriesId);
            try
            {
                var policy =
                    RetryHandler.RetryAndWaitPolicy(
                        (exception, timespan) =>
                            Log.Error(exception, "Exception when calling GetSeries by ID for Sonarr, Retrying {0}",
                                timespan));

                return policy.Execute(() => Api.ExecuteJson<Series>(request, baseUrl));
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been an API exception when getting the Sonarr Series by ID");
                return null;
            }
        }

        /// <summary>
        /// Returns all episodes for the given series.
        /// </summary>
        /// <param name="seriesId">The series identifier.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        public IEnumerable<SonarrEpisodes> GetEpisodes(string seriesId, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/Episode", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);
            request.AddQueryParameter("seriesId", seriesId);
            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) =>
                    Log.Error(exception, "Exception when calling GetEpisodes for Sonarr, Retrying {0}", timespan));

                return policy.Execute(() => Api.ExecuteJson<List<SonarrEpisodes>>(request, baseUrl));
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been an API exception when getting the Sonarr GetEpisodes");
                return null;
            }
        }

        /// <summary>
        /// Returns the episode with the matching id.
        /// </summary>
        /// <param name="episodeId">The episode identifier.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        public SonarrEpisode GetEpisode(string episodeId, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/Episode/{episodeId}", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);
            request.AddUrlSegment("episodeId", episodeId);
            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) =>
                    Log.Error(exception, "Exception when calling GetEpisode by ID for Sonarr, Retrying {0}", timespan));

                return policy.Execute(() => Api.ExecuteJson<SonarrEpisode>(request, baseUrl));
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been an API exception when getting the Sonarr GetEpisode by ID");
                return null;
            }
        }

        /// <summary>
        /// Update the given episodes, currently only monitored is changed, all other modifications are ignored.
        /// Required: All parameters (you should perform a GET/{id} and submit the full body with the changes, as other values may be editable in the future.
        /// </summary>
        /// <param name="episodeInfo">The episode information.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        public SonarrEpisode UpdateEpisode(SonarrEpisode episodeInfo, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/Episode", Method = Method.PUT };
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(episodeInfo);
            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) =>
                    Log.Error(exception, "Exception when calling UpdateEpisode for Sonarr, Retrying {0}", timespan));

                return policy.Execute(() => Api.ExecuteJson<SonarrEpisode>(request, baseUrl));
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been an API exception when put the Sonarr UpdateEpisode");
                return null;
            }
        }

        /// <summary>
        /// Search for one or more episodes
        /// </summary>
        /// <param name="episodeIds">The episode ids.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        public SonarrAddEpisodeResult SearchForEpisodes(int[] episodeIds, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/Command", Method = Method.POST };
            request.AddHeader("X-Api-Key", apiKey);

            var body = new SonarrAddEpisodeBody
            {
                name = "EpisodeSearch",
                episodeIds = episodeIds
            };

            request.AddJsonBody(body);

            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) =>
                    Log.Error(exception, "Exception when calling SearchForEpisodes for Sonarr, Retrying {0}", timespan));

                return policy.Execute(() => Api.ExecuteJson<SonarrAddEpisodeResult>(request, baseUrl));
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been an API exception when put the Sonarr SearchForEpisodes");
                return null;
            }
        }
    }
}