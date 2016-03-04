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

using Newtonsoft.Json.Linq;

using NLog;

using RestSharp;

namespace PlexRequests.Api
{
    public class CouchPotatoApi
    {
        public CouchPotatoApi()
        {
            Api = new ApiRequest();
        }
        private ApiRequest Api { get; set; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public bool AddMovie(string imdbid, string apiKey, string title, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/{apikey}/movie.add?title={title}&identifier={imdbid}" };

            request.AddUrlSegment("apikey", apiKey);
            request.AddUrlSegment("imdbid", imdbid);
            request.AddUrlSegment("title", title);

            var obj = Api.ExecuteJson<JObject>(request, baseUrl);
            Log.Trace("CP movie Add result count {0}", obj.Count);

            if (obj.Count > 0)
            {
                Log.Trace("CP movie obj[\"success\"] = {0}", obj["success"]);
                var result = (bool)obj["success"];
                Log.Trace("CP movie Add result {0}", result);
                return result;
            }
            return false;
        }
    }
}