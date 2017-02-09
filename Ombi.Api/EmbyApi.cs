﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: EmbyApi.cs
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
using System.Net;
using Newtonsoft.Json;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Helpers;
using RestSharp;

namespace Ombi.Api
{
    public class EmbyApi : IEmbyApi
    {
        public EmbyApi()
        {
            Api = new ApiRequest();
        }

        private ApiRequest Api { get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns all users from the Emby Instance
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="apiKey"></param>
        public List<EmbyUser> GetUsers(Uri baseUri, string apiKey)
        {
            var request = new RestRequest
            {
                Resource = "emby/users",
                Method = Method.GET
            };

            AddHeaders(request, apiKey);

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetUsers for Emby, Retrying {0}", timespan), new[] {
                TimeSpan.FromSeconds (1),
                TimeSpan.FromSeconds(5)
            });

            var obj = policy.Execute(() => Api.ExecuteJson<List<EmbyUser>>(request, baseUri));

            return obj;
        }

        public EmbyItemContainer<EmbyLibrary> ViewLibrary(string apiKey, string userId, Uri baseUri)
        {
            var request = new RestRequest
            {
                Resource = "emby/users/{userId}/items",
                Method = Method.GET
            };

            request.AddUrlSegment("userId", userId);
            AddHeaders(request, apiKey);

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling ViewLibrary for Emby, Retrying {0}", timespan), new[] {
                TimeSpan.FromSeconds (1),
                TimeSpan.FromSeconds(5)
            });

            var obj = policy.Execute(() => Api.ExecuteJson<EmbyItemContainer<EmbyLibrary>>(request, baseUri));

            return obj;
        }

        public EmbyItemContainer<EmbyMovieItem> GetAllMovies(string apiKey, string userId, Uri baseUri)
        {
            return GetAll<EmbyMovieItem>("Movie", apiKey, userId, baseUri);
        }

        public EmbyItemContainer<EmbyEpisodeItem> GetAllEpisodes(string apiKey, string userId, Uri baseUri)
        {
            return GetAll<EmbyEpisodeItem>("Episode", apiKey, userId, baseUri);
        }

        public EmbyItemContainer<EmbyMovieInformation> GetCollection(string mediaId, string apiKey, string userId, Uri baseUrl)
        {
            var request = new RestRequest
            {
                Resource = "emby/users/{userId}/items?parentId={mediaId}",
                Method = Method.GET
            };

            request.AddUrlSegment("userId", userId);
            request.AddUrlSegment("mediaId", mediaId);

            AddHeaders(request, apiKey);


            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetCollections for Emby, Retrying {0}", timespan), new[] {
                TimeSpan.FromSeconds (1),
                TimeSpan.FromSeconds(5)
            });
            return policy.Execute(() => Api.ExecuteJson<EmbyItemContainer<EmbyMovieInformation>>(request, baseUrl));
        }

        public EmbyInformation GetInformation(string mediaId, EmbyMediaType type, string apiKey, string userId, Uri baseUri)
        {
            var request = new RestRequest
            {
                Resource = "emby/users/{userId}/items/{mediaId}",
                Method = Method.GET
            };

            request.AddUrlSegment("userId", userId);
            request.AddUrlSegment("mediaId", mediaId);

            AddHeaders(request, apiKey);


            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetAll<T>({1}) for Emby, Retrying {0}", timespan, type), new[] {
                TimeSpan.FromSeconds (1),
                TimeSpan.FromSeconds(5)
            });

            switch (type)
            {
                case EmbyMediaType.Movie:
                    return new EmbyInformation
                    {
                        MovieInformation = policy.Execute(() => Api.ExecuteJson<EmbyMovieInformation>(request, baseUri))
                    };
                case EmbyMediaType.Series:
                    return new EmbyInformation
                    {
                        SeriesInformation =
                            policy.Execute(() => Api.ExecuteJson<EmbySeriesInformation>(request, baseUri))
                    };
                case EmbyMediaType.Music:
                    break;
                case EmbyMediaType.Episode:
                    return new EmbyInformation
                    {
                        EpisodeInformation = 
                            policy.Execute(() => Api.ExecuteJson<EmbyEpisodeInformation>(request, baseUri))
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return new EmbyInformation();
        }


        public EmbyItemContainer<EmbySeriesItem> GetAllShows(string apiKey, string userId, Uri baseUri)
        {
            return GetAll<EmbySeriesItem>("Series", apiKey, userId, baseUri);
        }

        public EmbyUser LogIn(string username, string password, string apiKey, Uri baseUri)
        {
            var request = new RestRequest
            {
                Resource = "emby/users/authenticatebyname",
                Method = Method.POST
            };

            var body = new
            {
                username,
                password = StringHasher.GetSha1Hash(password).ToLower(),
                passwordMd5 = StringHasher.CalcuateMd5Hash(password)
            };

            request.AddJsonBody(body);

            request.AddHeader("X-Emby-Authorization",
                $"MediaBrowser Client=\"Ombi\", Device=\"Ombi\", DeviceId=\"{AssemblyHelper.GetProductVersion()}\", Version=\"{AssemblyHelper.GetAssemblyVersion()}\"");
            AddHeaders(request, apiKey);


            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling LogInfor Emby, Retrying {0}", timespan), new[] {
                TimeSpan.FromSeconds (1),
                TimeSpan.FromSeconds(5)
            });

            var obj = policy.Execute(() => Api.Execute(request, baseUri));

            if (obj.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<EmbyUserLogin>(obj.Content)?.User;
        }

        private EmbyItemContainer<T> GetAll<T>(string type, string apiKey, string userId, Uri baseUri)
        {
            var request = new RestRequest
            {
                Resource = "emby/users/{userId}/items",
                Method = Method.GET
            };

            request.AddUrlSegment("userId", userId);
            request.AddQueryParameter("Recursive", true.ToString());
            request.AddQueryParameter("IncludeItemTypes", type);

            AddHeaders(request, apiKey);


            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetAll<T>({1}) for Emby, Retrying {0}", timespan, type), new[] {
                TimeSpan.FromSeconds (1),
                TimeSpan.FromSeconds(5)
            });

            var obj = policy.Execute(() => Api.ExecuteJson<EmbyItemContainer<T>>(request, baseUri));

            return obj;
        }


        private static void AddHeaders(IRestRequest req, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                req.AddHeader("X-MediaBrowser-Token", apiKey);
            }
            req.AddHeader("Accept", "application/json");
            req.AddHeader("Content-Type", "application/json");
            req.AddHeader("Device", "Ombi");
        }
    }
}