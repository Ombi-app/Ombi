using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.Friends;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Test.Common;
using Ombi.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexUserImporterTests
    {
        private List<OmbiUser> _users = new List<OmbiUser>
        {
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="abc", NormalizedUserName = "ABC", UserType = UserType.LocalUser},
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="sys", NormalizedUserName = "SYS", UserType = UserType.SystemUser},
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="plex", NormalizedUserName = "PLEX", UserType = UserType.PlexUser, ProviderUserId = "PLEX_ID", Email = "dupe"},
        };
        private AutoMocker _mocker;
        private PlexUserImporter _subject;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMocker();

            var um = MockHelper.MockUserManager(_users);
            var hub = SignalRHelper.MockHub<NotificationHub>();

            _mocker.Use(um);
            _mocker.Use(hub);

            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings
            {
                Enable = true,
                Servers = new List<PlexServers>
                {
                    new PlexServers { Name = "Test", MachineIdentifier = "123", PlexAuthToken = "abc" }
                }
            });
            _subject = _mocker.CreateInstance<PlexUserImporter>();
        }

        [Test]
        public async Task Import_Exits_WhenNot_Enabled()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = false, ImportPlexUsers = false });

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        [Test]
        public async Task Import_Exits_When_Plex_Not_Enabled()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = true, ImportPlexUsers = true });

            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = false });

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        [Test]
        public async Task Import_Exits_When_Plex_No_AuthToken()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = true, ImportPlexUsers = true });
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings
            {
                Enable = true,
                Servers = new List<PlexServers>
                {
                    new PlexServers { Name = "Test", MachineIdentifier = "123", PlexAuthToken = null }
                }
            });

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        [Test]
        public async Task Import_Only_Imports_Plex_Admin()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = true, ImportPlexUsers = false });
            _mocker.Setup<IPlexApi, Task<PlexAccount>>(x => x.GetAccount(It.IsAny<string>())).ReturnsAsync(new PlexAccount
            {
                user = new User
                {
                    email = "email",
                    authentication_token = "user_token",
                    title = "user_title",
                    username = "user_username",
                    id = "user_id",
                }
            });
            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.CreateAsync(It.Is<OmbiUser>(x => x.UserName == "user_username" && x.Email == "email" && x.ProviderUserId == "user_id" && x.UserType == UserType.PlexUser)))
                .ReturnsAsync(IdentityResult.Success);
            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.AddToRoleAsync(It.Is<OmbiUser>(x => x.UserName == "user_username"), It.Is<string>(x => x == OmbiRoles.Admin)))
                .ReturnsAsync(IdentityResult.Success);

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Once);
        }

        [Test]
        public async Task Import_Only_Imports_Plex_Admin_Already_Exists()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = true, ImportPlexUsers = false });
            _mocker.Setup<IPlexApi, Task<PlexAccount>>(x => x.GetAccount(It.IsAny<string>())).ReturnsAsync(new PlexAccount
            {
                user = new User
                {
                    email = "email",
                    authentication_token = "user_token",
                    title = "user_title",
                    username = "newUsername",
                    id = "PLEX_ID",
                }
            });
            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.CreateAsync(It.Is<OmbiUser>(x => x.UserName == "user_username" && x.Email == "email" && x.ProviderUserId == "user_id" && x.UserType == UserType.PlexUser)))
                .ReturnsAsync(IdentityResult.Success);
            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.AddToRoleAsync(It.Is<OmbiUser>(x => x.UserName == "user_username"), It.Is<string>(x => x == OmbiRoles.Admin)))
                .ReturnsAsync(IdentityResult.Success);

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
            _mocker.Verify<OmbiUserManager>(x => x.UpdateAsync(It.Is<OmbiUser>(x => x.Email == "email" && x.UserName == "newUsername")), Times.Once);
        }

        [Test]
        public async Task Import_Only_Imports_Plex_Admin_Username_Clash()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = true, ImportPlexUsers = false });
            _mocker.Setup<IPlexApi, Task<PlexAccount>>(x => x.GetAccount(It.IsAny<string>())).ReturnsAsync(new PlexAccount
            {
                user = new User
                {
                    email = "email",
                    authentication_token = "user_token",
                    title = "user_title",
                    username = "abc",
                    id = "nah",
                }
            });
            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.CreateAsync(It.Is<OmbiUser>(x => x.UserName == "user_username" && x.Email == "email" && x.ProviderUserId == "user_id" && x.UserType == UserType.PlexUser)))
                .ReturnsAsync(IdentityResult.Success);
            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.AddToRoleAsync(It.Is<OmbiUser>(x => x.UserName == "user_username"), It.Is<string>(x => x == OmbiRoles.Admin)))
                .ReturnsAsync(IdentityResult.Success);

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
            _mocker.Verify<OmbiUserManager>(x => x.UpdateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }



        [Test]
        public async Task Import_Doesnt_Import_Banned_Users()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = false, ImportPlexUsers = true, BannedPlexUserIds = new List<string> { "Banned" } });
            _mocker.Setup<IPlexApi, Task<PlexFriends>>(x => x.GetUsers(It.IsAny<string>())).ReturnsAsync(new PlexFriends
            {
                User = new UserFriends[]
                {
                    new UserFriends
                    {
                        Email = "email",
                        Id = "Banned",
                        Title = "title",
                        Username = "username"
                    }
                }
            });

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        [Test]
        public async Task Import_Doesnt_Import_Managed_User()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = false, ImportPlexUsers = true });
            _mocker.Setup<IPlexApi, Task<PlexFriends>>(x => x.GetUsers(It.IsAny<string>())).ReturnsAsync(new PlexFriends
            {
                User = new UserFriends[]
                {
                    new UserFriends
                    {
                        Email = "email",
                        Id = "id",
                        Title = "title",
                    }
                }
            });

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        [Test]
        public async Task Import_Doesnt_Import_DuplicateEmail()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = false, ImportPlexUsers = true });
            _mocker.Setup<IPlexApi, Task<PlexFriends>>(x => x.GetUsers(It.IsAny<string>())).ReturnsAsync(new PlexFriends
            {
                User = new UserFriends[]
                {
                    new UserFriends
                    {
                        Email = "dupe",
                        Id = "id",
                        Title = "title",
                        Username = "username"
                    }
                }
            });

            _mocker.Setup<OmbiUserManager, Task<OmbiUser>>(x => x.FindByEmailAsync("dupe")).ReturnsAsync(new OmbiUser());

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        [Test]
        public async Task Import_Created_Plex_User()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { ImportPlexAdmin = false, ImportPlexUsers = true, DefaultRoles = new List<string>
                {
                    OmbiRoles.RequestMovie
                }
                });
            _mocker.Setup<IPlexApi, Task<PlexFriends>>(x => x.GetUsers(It.IsAny<string>())).ReturnsAsync(new PlexFriends
            {
                User = new UserFriends[]
                {
                    new UserFriends
                    {
                        Email = "email",
                        Id = "id",
                        Username = "plex"
                    }
                }
            });

            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.CreateAsync(It.Is<OmbiUser>(x => x.UserName == "plex" && x.Email == "email" && x.ProviderUserId == "id" && x.UserType == UserType.PlexUser)))
                .ReturnsAsync(IdentityResult.Success);

            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.AddToRoleAsync(It.Is<OmbiUser>(x => x.UserName == "plex"), OmbiRoles.RequestMovie))
                .ReturnsAsync(IdentityResult.Success);

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Once);
        }

        [Test]
        public async Task Import_Update_Plex_User()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings
                {
                    ImportPlexAdmin = false,
                    ImportPlexUsers = true,
                    DefaultRoles = new List<string>
                {
                    OmbiRoles.RequestMovie
                }
                });
            _mocker.Setup<IPlexApi, Task<PlexFriends>>(x => x.GetUsers(It.IsAny<string>())).ReturnsAsync(new PlexFriends
            {
                User = new UserFriends[]
                {
                    new UserFriends
                    {
                        Email = "email",
                        Id = "PLEX_ID",
                        Username = "user"
                    }
                }
            });

            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.CreateAsync(It.Is<OmbiUser>(x => x.UserName == "plex" && x.Email == "email" && x.ProviderUserId == "id" && x.UserType == UserType.PlexUser)))
                .ReturnsAsync(IdentityResult.Success);

            _mocker.Setup<OmbiUserManager, Task<IdentityResult>>(x => x.AddToRoleAsync(It.Is<OmbiUser>(x => x.UserName == "plex"), OmbiRoles.RequestMovie))
                .ReturnsAsync(IdentityResult.Success);

            await _subject.Execute(null);

            _mocker.Verify<OmbiUserManager>(x => x.UpdateAsync(It.Is<OmbiUser>(x => x.ProviderUserId == "PLEX_ID" && x.Email == "email" && x.UserName == "user")), Times.Once);
        }
    }
}
