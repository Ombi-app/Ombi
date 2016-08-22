using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Newtonsoft.Json;
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
        public UserManagementModule(ISettingsService<PlexRequestSettings> pr, ICustomUserMapper m, IPlexApi plexApi, ISettingsService<PlexSettings> plex) : base("usermanagement", pr)
        {
#if !DEBUG
            this.RequiresClaims(UserClaims.Admin);
#endif
            UserMapper = m;
            PlexApi = plexApi;
            PlexSettings = plex;

            Get["/"] = x => Load();

            Get["/users", true] = async (x, ct) => await LoadUsers();
            Post["/createuser"] = x => CreateUser();
            Get["/local/{id}"] = x => LocalDetails((Guid)x.id);
            Get["/plex/{id}", true] = async (x, ct) => await PlexDetails(x.id);
            Get["/claims"] = x => GetClaims();
        }

        private ICustomUserMapper UserMapper { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }

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
                    Id = user.UserGuid,
                    Claims = claimsString,
                    Username = user.UserName,
                    Type = UserType.LocalUser,
                    EmailAddress = userProps.EmailAddress
                });
            }

            var plexSettings = await PlexSettings.GetSettingsAsync();
            if (!string.IsNullOrEmpty(plexSettings.PlexAuthToken))
            {
                //Get Plex Users
                var plexUsers = PlexApi.GetUsers(plexSettings.PlexAuthToken);

                foreach (var u in plexUsers.User)
                {

                    model.Add(new UserManagementUsersViewModel
                    {
                        Username = u.Username,
                        Type = UserType.PlexUser,
                        Id = u.Id,
                        Claims = "Requestor",
                        EmailAddress = u.Email,
                        PlexInfo = new UserManagementPlexInformation
                        {
                            Thumb = u.Thumb
                        }
                    });
                }
            }
            return Response.AsJson(model);
        }

        private Response CreateUser()
        {
            var body = Request.Body.AsString();
            if (string.IsNullOrEmpty(body))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user, invalid JSON body" });
            }

            var model = JsonConvert.DeserializeObject<UserManagementCreateModel>(body);

            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = "Please enter in a valid Username and Password"
                });
            }
            var user = UserMapper.CreateUser(model.Username, model.Password, model.Claims, new UserProperties { EmailAddress = model.EmailAddress });
            if (user.HasValue)
            {
                return Response.AsJson(user);
            }

            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user" });
        }

        private Response LocalDetails(Guid id)
        {
            var localUser = UserMapper.GetUser(id);
            if (localUser != null)
            {
                return Response.AsJson(localUser);
            }

            return Nancy.Response.NoBody;
        }

        private async Task<Response> PlexDetails(string id)
        {
            var plexSettings = await PlexSettings.GetSettingsAsync();
            if (!string.IsNullOrEmpty(plexSettings.PlexAuthToken))
            {
                //Get Plex Users
                var plexUsers = PlexApi.GetUsers(plexSettings.PlexAuthToken);

                var selectedUser = plexUsers.User?.FirstOrDefault(x => x.Id.ToString() == id);
                if (selectedUser != null)
                {
                    return Response.AsJson(selectedUser);
                }

            }

            return Nancy.Response.NoBody;
        }

        /// <summary>
        /// Returns all claims for the users.
        /// </summary>
        /// <returns></returns>
        private Response GetClaims()
        {
            var retVal = new List<dynamic>();
            var claims = UserMapper.GetAllClaims();
            foreach (var c in claims)
            {
                retVal.Add(new { Name = c, Selected = false });
            }
            return Response.AsJson(retVal);
        }
    }
}

