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
using Polly;


#endregion
using System;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Helpers;

using RestSharp;

namespace PlexRequests.Api
{
    public class PlexApi : IPlexApi
    {
        static PlexApi()
        {
            Version = AssemblyHelper.GetAssemblyVersion();
        }

		public PlexApi (IApiRequest api)
		{
			Api = api;
		}
			
		private IApiRequest Api { get; }

		private const string SignInUri = "https://plex.tv/users/sign_in.json";
		private const string FriendsUri = "https://plex.tv/pms/friends/all";
		private const string GetAccountUri = "https://plex.tv/users/account";
        private const string ServerUri = "https://plex.tv/pms/servers.xml";

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static string Version { get; }

        public PlexAuthentication SignIn(string username, string password)
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

            AddHeaders(ref request);

            request.AddJsonBody(userModel);

			var obj = RetryHandler.Execute<PlexAuthentication>(() => Api.Execute<PlexAuthentication> (request, new Uri(SignInUri)),
				(exception, timespan) => Log.Error (exception, "Exception when calling SignIn for Plex, Retrying {0}", timespan));
			
			return obj;
        }

        public PlexFriends GetUsers(string authToken)
        {
                var request = new RestRequest
                {
                    Method = Method.GET,
                };

                AddHeaders(ref request, authToken);

			    var users = RetryHandler.Execute(() => Api.ExecuteXml<PlexFriends> (request, new Uri(FriendsUri)),
				    (exception, timespan) => Log.Error (exception, "Exception when calling GetUsers for Plex, Retrying {0}", timespan), null);
			

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
            AddHeaders(ref request, authToken);

			var search = RetryHandler.Execute(() => Api.ExecuteXml<PlexSearch> (request, plexFullHost),
				(exception, timespan) => Log.Error (exception, "Exception when calling SearchContent for Plex, Retrying {0}", timespan), null);

            return search;
        }

