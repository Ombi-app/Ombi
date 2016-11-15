using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Newtonsoft.Json;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UserManagementModule : BaseModule
    {
        public UserManagementModule(ISettingsService<PlexRequestSettings> pr, ICustomUserMapper m, IPlexApi plexApi, ISettingsService<PlexSettings> plex, IRepository<UserLogins> userLogins, IRepository<PlexUsers> plexRepo) : base("usermanagement", pr)
        {
#if !DEBUG
            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);
#endif
            UserMapper = m;
            PlexApi = plexApi;
            PlexSettings = plex;
            UserLoginsRepo = userLogins;
            PlexUsersRepository = plexRepo;

            Get["/"] = x => Load();

            Get["/users", true] = async (x, ct) => await LoadUsers();
            Post["/createuser"] = x => CreateUser();
            Get["/local/{id}"] = x => LocalDetails((Guid)x.id);
            Get["/plex/{id}", true] = async (x, ct) => await PlexDetails(x.id);
            Get["/permissions"] = x => GetEnum<Permissions>();
            Get["/features"] = x => GetEnum<Features>();
            Post["/updateuser", true] = async (x, ct) => await UpdateUser();
            Post["/deleteuser"] = x => DeleteUser();
        }

        private ICustomUserMapper UserMapper { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private IRepository<UserLogins> UserLoginsRepo { get; }
        private IRepository<PlexUsers> PlexUsersRepository { get; }

        private Negotiator Load()
        {
            return View["Index"];
        }

        private async Task<Response> LoadUsers()
        {
            var localUsers = await UserMapper.GetUsersAsync();
            var plexDbUsers = await PlexUsersRepository.GetAllAsync();
            var model = new List<UserManagementUsersViewModel>();

            var userLogins = UserLoginsRepo.GetAll().ToList();

            foreach (var user in localUsers)
            {
                var userDb = userLogins.FirstOrDefault(x => x.UserId == user.UserGuid);
                model.Add(MapLocalUser(user, userDb?.LastLoggedIn ?? DateTime.MinValue));
            }

            var plexSettings = await PlexSettings.GetSettingsAsync();
            if (!string.IsNullOrEmpty(plexSettings.PlexAuthToken))
            {
                //Get Plex Users
                var plexUsers = PlexApi.GetUsers(plexSettings.PlexAuthToken);

                foreach (var u in plexUsers.User)
                {
                    var dbUser = plexDbUsers.FirstOrDefault(x => x.PlexUserId == u.Id);
                    var userDb = userLogins.FirstOrDefault(x => x.UserId == u.Id);
                    // We don't have the user in the database yet
                    if (dbUser == null)
                    {
                        model.Add(MapPlexUser(u, null, userDb?.LastLoggedIn ?? DateTime.MinValue));
                    }
                    else
                    {
                        // The Plex User is in the database
                        model.Add(MapPlexUser(u, dbUser, userDb?.LastLoggedIn ?? DateTime.MinValue));
                    }
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

            var featuresVal = 0;
            var permissionsVal = 0;

            foreach (var feature in model.Features)
            {
                var f = (int)EnumHelper<Features>.GetValueFromName(feature);
                featuresVal += f;
            }

            foreach (var permission in model.Permissions)
            {
                var f = (int)EnumHelper<Permissions>.GetValueFromName(permission);
                permissionsVal += f;
            }
            
            var user = UserMapper.CreateUser(model.Username, model.Password, permissionsVal, featuresVal, new UserProperties { EmailAddress = model.EmailAddress });
            if (user.HasValue)
            {
                return Response.AsJson(MapLocalUser(UserMapper.GetUser(user.Value), DateTime.MinValue));
            }

            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save user" });
        }

        private async Task<Response> UpdateUser()
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
            
            var permissionsValue = model.Permissions.Where(c => c.Selected).Sum(c => c.Value);
            var featuresValue = model.Features.Where(c => c.Selected).Sum(c => c.Value);

            Guid outId;
            Guid.TryParse(model.Id, out outId);
            var localUser = UserMapper.GetUser(outId);

            // Update Local User
            if (localUser != null)
            {
                localUser.Permissions = permissionsValue;
                localUser.Features = featuresValue;

                var currentProps = ByteConverterHelper.ReturnObject<UserProperties>(localUser.UserProperties);
                currentProps.UserAlias = model.Alias;
                currentProps.EmailAddress = model.EmailAddress;

                localUser.UserProperties = ByteConverterHelper.ReturnBytes(currentProps);

                var user = UserMapper.EditUser(localUser);
                var dbUser = UserLoginsRepo.GetAll().FirstOrDefault(x => x.UserId == user.UserGuid);
                var retUser = MapLocalUser(user, dbUser?.LastLoggedIn ?? DateTime.MinValue);
                return Response.AsJson(retUser);
            }

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var plexDbUsers = await PlexUsersRepository.GetAllAsync();
            var plexUsers = PlexApi.GetUsers(plexSettings.PlexAuthToken);
            var plexDbUser = plexDbUsers.FirstOrDefault(x => x.PlexUserId == model.Id);
            var plexUser = plexUsers.User.FirstOrDefault(x => x.Id == model.Id);
            var userLogin = UserLoginsRepo.GetAll().FirstOrDefault(x => x.UserId == model.Id);
            if (plexDbUser != null && plexUser != null)
            {
                // We have a user in the DB for this Plex Account
                plexDbUser.Permissions = permissionsValue;
                plexDbUser.Features = featuresValue;

                plexDbUser.UserAlias = model.Alias;

                await PlexUsersRepository.UpdateAsync(plexDbUser);
                
                var retUser = MapPlexUser(plexUser, plexDbUser, userLogin?.LastLoggedIn ?? DateTime.MinValue);
                return Response.AsJson(retUser);
            }

            // We have a Plex Account but he's not in the DB
            if (plexUser != null)
            {
                var user = new PlexUsers
                {
                    Permissions = permissionsValue,
                    Features = featuresValue,
                    UserAlias = model.Alias,
                    PlexUserId = plexUser.Id
                };

                await PlexUsersRepository.InsertAsync(user);

                var retUser = MapPlexUser(plexUser, user, userLogin?.LastLoggedIn ?? DateTime.MinValue);
                return Response.AsJson(retUser);
            }
            return null; // We should never end up here.
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
        private Response GetEnum<T>()
        {
            var retVal = new List<CheckBox>();
            foreach (var p in Enum.GetValues(typeof(T)))
            {
                var perm = (T)p;
                var displayValue = EnumHelper<T>.GetDisplayValue(perm);
                
                retVal.Add(new CheckBox{ Name = displayValue, Selected = false, Value = (int)p });
            }

            return Response.AsJson(retVal);
        }

        private UserManagementUsersViewModel MapLocalUser(UsersModel user, DateTime lastLoggedIn)
        {
            var features = (Features)user.Features;
            var permissions = (Permissions)user.Permissions;

            var userProps = ByteConverterHelper.ReturnObject<UserProperties>(user.UserProperties);

            var m = new UserManagementUsersViewModel
            {
                Id = user.UserGuid,
                PermissionsFormattedString = permissions == 0 ? "None" : permissions.ToString(),
                FeaturesFormattedString = features.ToString(),
                Username = user.UserName,
                Type = UserType.LocalUser,
                EmailAddress = userProps.EmailAddress,
                Alias = userProps.UserAlias,
                LastLoggedIn = lastLoggedIn,
            };

            // Add permissions
            foreach (var p in Enum.GetValues(typeof(Permissions)))
            {
                var perm = (Permissions)p;
                var displayValue = EnumHelper<Permissions>.GetDisplayValue(perm);
                var pm = new CheckBox
                {
                    Name = displayValue,
                    Selected = permissions.HasFlag(perm),
                    Value = (int)perm
                };

                m.Permissions.Add(pm);
            }

            // Add features
            foreach (var p in Enum.GetValues(typeof(Features)))
            {
                var perm = (Features)p;
                var displayValue = EnumHelper<Features>.GetDisplayValue(perm);
                var pm = new CheckBox
                {
                    Name = displayValue,
                    Selected = features.HasFlag(perm),
                    Value = (int)perm
                };

                m.Features.Add(pm);
            }

            return m;
        }

        private UserManagementUsersViewModel MapPlexUser(UserFriends plexInfo, PlexUsers dbUser, DateTime lastLoggedIn)
        {
            if (dbUser == null)
            {
                dbUser = new PlexUsers();
            }
            var features = (Features)dbUser?.Features;
            var permissions = (Permissions)dbUser?.Permissions;

            var m = new UserManagementUsersViewModel
            {
                Id = plexInfo.Id,
                PermissionsFormattedString = permissions == 0 ? "None" : permissions.ToString(),
                FeaturesFormattedString = features.ToString(),
                Username = plexInfo.Username,
                Type = UserType.PlexUser,
                EmailAddress = plexInfo.Email,
                Alias = dbUser?.UserAlias ?? string.Empty,
                LastLoggedIn = lastLoggedIn,
                PlexInfo = new UserManagementPlexInformation
                {
                    Thumb = plexInfo.Thumb
                },
            };

            // Add permissions
            foreach (var p in Enum.GetValues(typeof(Permissions)))
            {
                var perm = (Permissions)p;
                var displayValue = EnumHelper<Permissions>.GetDisplayValue(perm);
                var pm = new CheckBox
                {
                    Name = displayValue,
                    Selected = permissions.HasFlag(perm),
                    Value = (int)perm
                };

                m.Permissions.Add(pm);
            }

            // Add features
            foreach (var p in Enum.GetValues(typeof(Features)))
            {
                var perm = (Features)p;
                var displayValue = EnumHelper<Features>.GetDisplayValue(perm);
                var pm = new CheckBox
                {
                    Name = displayValue,
                    Selected = features.HasFlag(perm),
                    Value = (int)perm
                };

                m.Features.Add(pm);
            }

            return m;
        }
    }
}

