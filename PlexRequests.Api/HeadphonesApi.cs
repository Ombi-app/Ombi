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

using Newtonsoft.Json;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Music;

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

        public bool AddAlbum(string apiKey, Uri baseUrl, string albumId)
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
                //var result = Api.Execute<string>(request, baseUrl);
                return false;
            }
            catch (JsonSerializationException jse)
            {
                Log.Warn(jse);
                return false; // If there is no matching result we do not get returned a JSON string, it just returns "false".
            }
        }
    }
}