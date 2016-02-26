using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nancy.Json;

namespace RequestPlex.Api
{
    public class PlexApi
    {
        public void GetToken(string username, string password)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("username:password");
            string auth = System.Convert.ToBase64String(plainTextBytes);

            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    ["Authorization"] = "Basic " + auth,
                    ["X-Plex-Client-Identifier"] = "RequestPlex0001",
                    ["X-Plex-Product"] = "Request Plex",
                    ["X-Plex-Version"] = "0.1.0"
                };

                client.Headers.Add(values);

                var response = client.UploadString("https://plex.tv/users/sign_in.json", "");

                var json = new JavaScriptSerializer();
                dynamic result = json.DeserializeObject(response);

                var token = result["user"]["authentication_token"];

                Debug.WriteLine(token);
            }
        }
    }
}
