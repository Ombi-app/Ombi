using System.Collections.Generic;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UserManagementModule : BaseModule
    {
        public UserManagementModule(ISettingsService<PlexRequestSettings> pr) : base("usermanagement",pr)
        {
            this.RequiresClaims(UserClaims.Admin);
            Get["/"] = x => Load();

            Get["/users"] = x => LoadUsers();
        }

        private Negotiator Load()
        {
            return View["Index"];
        }

        private Response LoadUsers()
        {
            var users = UserMapper.GetUsers();
            var model = new List<UserManagementUsersViewModel>();
            foreach (var user in users)
            {
                model.Add(new UserManagementUsersViewModel
                {
                    //Claims = ByteConverterHelper.ReturnObject<string[]>(user.Claims),
                    Claims = "test",
                    Id = user.Id,
                    Username = user.UserName,
                    //Type = UserType.LocalUser
                });
            }
            return Response.AsJson(users);
        }

        private Response CreateUser(string username, string password, string claims)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = "Please enter in a valid Username and Password"
                });
            }
            var user = UserMapper.CreateUser(username, password, new string[] {claims});
            if (user.HasValue)
            {
                return Response.AsJson(new JsonResponseModel {Result = true});
            }

            return Response.AsJson(new JsonResponseModel {Result = false, Message = "Could not save user"});
        }
    }
}

