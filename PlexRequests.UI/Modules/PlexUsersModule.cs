#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexUsersModule.cs
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
using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Responses.Negotiation;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Music;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class PlexUsersModule : BaseAuthModule
    {
        public PlexUsersModule(ISettingsService<PlexRequestSettings> pr, IPlexApi plexApi, ISettingsService<AuthenticationSettings> auth,
                               IRepository<PlexUsers> repo) : base("plexusers", pr)
        {
            PlexApi = plexApi;
            AuthSettings = auth;
            Repo = repo;

            Get["/"] = x => Index();
            Get["/users", true] = async (x, ct) => await GetPlexUsers();

            Post["/alias", true] = async (x, ct) => await Alias();
        }

        private IPlexApi PlexApi { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }
        private IRepository<PlexUsers> Repo { get; }


        private Negotiator Index()
        {
            return View["Index"];
        }

        private async Task<Response> GetPlexUsers()
        {
            var authSettings = await AuthSettings.GetSettingsAsync();
            var users = PlexApi.GetUsers(authSettings.PlexAuthToken);

            return Response.AsJson(users.User);
        }

        private async Task<Response> Alias()
        {
            var plexUserId = (int)Request.Form["plexid"];
            var alias = (string)Request.Form["alias"];
            var allUsers = await Repo.GetAllAsync();
            var existingUser = allUsers.FirstOrDefault(x => x.PlexUserId == plexUserId);
            if (existingUser == null)
            {
                // Create a new mapping
                existingUser = new PlexUsers { PlexUserId = plexUserId, UserAlias = alias };
            }
            else
            {
                // Modify existing alias
                existingUser.UserAlias = alias;
            }

            await Repo.InsertAsync(existingUser);

            return Response.AsJson(new JsonResponseModel { Result = true });
        }
    }
}