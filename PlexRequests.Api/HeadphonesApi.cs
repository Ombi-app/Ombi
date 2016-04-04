#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: HeadphonesApi.cs
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

using Newtonsoft.Json;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Music;
using PlexRequests.Helpers;

using RestSharp;

namespace PlexRequests.Api
{
    public class HeadphonesApi : IHeadphonesApi
    {
        public HeadphonesApi()
        {
            Api = new ApiRequest();
        }
        private ApiRequest Api { get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<bool> AddAlbum(string apiKey, Uri baseUrl, string albumId)
        {
            Log.Trace("Adding album: {0}", albumId);
            var request = new RestRequest
            {
                Resource = "/api?cmd=addAlbum&id={albumId}",
                Method = Method.GET
            };

            request.AddQueryParameter("apikey", apiKey);
            request.AddUrlSegment("albumId", albumId);

            try
            {
                var result = await Task.Run(() => Api.Execute(request, baseUrl)).ConfigureAwait(false);
                Log.Trace("Add Album Result: {0}", result.DumpJson());

                var albumResult = result.Content.Equals("OK", StringComparison.CurrentCultureIgnoreCase);
                Log.Info("Album add result {0}", albumResult);

                return albumResult;
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                return false; // If there is no matching result we do not get returned a JSON string, it just returns "false".
            }
        }
        public async Task<List<HeadphonesGetIndex>> GetIndex(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest
            {
                Resource = "/api",
                Method = Method.GET
            };

            request.AddQueryParameter("apikey", apiKey);
            request.AddQueryParameter("cmd", "getIndex");

            try
            {
                var result = await Task.Run(() => Api.ExecuteJson<List<HeadphonesGetIndex>>(request, baseUrl)).ConfigureAwait(false);

                return result;
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                return new List<HeadphonesGetIndex>();
            }
        }
        public async Task<bool> AddArtist(string apiKey, Uri baseUrl, string artistId)
        {
            Log.Trace("Adding Artist: {0}", artistId);
            var request = new RestRequest
            {
                Resource = "/api",
                Method = Method.GET
            };

            request.AddQueryParameter("apikey", apiKey);
            request.AddQueryParameter("cmd", "addArtist");
            request.AddQueryParameter("id", artistId);
            
            try
            {
                var result = await Task.Run(() => Api.Execute(request, baseUrl)).ConfigureAwait(false);
                Log.Info("Add Artist Result: {0}", result.Content);
                Log.Trace("Add Artist Result: {0}", result.DumpJson());
                return result.Content.Equals("OK", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                return false; // If there is no matching result we do not get returned a JSON string, it just returns "false".
            }
        }
        public async Task<bool> QueueAlbum(string apiKey, Uri baseUrl, string albumId)
        {
            Log.Trace("Queing album: {0}", albumId);
            var request = new RestRequest
            {
                Resource = "/api?cmd=queueAlbum&id={albumId}",
                Method = Method.GET
            };

            request.AddQueryParameter("apikey", apiKey);
            request.AddUrlSegment("albumId", albumId);

            try
            {
                var result = await Task.Run(() => Api.Execute(request, baseUrl)).ConfigureAwait(false);
                Log.Info("Queue Result: {0}", result.Content);
                Log.Trace("Queue Result: {0}", result.DumpJson());
                return result.Content.Equals("OK", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                return false; // If there is no matching result we do not get returned a JSON string, it just returns "false".
            }
        }

        public async Task<bool> RefreshArtist(string apiKey, Uri baseUrl, string artistId)
        {
            Log.Trace("Refreshing artist: {0}", artistId);
            var request = new RestRequest
            {
                Resource = "/api",
                Method = Method.GET
            };

            request.AddQueryParameter("apikey", apiKey);
            request.AddQueryParameter("cmd", "queueAlbum");
            request.AddQueryParameter("id", artistId);

            try
            {
                var result = await Task.Run(() => Api.Execute(request, baseUrl)).ConfigureAwait(false);
                Log.Info("Artist refresh Result: {0}", result.Content);
                Log.Trace("Artist refresh Result: {0}", result.DumpJson());
                return result.Content.Equals("OK", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                return false; // If there is no matching result we do not get returned a JSON string, it just returns "false".
            }
        }

        public HeadphonesVersion GetVersion(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest
            {
                Resource = "/api",
                Method = Method.GET
            };

            request.AddQueryParameter("apikey", apiKey);
            request.AddQueryParameter("cmd", "getVersion");

            try
            {
                return Api.ExecuteJson<HeadphonesVersion>(request, baseUrl);
            }
            catch (JsonSerializationException jse)
            {
                Log.Error(jse);
                return new HeadphonesVersion(); // If there is no matching result we do not get returned a JSON string, it just returns "false".
            }
        }
    }
}