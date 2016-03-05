#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexApi.cs
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

using PlexRequests.Api.Models;
using PlexRequests.Helpers;

using RestSharp;

namespace PlexRequests.Api
{
    public class PlexApi
    {
        static PlexApi()
        {
            Version = AssemblyHelper.GetAssemblyVersion();
        }
        private static string Version { get; set; }

        public PlexAuthentication GetToken(string username, string password)
        {
            var userModel = new PlexUserRequest
            {
                user = new UserRequest
                {
                    password = password,
                    login = username
                }
            };
            var request = new RestRequest
            {
                Method = Method.POST
            };

            request.AddHeader("X-Plex-Client-Identifier", "Test213"); // TODO need something unique to the users version/installation
            request.AddHeader("X-Plex-Product", "Request Plex");
            request.AddHeader("X-Plex-Version", Version);
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(userModel);

            var api = new ApiRequest();
            return api.Execute<PlexAuthentication>(request, new Uri("https://plex.tv/users/sign_in.json"));
        }

        public PlexFriends GetUsers(string authToken)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
            };

            request.AddHeader("X-Plex-Client-Identifier", "Test213");
            request.AddHeader("X-Plex-Product", "Request Plex");
            request.AddHeader("X-Plex-Version", Version);
            request.AddHeader("X-Plex-Token", authToken);
            request.AddHeader("Content-Type", "application/xml");

            var api = new ApiRequest();
            var users = api.ExecuteXml<PlexFriends>(request, new Uri("https://plex.tv/pms/friends/all"));

            return users;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="plexFullHost">The full plex host.</param>
        /// <returns></returns>
        public PlexSearch SearchContent(string authToken, string searchTerm, Uri plexFullHost)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "search?query={searchTerm}"
            };

            request.AddUrlSegment("searchTerm", searchTerm);
            request.AddHeader("X-Plex-Client-Identifier", "Test213");
            request.AddHeader("X-Plex-Product", "Request Plex");
            request.AddHeader("X-Plex-Version", Version);
            request.AddHeader("X-Plex-Token", authToken);
            request.AddHeader("Content-Type", "application/xml");

            var api = new ApiRequest();
            var search = api.ExecuteXml<PlexSearch>(request, plexFullHost);

            return search;
        }
    }
}