        public PlexStatus GetStatus(string authToken, Uri uri)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
            };

            AddHeaders(ref request, authToken);

			var users = RetryHandler.Execute(() => Api.ExecuteXml<PlexStatus> (request, uri),
				(exception, timespan) => Log.Error (exception, "Exception when calling GetStatus for Plex, Retrying {0}", timespan), null);
		
            return users;
        }

        public PlexAccount GetAccount(string authToken)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
            };

            AddHeaders(ref request, authToken);

			var account = RetryHandler.Execute(() => Api.ExecuteXml<PlexAccount> (request, new Uri(GetAccountUri)),
				(exception, timespan) => Log.Error (exception, "Exception when calling GetAccount for Plex, Retrying {0}", timespan), null);
			
            return account;
        }

        public PlexLibraries GetLibrarySections(string authToken, Uri plexFullHost)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "library/sections"
            };

            AddHeaders(ref request, authToken);

            try
            {
				var lib = RetryHandler.Execute(() => Api.ExecuteXml<PlexLibraries> (request, plexFullHost),
					(exception, timespan) => Log.Error (exception, "Exception when calling GetLibrarySections for Plex, Retrying {0}", timespan), new TimeSpan[] { 
					    TimeSpan.FromSeconds (5),
					    TimeSpan.FromSeconds(10),
					    TimeSpan.FromSeconds(30)
					});

				return lib;
            }
			catch (Exception e)
            {
                Log.Error(e,"There has been a API Exception when attempting to get the Plex Libraries");
                return new PlexLibraries();
            }
        }

        public PlexSearch GetLibrary(string authToken, Uri plexFullHost, string libraryId)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "library/sections/{libraryId}/all"
            };

            request.AddUrlSegment("libraryId", libraryId);
            AddHeaders(ref request, authToken);

            try
            {
				var lib = RetryHandler.Execute(() => Api.ExecuteXml<PlexSearch> (request, plexFullHost),
					(exception, timespan) => Log.Error (exception, "Exception when calling GetLibrary for Plex, Retrying {0}", timespan), new TimeSpan[] { 
					    TimeSpan.FromSeconds (5),
					    TimeSpan.FromSeconds(10),
					    TimeSpan.FromSeconds(30)
					});

				return lib;
            }
			catch (Exception e)
            {
                Log.Error(e,"There has been a API Exception when attempting to get the Plex Library");
                return new PlexSearch();
            }
        }

        public PlexEpisodeMetadata GetEpisodeMetaData(string authToken, Uri host, string ratingKey)
        {

            //192.168.1.69:32400/library/metadata/3662/allLeaves
            // The metadata ratingkey should be in the Cache
            // Search for it and then call the above with the Directory.RatingKey
            // THEN! We need the episode metadata using result.Vide.Key ("/library/metadata/3664")
            // We then have the GUID which contains the TVDB ID plus the season and episode number: guid="com.plexapp.agents.thetvdb://269586/2/8?lang=en"
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "/library/metadata/{ratingKey}"
            };

            request.AddUrlSegment("ratingKey", ratingKey);
            AddHeaders(ref request, authToken);

            try
            {
                var lib = RetryHandler.Execute(() => Api.ExecuteXml<PlexEpisodeMetadata>(request, host),
                    (exception, timespan) => Log.Error(exception, "Exception when calling GetEpisodeMetaData for Plex, Retrying {0}", timespan));

                return lib;
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been a API Exception when attempting to get GetEpisodeMetaData");
                return new PlexEpisodeMetadata();
            }
        }

        public PlexSearch GetAllEpisodes(string authToken, Uri host, string section, int startPage, int returnCount)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "/library/sections/{section}/all"
            };

            request.AddQueryParameter("type", 4.ToString());
            request.AddQueryParameter("X-Plex-Container-Start", startPage.ToString());
            request.AddQueryParameter("X-Plex-Container-Size", returnCount.ToString());
            request.AddUrlSegment("section", section);
            AddHeaders(ref request, authToken);

            try
            {
                var lib = RetryHandler.Execute(() => Api.ExecuteXml<PlexSearch>(request, host),
                    (exception, timespan) => Log.Error(exception, "Exception when calling GetAllEpisodes for Plex, Retrying {0}", timespan));

                return lib;
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been a API Exception when attempting to get GetAllEpisodes");
                return new PlexSearch();
            }
        }

        public PlexMetadata GetMetadata(string authToken, Uri plexFullHost, string itemId)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "library/metadata/{itemId}"
            };

            request.AddUrlSegment("itemId", itemId);
            AddHeaders(ref request, authToken);

            try
            {
                var lib = RetryHandler.Execute(() => Api.ExecuteXml<PlexMetadata>(request, plexFullHost),
                    (exception, timespan) => Log.Error(exception, "Exception when calling GetMetadata for Plex, Retrying {0}", timespan), new[] {
                        TimeSpan.FromSeconds (5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)
                    });

                return lib;
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been a API Exception when attempting to get the Plex GetMetadata");
                return new PlexMetadata();
            }
        }


        public PlexSeasonMetadata GetSeasons(string authToken, Uri plexFullHost, string ratingKey)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "library/metadata/{ratingKey}/children"
            };

            request.AddUrlSegment("ratingKey", ratingKey);
            AddHeaders(ref request, authToken);

            try
            {
                var lib = RetryHandler.Execute(() => Api.ExecuteXml<PlexSeasonMetadata>(request, plexFullHost),
                    (exception, timespan) => Log.Error(exception, "Exception when calling GetMetadata for Plex, Retrying {0}", timespan), new[] {
                        TimeSpan.FromSeconds (5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)
                    });

                return lib;
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been a API Exception when attempting to get the Plex GetMetadata");
                return new PlexSeasonMetadata();
            }
        }

        public PlexServer GetServer(string authToken)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
            };

            AddHeaders(ref request, authToken);

            var servers = RetryHandler.Execute(() => Api.ExecuteXml<PlexServer>(request, new Uri(ServerUri)),
                (exception, timespan) => Log.Error(exception, "Exception when calling GetServer for Plex, Retrying {0}", timespan));


            return servers;
        }

        public RecentlyAdded RecentlyAdded(string authToken, Uri plexFullHost)
        {
            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = "library/recentlyAdded"
            };
            
            request.AddHeader("X-Plex-Token", authToken);
            request.AddHeader("X-Plex-Client-Identifier", $"PlexRequests.Net{Version}");
            request.AddHeader("X-Plex-Product", "Plex Requests .Net");
            request.AddHeader("X-Plex-Version", Version);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            try
            {
                var lib = RetryHandler.Execute(() => Api.ExecuteJson<RecentlyAdded>(request, plexFullHost),
                    (exception, timespan) => Log.Error(exception, "Exception when calling RecentlyAdded for Plex, Retrying {0}", timespan), new[] {
                        TimeSpan.FromSeconds (5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)
                    });

                return lib;
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been a API Exception when attempting to get the Plex RecentlyAdded");
                return new RecentlyAdded();
            }
        }

        private void AddHeaders(ref RestRequest request, string authToken)
        {
            request.AddHeader("X-Plex-Token", authToken);
            AddHeaders(ref request);
        }

        private void AddHeaders(ref RestRequest request)
        {
            request.AddHeader("X-Plex-Client-Identifier", $"PlexRequests.Net{Version}");
            request.AddHeader("X-Plex-Product", "Plex Requests .Net");
            request.AddHeader("X-Plex-Version", Version);
            request.AddHeader("Content-Type", "application/xml");
        }
    }
}

