#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserHelper.cs
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

using System.Collections.Generic;
using System.Linq;
using Ombi.Core.Models;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;

namespace Ombi.Core.Users
{
    public class UserHelper : IUserHelper
    {
        public UserHelper(IUserRepository userRepository, IExternalUserRepository<PlexUsers> plexUsers, IExternalUserRepository<EmbyUsers> emby, ISecurityExtensions security)
        {
            LocalUserRepository = userRepository;
            PlexUserRepository = plexUsers;
            Security = security;
            EmbyUserRepository = emby;
        }

        private IUserRepository LocalUserRepository { get; }
        private IExternalUserRepository<PlexUsers> PlexUserRepository { get; }
        private ISecurityExtensions Security { get; }
        private IExternalUserRepository<EmbyUsers> EmbyUserRepository { get; }

        public UserHelperModel GetUser(string username)
        {
            var localUsers = LocalUserRepository.GetUserByUsername(username);
            if (localUsers != null)
            {
                var props = ByteConverterHelper.ReturnObject<UserProperties>(localUsers.UserProperties);
                return new UserHelperModel
                {
                    Type = UserType.LocalUser,
                    Username = localUsers.UserName,
                    UserAlias = props.UserAlias,
                    EmailAddress = props.EmailAddress,
                    Permissions = (Permissions) localUsers.Permissions
                };
            }

            var plexUsers = PlexUserRepository.GetUserByUsername(username);
            if (plexUsers != null)
            {
                return new UserHelperModel
                {
                    Type = UserType.PlexUser,
                    Username = plexUsers.Username,
                    UserAlias = plexUsers.UserAlias,
                    EmailAddress = plexUsers.EmailAddress,
                    Permissions = (Permissions)plexUsers.Permissions
                };
            }

            var embyUsers = EmbyUserRepository.GetUserByUsername(username);
            if (embyUsers != null)
            {
                return new UserHelperModel
                {
                    Type = UserType.EmbyUser,
                    Username = embyUsers.Username,
                    UserAlias = embyUsers.UserAlias,
                    EmailAddress = embyUsers.EmailAddress,
                    Permissions = (Permissions)embyUsers.Permissions
                };
            }
            return null;
        }

        public IEnumerable<UserHelperModel> GetUsers()
        {
            var model = new List<UserHelperModel>();

            var localUsers = LocalUserRepository.GetAll();
            var plexUsers = PlexUserRepository.GetAll().ToList();
            var embyUsers = EmbyUserRepository.GetAll().ToList();

            foreach (var user in localUsers)
            {
                var props = ByteConverterHelper.ReturnObject<UserProperties>(user.UserProperties);
                model.Add(new UserHelperModel
                {
                    Type = UserType.LocalUser,
                    Username = user.UserName,
                    UserAlias = props.UserAlias,
                    EmailAddress = props.EmailAddress,
                    Permissions = (Permissions)user.Permissions
                });
            }

            if (plexUsers.Any())
            {
                model.AddRange(plexUsers.Select(user => new UserHelperModel
                {
                    Type = UserType.PlexUser,
                    Username = user.Username,
                    UserAlias = user.UserAlias,
                    EmailAddress = user.EmailAddress,
                    Permissions = (Permissions) user.Permissions
                }));
            }

            if (embyUsers.Any())
            {
                model.AddRange(embyUsers.Select(user => new UserHelperModel
                {
                    Type = UserType.EmbyUser,
                    Username = user.Username,
                    UserAlias = user.UserAlias,
                    EmailAddress = user.EmailAddress,
                    Permissions = (Permissions)user.Permissions
                }));

            }

            return model;
        }

        public IEnumerable<UserHelperModel> GetUsersWithPermission(Permissions permission)
        {
            var model = new List<UserHelperModel>();

            var localUsers = LocalUserRepository.GetAll().ToList();
            var plexUsers = PlexUserRepository.GetAll().ToList();
            var embyUsers = EmbyUserRepository.GetAll().ToList();

            var filteredLocal = localUsers.Where(x => ((Permissions)x.Permissions).HasFlag(permission));
            var filteredPlex = plexUsers.Where(x => ((Permissions)x.Permissions).HasFlag(permission));
            var filteredEmby = embyUsers.Where(x => ((Permissions)x.Permissions).HasFlag(permission));


            foreach (var user in filteredLocal)
            {
                var props = ByteConverterHelper.ReturnObject<UserProperties>(user.UserProperties);
                model.Add(new UserHelperModel
                {
                    Type = UserType.LocalUser,
                    Username = user.UserName,
                    UserAlias = props.UserAlias,
                    EmailAddress = props.EmailAddress,
                    Permissions = (Permissions)user.Permissions,
                    Features = (Features)user.Features
                });
            }

            model.AddRange(filteredPlex.Select(user => new UserHelperModel
            {
                Type = UserType.PlexUser,
                Username = user.Username,
                UserAlias = user.UserAlias,
                EmailAddress = user.EmailAddress,
                Permissions = (Permissions)user.Permissions,
                Features = (Features)user.Features
            }));

            model.AddRange(filteredEmby.Select(user => new UserHelperModel
            {
                Type = UserType.EmbyUser,
                Username = user.Username,
                UserAlias = user.UserAlias,
                EmailAddress = user.EmailAddress,
                Permissions = (Permissions)user.Permissions,
                Features = (Features)user.Features
            }));


            return model;
        }

        public IEnumerable<UserHelperModel> GetUsersWithFeature(Features features)
        {
            var model = new List<UserHelperModel>();

            var localUsers = LocalUserRepository.GetAll().ToList();
            var plexUsers = PlexUserRepository.GetAll().ToList();
            var embyUsers = PlexUserRepository.GetAll().ToList();

            var filteredLocal = localUsers.Where(x => ((Features)x.Features).HasFlag(features));
            var filteredPlex = plexUsers.Where(x => ((Features)x.Features).HasFlag(features));
            var filteredEmby = embyUsers.Where(x => ((Features)x.Features).HasFlag(features));


            foreach (var user in filteredLocal)
            {
                var props = ByteConverterHelper.ReturnObject<UserProperties>(user.UserProperties);
                model.Add(new UserHelperModel
                {
                    Type = UserType.LocalUser,
                    Username = user.UserName,
                    UserAlias = props.UserAlias,
                    EmailAddress = props.EmailAddress,
                    Permissions = (Permissions)user.Permissions,
                    Features = (Features)user.Features
                });
            }

            model.AddRange(filteredPlex.Select(user => new UserHelperModel
            {
                Type = UserType.PlexUser,
                Username = user.Username,
                UserAlias = user.UserAlias,
                EmailAddress = user.EmailAddress,
                Permissions = (Permissions)user.Permissions,
                Features = (Features)user.Features
            }));

            model.AddRange(filteredEmby.Select(user => new UserHelperModel
            {
                Type = UserType.EmbyUser,
                Username = user.Username,
                UserAlias = user.UserAlias,
                EmailAddress = user.EmailAddress,
                Permissions = (Permissions)user.Permissions,
                Features = (Features)user.Features
            }));

            return model;
        }
    }
}