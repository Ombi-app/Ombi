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
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models;

using RestSharp;

namespace PlexRequests.Api
{
    public class ApiRequest : IApiRequest
    {
        
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

            if (response.ErrorException != null)
            { 
                var message = "Error retrieving response. Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            return response.Data;

        }

        public T ExecuteXml<T>(IRestRequest request, Uri baseUri) where T : class
        {
            var client = new RestClient { BaseUrl = baseUri };

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                var message = "Error retrieving response. Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            return Deserialize<T>(response.Content);
        }

        public T Deserialize<T>(string input)
             where T : class
        {
            var ser = new XmlSerializer(typeof(T));

            using (var sr = new StringReader(input))
                return (T)ser.Deserialize(sr);
        }
    }
}
