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
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UserManagementModule : BaseModule
    {
        public UserManagementModule(ISettingsService<PlexRequestSettings> pr, ICustomUserMapper m, IPlexApi plexApi, ISettingsService<PlexSettings> plex, IRepository<UserLogins> userLogins) : base("usermanagement", pr)
        {
#if !DEBUG
            this.RequiresClaims(UserClaims.Admin);
#endif
            UserMapper = m;
            PlexApi = plexApi;
            PlexSettings = plex;
            UserLoginsRepo = userLogins;

            Get["/"] = x => Load();

            Get["/users", true] = async (x, ct) => await LoadUsers();
            Post["/createuser"] = x => CreateUser();
            Get["/local/{id}"] = x => LocalDetails((Guid)x.id);
            Get["/plex/{id}", true] = async (x, ct) => await PlexDetails(x.id);
            Get["/claims"] = x => GetClaims();
            Post["/updateuser"] = x => UpdateUser();
            Post["/deleteuser"] = x => DeleteUser();
        }

        private ICustomUserMapper UserMapper { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private IRepository<UserLogins> UserLoginsRepo { get; }

        private Negotiator Load()
        {
            return View["Index"];
        }

        private async Task<Response> LoadUsers()
        {
            var localUsers = await UserMapper.GetUsersAsync();
            var model = new List<UserManagementUsersViewModel>();

            var usersDb = UserLoginsRepo.GetAll().ToList();

            foreach (var user in localUsers)
            {
                var userDb = usersDb.FirstOrDefault(x => x.UserId == user.UserGuid);
                model.Add(MapLocalUser(user, userDb?.LastLoggedIn ?? DateTime.MinValue));
            }

            var plexSettings = await PlexSettings.GetSettingsAsync();
            if (!string.IsNullOrEmpty(plexSettings.PlexAuthToken))
            {
                //Get Plex Users
                var plexUsers = PlexApi.GetUsers(plexSettings.PlexAuthToken);

                foreach (var u in plexUsers.User)
                {
                    var userDb = usersDb.FirstOrDefault(x => x.UserId == u.Id);
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
                        },
                        LastLoggedIn = userDb?.LastLoggedIn ?? DateTime.MinValue,
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
                return Response.AsJson(MapLocalUser(UserMapper.GetUser(user.Value), DateTime.MinValue));
            }

            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user" });
        }

        private Response UpdateUser()
        {
            var body = Request.Body.AsString();
            if (string.IsNullOrEmpty(body))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user, invalid JSON body" });
            }

            var model = JsonConvert.DeserializeObject<UserManagementUpdateModel>(body);

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = "Couldn't find the user"
                });
            }

            var claims = new List<string>();

            foreach (var c in model.Claims)
            {
                if (c.Selected)
                {
                    claims.Add(c.Name);
                }
            }

            var userFound = UserMapper.GetUser(new Guid(model.Id));

            userFound.Claims = ByteConverterHelper.ReturnBytes(claims.ToArray());
            var currentProps = ByteConverterHelper.ReturnObject<UserProperties>(userFound.UserProperties);
            currentProps.UserAlias = model.Alias;
            currentProps.EmailAddress = model.EmailAddress;

            userFound.UserProperties = ByteConverterHelper.ReturnBytes(currentProps);

            var user = UserMapper.EditUser(userFound);
            var dbUser = UserLoginsRepo.GetAll().FirstOrDefault(x => x.UserId == user.UserGuid);
            var retUser = MapLocalUser(user, dbUser?.LastLoggedIn ?? DateTime.MinValue);
            return Response.AsJson(retUser);
        }

        private Response DeleteUser()
        {
            var body = Request.Body.AsString();
            if (string.IsNullOrEmpty(body))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user, invalid JSON body" });
            }

            var model = JsonConvert.DeserializeObject<DeleteUserViewModel>(body);

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = "Couldn't find the user"
                });
            }

           UserMapper.DeleteUser(model.Id);

            return Response.AsJson(new JsonResponseModel {Result = true});
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

        private UserManagementUsersViewModel MapLocalUser(UsersModel user, DateTime lastLoggedIn)
        {
            var claims = ByteConverterHelper.ReturnObject<string[]>(user.Claims);
            var claimsString = string.Join(", ", claims);

            var userProps = ByteConverterHelper.ReturnObject<UserProperties>(user.UserProperties);

            var m = new UserManagementUsersViewModel
            {
                Id = user.UserGuid,
                Claims = claimsString,
                Username = user.UserName,
                Type = UserType.LocalUser,
                EmailAddress = userProps.EmailAddress,
                Alias = userProps.UserAlias,
                ClaimsArray = claims,
                ClaimsItem = new List<UserManagementUpdateModel.ClaimsModel>(),
                LastLoggedIn = lastLoggedIn
            };

            // Add all of the current claims
            foreach (var c in claims)
            {
                m.ClaimsItem.Add(new UserManagementUpdateModel.ClaimsModel { Name = c, Selected = true });
            }

            var allClaims = UserMapper.GetAllClaims();

            // Get me the current claims that the user does not have
            var missingClaims = allClaims.Except(claims);

            // Add them into the view
            foreach (var missingClaim in missingClaims)
            {
                m.ClaimsItem.Add(new UserManagementUpdateModel.ClaimsModel { Name = missingClaim, Selected = false });
            }
            return m;
        }
    }
}

