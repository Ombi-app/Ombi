using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UserManagementModule : BaseModule
    {
        public UserManagementModule(ISettingsService<PlexRequestSettings> pr, ICustomUserMapper m, IPlexApi plexApi, ISettingsService<AuthenticationSettings> auth) : base("usermanagement", pr)
        {
            //this.RequiresClaims(UserClaims.Admin);

            UserMapper = m;
            PlexApi = plexApi;
            AuthSettings = auth;

            Get["/"] = x => Load();

            Get["/users", true] = async (x, ct) => await LoadUsers();
            Post["/createuser"] = x => CreateUser(Request.Form["userName"].ToString(), Request.Form["password"].ToString());
        }

        private ICustomUserMapper UserMapper { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }

        private Negotiator Load()
        {
            return View["Index"];
        }

        private async Task<Response> LoadUsers()
        {
            var localUsers = await UserMapper.GetUsersAsync();
            var model = new List<UserManagementUsersViewModel>();
            foreach (var user in localUsers)
            {
                var claims = ByteConverterHelper.ReturnObject<string[]>(user.Claims);
                var claimsString = string.Join(", ", claims);

                var userProps = ByteConverterHelper.ReturnObject<UserProperties>(user.UserProperties);

                model.Add(new UserManagementUsersViewModel
                {
                    Claims = claimsString,
                    Username = user.UserName,
                    Type = UserType.LocalUser,
                    EmailAddress = userProps.EmailAddress
                });
            }

            var authSettings = await AuthSettings.GetSettingsAsync();
            if (!string.IsNullOrEmpty(authSettings.PlexAuthToken))
            {
                //Get Plex Users
                var plexUsers = PlexApi.GetUsers(authSettings.PlexAuthToken);

                foreach (var u in plexUsers.User)
                {
                    model.Add(new UserManagementUsersViewModel
                    {
                        Username = u.Username,
                        Type = UserType.PlexUser,
                        //Alias = 
                        Claims = "Requestor",
                        EmailAddress = u.Email
                    });
                }
            }
            return Response.AsJson(model);
        }

        private Response CreateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = "Please enter in a valid Username and Password"
                });
            }
            var user = UserMapper.CreateRegularUser(username, password);
            if (user.HasValue)
            {
                return Response.AsJson(user);
            }

            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user" });
        }
    }
}

