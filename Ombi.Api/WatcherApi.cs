#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: WatcherApi.cs
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
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Watcher;
using RestSharp;

namespace Ombi.Api
{
    public class WatcherApi : IWatcherApi
    {
        public WatcherApi()
        {
            Api = new ApiRequest();
        }

        private ApiRequest Api { get; set; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public WatcherAddMovieResult AddMovie(string imdbId, string apiKey, Uri baseUrl)
        {
            var response =  Send("addmovie", apiKey, baseUrl, imdbId);
            try
            {
                return JsonConvert.DeserializeObject<WatcherAddMovieResult>(response.Content);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new WatcherAddMovieResult
                {
                    Error = true,
                    ErrorMessage = e.Message
                };
            }
        }

        public WatcherListStatusResultContainer ListMovies(string apiKey, Uri baseUrl)
        {
            var response = Send("liststatus", apiKey, baseUrl);
            try
            {
                if (response.Content.Contains("No movies found"))
                {
                    return new WatcherListStatusResultContainer();
                }
                var items = JsonConvert.DeserializeObject<List<WatcherListStatusResult>>(response.Content);

                return new WatcherListStatusResultContainer {Results = items};
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new WatcherListStatusResultContainer
                {
                    Error = true,
                    ErrorMessage = e.Message
                };
            }
        }

        public WatcherListStatusResultContainer ListMovies(string apiKey, Uri baseUrl, string imdbId)
        {
            var response = Send("liststatus", apiKey, baseUrl, imdbId);
            try
            {
                if (response.Content.Contains("No movies found"))
                {
                    return new WatcherListStatusResultContainer();
                }
                var items = JsonConvert.DeserializeObject<List<WatcherListStatusResult>>(response.Content);

                return new WatcherListStatusResultContainer { Results = items };
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new WatcherListStatusResultContainer
                {
                    Error = true,
                    ErrorMessage = e.Message
                };
            }
        }


        private IRestResponse Send(string mode, string apiKey, Uri baseUrl, string imdbid = "")
        {
            RestRequest request;
            request = new RestRequest
            {
                Resource = string.IsNullOrEmpty(imdbid) ? "/api?apikey={apikey}&mode={mode}" : "/api?apikey={apikey}&mode={mode}&imdbid={imdbid}"
            };

            
            request.AddUrlSegment("apikey", apiKey);
            if (!string.IsNullOrEmpty(imdbid))
            {
                request.AddUrlSegment("imdbid", imdbid);
            }
            request.AddUrlSegment("mode", mode);

            return Api.Execute(request, baseUrl);
        }
    }

}