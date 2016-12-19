#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiRequest.cs
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
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Helpers.Exceptions;
using RestSharp;

namespace Ombi.Api
{
    public class ApiRequest : IApiRequest
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private static Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// An API request handler
        /// </summary>
        /// <typeparam name="T">The type of class you want to deserialize</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <returns>The type of class you want to deserialize</returns>
        public T Execute<T>(IRestRequest request, Uri baseUri) where T : new()
        {
            var client = new RestClient { BaseUrl = baseUri };

            var response = client.Execute<T>(request);
            Log.Trace("Api Content Response:");
            Log.Trace(response.Content);


            if (response.ErrorException != null)
            {
                var message = "Error retrieving response. Check inner details for more info.";
                Log.Error(response.ErrorException);
                throw new ApiRequestException(message, response.ErrorException);
            }

            return response.Data;
        }
        
        public IRestResponse Execute(IRestRequest request, Uri baseUri)
        {
            var client = new RestClient { BaseUrl = baseUri };

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                Log.Error(response.ErrorException);
                var message = "Error retrieving response. Check inner details for more info.";
                throw new ApiRequestException(message, response.ErrorException);
            }

            return response;
        }

        public T ExecuteXml<T>(IRestRequest request, Uri baseUri) where T : class
        {
            var client = new RestClient { BaseUrl = baseUri };

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                Log.Error(response.ErrorException);
                var message = "Error retrieving response. Check inner details for more info.";
                throw new ApiRequestException(message, response.ErrorException);
            }

            var result = DeserializeXml<T>(response.Content);
            return result;}

        public T ExecuteJson<T>(IRestRequest request, Uri baseUri) where T : new()
        {
            var client = new RestClient { BaseUrl = baseUri };
            var response = client.Execute(request);
            Log.Trace("Api Content Response:");
            Log.Trace(response.Content);
            if (response.ErrorException != null)
            {
                Log.Error(response.ErrorException);
                var message = "Error retrieving response. Check inner details for more info.";
                throw new ApiRequestException(message, response.ErrorException);
            }

            Log.Trace("Deserialzing Object");
            var json = JsonConvert.DeserializeObject<T>(response.Content, _settings);
            Log.Trace("Finished Deserialzing Object");

            return json;
        }

        private T DeserializeXml<T>(string input)
             where T : class
        {
            var ser = new XmlSerializer(typeof(T));

            try
            {
                using (var sr = new StringReader(input))
                    return (T)ser.Deserialize(sr);
            }
            catch (InvalidOperationException e)
            {
                Log.Error(e);
                return null;
            }
        }
    }

   
}
