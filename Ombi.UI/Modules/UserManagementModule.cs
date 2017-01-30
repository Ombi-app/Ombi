using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Newtonsoft.Json;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Ombi.UI.Models;
using Ombi.UI.Models.UserManagement;
using Action = Ombi.Helpers.Analytics.Action;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules
{
    public class UserManagementModule : BaseModule
    {
        public UserManagementModule(ISettingsService<PlexRequestSettings> pr, ICustomUserMapper m, IPlexApi plexApi, ISettingsService<PlexSettings> plex, IRepository<UserLogins> userLogins, IExternalUserRepository<PlexUsers> plexRepo
            , ISecurityExtensions security, IRequestService req, IAnalytics ana, ISettingsService<EmbySettings> embyService, IEmbyApi embyApi, IExternalUserRepository<EmbyUsers> embyRepo) : base("usermanagement", pr, security)
        {
#if !DEBUG
            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);
#endif
            UserMapper = m;
            PlexApi = plexApi;
            PlexSettings = plex;
            UserLoginsRepo = userLogins;
            PlexUsersRepository = plexRepo;
            PlexRequestSettings = pr;
            RequestService = req;
            Analytics = ana;
            EmbySettings = embyService;
            EmbyApi = embyApi;
            EmbyRepository = embyRepo;

            Get["/"] = x => Load();

            Get["/users", true] = async (x, ct) => await LoadUsers();
            Post["/createuser", true] = async (x, ct) => await CreateUser();
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
        private IExternalUserRepository<PlexUsers> PlexUsersRepository { get; }
        private IExternalUserRepository<EmbyUsers> EmbyRepository { get; }
        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private IRequestService RequestService { get; }
        private IAnalytics Analytics { get; }
        private IEmbyApi EmbyApi { get; }

        private Negotiator Load()
        {
            return View["Index"];
        }

        private async Task<Response> LoadUsers()
        {

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();
            if (plexSettings.Enable)
            {
                return await LoadPlexUsers();
            }
            if (embySettings.Enable)
            {
                return await LoadEmbyUsers();
            }

            return null;
        }

        private async Task<Response> CreateUser()
        {
            Analytics.TrackEventAsync(Category.UserManagement, Action.Create, "Created User", Username, CookieHelper.GetAnalyticClientId(Cookies));
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
                    Result = false,
                    Message = "Please enter in a valid Username and Password"
                });
            }

            var users = await UserMapper.GetUsersAsync();
            if (users.Any(x => x.UserName.Equals(model.Username, StringComparison.CurrentCultureIgnoreCase)))
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = false,
                    Message = $"A user with the username '{model.Username}' already exists"
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
            Analytics.TrackEventAsync(Category.UserManagement, Action.Update, "Updated User", Username, CookieHelper.GetAnalyticClientId(Cookies));
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

                // Let's check if the alias has changed, if so we need to change all the requests associated with this
                await UpdateRequests(localUser.UserName, currentProps.UserAlias, model.Alias);

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

                await UpdateRequests(plexDbUser.Username, plexDbUser.UserAlias, model.Alias);

                plexDbUser.UserAlias = model.Alias;
                plexDbUser.EmailAddress = model.EmailAddress;

                await PlexUsersRepository.UpdateAsync(plexDbUser);

                var retUser = MapPlexUser(plexUser, plexDbUser, userLogin?.LastLoggedIn ?? DateTime.MinValue);
                return Response.AsJson(retUser);
            }

            // So it could actually be the admin
            var account = PlexApi.GetAccount(plexSettings.PlexAuthToken);
            if (plexDbUser != null && account != null)
            {
                // We have a user in the DB for this Plex Account
                plexDbUser.Permissions = permissionsValue;
                plexDbUser.Features = featuresValue;

                await UpdateRequests(plexDbUser.Username, plexDbUser.UserAlias, model.Alias);

                plexDbUser.UserAlias = model.Alias;

                await PlexUsersRepository.UpdateAsync(plexDbUser);

                var retUser = MapPlexAdmin(account, plexDbUser, userLogin?.LastLoggedIn ?? DateTime.MinValue);
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
                    PlexUserId = plexUser.Id,
                    EmailAddress = plexUser.Email,
                    Username = plexUser.Title,
                    LoginId = Guid.NewGuid().ToString()
                };

                await PlexUsersRepository.InsertAsync(user);

                var retUser = MapPlexUser(plexUser, user, userLogin?.LastLoggedIn ?? DateTime.MinValue);
                return Response.AsJson(retUser);
            }
            return null; // We should never end up here.
        }

        private async Task UpdateRequests(string username, string oldAlias, string newAlias)
        {
            var newUsername = string.IsNullOrEmpty(newAlias) ? username : newAlias; // User the username if we are clearing the alias
            var olderUsername = string.IsNullOrEmpty(oldAlias) ? username : oldAlias;
            // Let's check if the alias has changed, if so we need to change all the requests associated with this
            if (!olderUsername.Equals(newUsername, StringComparison.CurrentCultureIgnoreCase))
            {
                var requests = await RequestService.GetAllAsync();
                // Update all requests
                var requestsWithThisUser = requests.Where(x => x.RequestedUsers.Contains(olderUsername)).ToList();
                foreach (var r in requestsWithThisUser)
                {
                    r.RequestedUsers.Remove(olderUsername); // Remove old
                    r.RequestedUsers.Add(newUsername); // Add new
                }

                if (requestsWithThisUser.Any())
                {
                    RequestService.BatchUpdate(requestsWithThisUser);
                }

            }
        }

        private Response DeleteUser()
        {
            Analytics.TrackEventAsync(Category.UserManagement, Action.Delete, "Deleted User", Username, CookieHelper.GetAnalyticClientId(Cookies));
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

            return Response.AsJson(new JsonResponseModel { Result = true });
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

                retVal.Add(new CheckBox { Name = displayValue, Selected = false, Value = (int)p });
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


            m.Permissions.AddRange(GetPermissions(permissions));
            m.Features.AddRange(GetFeatures(features));

            return m;
        }

        private UserManagementUsersViewModel MapPlexUser(UserFriends plexInfo, PlexUsers dbUser, DateTime lastLoggedIn)
        {
            var newUser = false;
            if (dbUser == null)
            {
                newUser = true;
                dbUser = new PlexUsers();
            }
            var features = (Features)dbUser?.Features;
            var permissions = (Permissions)dbUser?.Permissions;

            var m = new UserManagementUsersViewModel
            {
                Id = plexInfo.Id,
                PermissionsFormattedString = newUser ? "Processing..." : (permissions == 0 ? "None" : permissions.ToString()),
                FeaturesFormattedString = newUser ? "Processing..." : features.ToString(),
                Username = plexInfo.Title,
                Type = UserType.PlexUser,
                EmailAddress = string.IsNullOrEmpty(plexInfo.Email) ? dbUser.EmailAddress : plexInfo.Email,
                Alias = dbUser?.UserAlias ?? string.Empty,
                LastLoggedIn = lastLoggedIn,
                PlexInfo = new UserManagementPlexInformation
                {
                    Thumb = plexInfo.Thumb
                },
                ManagedUser = string.IsNullOrEmpty(plexInfo.Username)
            };

            m.Permissions.AddRange(GetPermissions(permissions));
            m.Features.AddRange(GetFeatures(features));

            return m;
        }

        private UserManagementUsersViewModel MapEmbyUser(EmbyUser embyInfo, EmbyUsers dbUser, DateTime lastLoggedIn)
        {
            var newUser = false;
            if (dbUser == null)
            {
                newUser = true;
                dbUser = new EmbyUsers();
            }
            var features = (Features)dbUser?.Features;
            var permissions = (Permissions)dbUser?.Permissions;

            var m = new UserManagementUsersViewModel
            {
                Id = embyInfo.Id,
                PermissionsFormattedString = newUser ? "Processing..." : (permissions == 0 ? "None" : permissions.ToString()),
                FeaturesFormattedString = newUser ? "Processing..." : features.ToString(),
                Username = embyInfo.Name,
                Type = UserType.EmbyUser,
                EmailAddress =dbUser.EmailAddress,
                Alias = dbUser?.UserAlias ?? string.Empty,
                LastLoggedIn = lastLoggedIn,
                ManagedUser = false
            };

            m.Permissions.AddRange(GetPermissions(permissions));
            m.Features.AddRange(GetFeatures(features));

            return m;
        }

        private UserManagementUsersViewModel MapPlexAdmin(PlexAccount plexInfo, PlexUsers dbUser, DateTime lastLoggedIn)
        {
            var newUser = false;
            if (dbUser == null)
            {
                newUser = true;
                dbUser = new PlexUsers();
            }
            var features = (Features)dbUser?.Features;
            var permissions = (Permissions)dbUser?.Permissions;

            var m = new UserManagementUsersViewModel
            {
                Id = plexInfo.Id,
                PermissionsFormattedString = newUser ? "Processing..." : (permissions == 0 ? "None" : permissions.ToString()),
                FeaturesFormattedString = newUser ? "Processing..." : features.ToString(),
                Username = plexInfo.Username,
                Type = UserType.PlexUser,
                EmailAddress = plexInfo.Email,
                Alias = dbUser?.UserAlias ?? string.Empty,
                LastLoggedIn = lastLoggedIn,
            };

            m.Permissions.AddRange(GetPermissions(permissions));
            m.Features.AddRange(GetFeatures(features));

            return m;
        }

        private List<CheckBox> GetPermissions(Permissions permissions)
        {
            var retVal = new List<CheckBox>();
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

                retVal.Add(pm);
            }

            return retVal;
        }

        private List<CheckBox> GetFeatures(Features features)
        {
            var retVal = new List<CheckBox>();
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

                retVal.Add(pm);
            }
            return retVal;
        }

        private async Task<Response> LoadPlexUsers()
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
                if (plexUsers != null && plexUsers.User != null)
                {
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

                // Also get the server admin
                var account = PlexApi.GetAccount(plexSettings.PlexAuthToken);
                if (account != null)
                {
                    var dbUser = plexDbUsers.FirstOrDefault(x => x.PlexUserId == account.Id);
                    var userDb = userLogins.FirstOrDefault(x => x.UserId == account.Id);
                    model.Add(MapPlexAdmin(account, dbUser, userDb?.LastLoggedIn ?? DateTime.MinValue));
                }
            }
            return Response.AsJson(model);
        }

        private async Task<Response> LoadEmbyUsers()
        {
            var localUsers = await UserMapper.GetUsersAsync();
            var embyDbUsers = await EmbyRepository.GetAllAsync();
            var model = new List<UserManagementUsersViewModel>();

            var userLogins = UserLoginsRepo.GetAll().ToList();

            foreach (var user in localUsers)
            {
                var userDb = userLogins.FirstOrDefault(x => x.UserId == user.UserGuid);
                model.Add(MapLocalUser(user, userDb?.LastLoggedIn ?? DateTime.MinValue));
            }

            var embySettings = await EmbySettings.GetSettingsAsync();
            if (!string.IsNullOrEmpty(embySettings.ApiKey))
            {
                //Get Plex Users
                var plexUsers = EmbyApi.GetUsers(embySettings.FullUri, embySettings.ApiKey);
                if (plexUsers != null)
                {
                    foreach (var u in plexUsers)
                    {
                        var dbUser = embyDbUsers.FirstOrDefault(x => x.PlexUserId == u.Id);
                        var userDb = userLogins.FirstOrDefault(x => x.UserId == u.Id);

                        // We don't have the user in the database yet
                        model.Add(dbUser == null
                            ? MapEmbyUser(u, null, userDb?.LastLoggedIn ?? DateTime.MinValue)
                            : MapEmbyUser(u, dbUser, userDb?.LastLoggedIn ?? DateTime.MinValue));
                    }
                }

                // Also get the server admin
                var account = PlexApi.GetAccount(embySettings.PlexAuthToken);
                if (account != null)
                {
                    var dbUser = embyDbUsers.FirstOrDefault(x => x.PlexUserId == account.Id);
                    var userDb = userLogins.FirstOrDefault(x => x.UserId == account.Id);
                    model.Add(MapPlexAdmin(account, dbUser, userDb?.LastLoggedIn ?? DateTime.MinValue));
                }
            }
            return Response.AsJson(model);
        }
    }
}

