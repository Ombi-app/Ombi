using System;
using RequestPlex.Api.Models;
using RestSharp;

namespace RequestPlex.Api
{
    public class PlexApi
    {
        public PlexAuthentication GetToken(string username, string password)
        {
            var userModel = new PlexUserRequest
            {
                user = new UserRequest
                {
                    password = password,
                    login = username
                },
            };
            var request = new RestRequest
            {
                Method = Method.POST,
            };

            request.AddHeader("X-Plex-Client-Identifier", "Test213"); // TODO need something unique to the users version/installation
            request.AddHeader("X-Plex-Product", "Request Plex");
            request.AddHeader("X-Plex-Version", "0.0.1");
            request.AddHeader("Content-Type", "application/json");
            
            request.AddJsonBody(userModel);

            var api = new ApiRequest();
            return api.Execute<PlexAuthentication>(request, new Uri("https://plex.tv/users/sign_in.json"));
        }

        public void GetUsers(string authToken)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
            };

            request.AddHeader("X-Plex-Client-Identifier", "Test213");
            request.AddHeader("X-Plex-Product", "Request Plex");
            request.AddHeader("X-Plex-Version", "0.0.1");
            request.AddHeader("X-Plex-Token", authToken);
            request.AddHeader("Content-Type", "application/json");

        }
    }
}

