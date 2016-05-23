#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiModuleTests.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

using FluentValidation;

using Moq;

using Nancy;
using Nancy.Testing;
using Nancy.Validation;
using Nancy.Validation.FluentValidation;

using Newtonsoft.Json;

using NUnit.Framework;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Models;
using PlexRequests.UI.Modules;
using PlexRequests.UI.Validators;

using Ploeh.AutoFixture;

namespace PlexRequests.UI.Tests
{
    [TestFixture]
    public class ApiModuleTests
    {
        private ConfigurableBootstrapper Bootstrapper { get; set; }

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();
            var requests = fixture.CreateMany<RequestedModel>();
            var requestMock = new Mock<IRequestService>();
            var settingsMock = new Mock<ISettingsService<PlexRequestSettings>>();
            var userRepoMock = new Mock<IRepository<UsersModel>>();
            var mapperMock = new Mock<ICustomUserMapper>();
            var authSettingsMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var plexSettingsMock = new Mock<ISettingsService<PlexSettings>>();
            var cpMock = new Mock<ISettingsService<CouchPotatoSettings>>();
            var sonarrMock = new Mock<ISettingsService<SonarrSettings>>();
            var sickRageMock = new Mock<ISettingsService<SickRageSettings>>();
            var headphonesMock = new Mock<ISettingsService<HeadphonesSettings>>();

            var userModels = fixture.CreateMany<UsersModel>().ToList();
            userModels.Add(new UsersModel
            {
                UserName = "user1"
            });

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings { ApiKey = "api" });
            requestMock.Setup(x => x.GetAll()).Returns(requests);
            requestMock.Setup(x => x.Get(1)).Returns(requests.FirstOrDefault());
            requestMock.Setup(x => x.Get(99)).Returns(new RequestedModel());
            requestMock.Setup(x => x.DeleteRequest(It.IsAny<RequestedModel>()));

            userRepoMock.Setup(x => x.GetAll()).Returns(userModels);
            userRepoMock.Setup(x => x.Update(It.IsAny<UsersModel>())).Returns(true);

            mapperMock.Setup(x => x.ValidateUser("user1", It.IsAny<string>())).Returns(Guid.NewGuid());
            mapperMock.Setup(x => x.UpdatePassword("user1", "password", "newpassword")).Returns(true);

            authSettingsMock.Setup(x => x.SaveSettings(It.Is<AuthenticationSettings>(c => c.PlexAuthToken.Equals("abc")))).Returns(true);

            plexSettingsMock.Setup(x => x.GetSettings()).Returns(fixture.Create<PlexSettings>());
            plexSettingsMock.Setup(x => x.SaveSettings(It.Is<PlexSettings>(c => c.Ip.Equals("192")))).Returns(true);

            cpMock.Setup(x => x.GetSettings()).Returns(fixture.Create<CouchPotatoSettings>());
            cpMock.Setup(x => x.SaveSettings(It.Is<CouchPotatoSettings>(c => c.Ip.Equals("192")))).Returns(true);

            sonarrMock.Setup(x => x.GetSettings()).Returns(fixture.Create<SonarrSettings>());
            sonarrMock.Setup(x => x.SaveSettings(It.Is<SonarrSettings>(c => c.Ip.Equals("192")))).Returns(true);

            sickRageMock.Setup(x => x.GetSettings()).Returns(fixture.Create<SickRageSettings>());
            sickRageMock.Setup(x => x.SaveSettings(It.Is<SickRageSettings>(c => c.Ip.Equals("192")))).Returns(true);

            headphonesMock.Setup(x => x.GetSettings()).Returns(fixture.Create<HeadphonesSettings>());
            headphonesMock.Setup(x => x.SaveSettings(It.Is<HeadphonesSettings>(c => c.Ip.Equals("192")))).Returns(true);

            Bootstrapper = new ConfigurableBootstrapper(with =>
            {
                with.Module<ApiRequestModule>();
                with.Module<ApiUserModule>();
                with.Module<ApiSettingsModule>();

                with.Dependency(requestMock.Object);
                with.Dependency(settingsMock.Object);
                with.Dependency(userRepoMock.Object);
                with.Dependency(mapperMock.Object);
                with.Dependency(headphonesMock.Object);
                with.Dependency(authSettingsMock.Object);
                with.Dependency(plexSettingsMock.Object);
                with.Dependency(cpMock.Object);
                with.Dependency(sonarrMock.Object);
                with.Dependency(sickRageMock.Object);


                with.RootPathProvider<TestRootPathProvider>();
                with.ModelValidatorLocator(
                    new DefaultValidatorLocator(
                        new List<IModelValidatorFactory>
                        {
                                new FluentValidationValidatorFactory(
                                    new DefaultFluentAdapterFactory(new List<IFluentAdapter>()),
                                    new List<IValidator> { new RequestedModelValidator(), new UserViewModelValidator(), new PlexValidator() })
                        }));
            });
        }

        private Action<BrowserContext> GetBrowser()
        {
            return with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
            };
        }

        [Test]
        public void InvalidApiKey()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/api/requests", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "a");
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<List<RequestedModel>>>(result.Body.AsString());
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void GetAllRequests()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/api/requests", GetBrowser());
            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<List<RequestedModel>>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Data.Count, Is.GreaterThan(0));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public void GetSingleRequest()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/api/requests/1", GetBrowser());
            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<List<RequestedModel>>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Data.Count, Is.EqualTo(1));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public void GetSingleRequestThatDoesntExist()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/api/requests/99", GetBrowser());
            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<List<RequestedModel>>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Data.Count, Is.EqualTo(0));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void DeleteARequest()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Delete("/api/requests/1", GetBrowser());
            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());
            Assert.That(body.Data, Is.True);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public void DeleteARequestThatDoesNotExist()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Delete("/api/requests/99", GetBrowser());
            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());
            Assert.That(body.Data, Is.False);
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        [Description("Should file the validation")]
        public void CreateAEmptyRequest()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Post("/api/requests/", GetBrowser());
            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string[]>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null.Or.Empty);
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void UpdateUsersPassword()
        {
            var model = new UserUpdateViewModel
            {
                CurrentPassword = "password",
                NewPassword = "newpassword"
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Put("/api/credentials/user1", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null.Or.Empty);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public void UpdateInvalidUsersPassword()
        {
            var model = new UserUpdateViewModel
            {
                CurrentPassword = "password",
                NewPassword = "newpassword"
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Put("/api/credentials/user99", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string>>(result.Body.AsString());
            Assert.That(body.Data, Is.Null.Or.Empty);
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void UpdateUsersInvalidPassword()
        {
            var model = new UserUpdateViewModel
            {
                CurrentPassword = "password",
                NewPassword = "password2"
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Put("/api/credentials/user1", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string>>(result.Body.AsString());
            Assert.That(body.Data, Is.Null.Or.Empty);
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void UpdateUsersWithBadModel()
        {
            var model = new UserUpdateViewModel
            {
                CurrentPassword = null,
                NewPassword = "password2"
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Put("/api/credentials/user1", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string[]>>(result.Body.AsString());
            Assert.That(body.Data.Length, Is.GreaterThan(0));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void GetApiKey()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/apikey", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.Query("username", "user1");
                with.Query("password", "password");
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null.Or.Empty);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public void GetApiKeyWithBadCredentials()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/apikey", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.Query("username", "user");
                with.Query("password", "password");
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string>>(result.Body.AsString());
            Assert.That(body.Data, Is.Null.Or.Empty);
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.Not.Null.Or.Empty);
        }


        [Test]
        public void SaveNewAuthSettings()
        {
            var model = new AuthenticationSettings
            {
                Id = 1,
                PlexAuthToken = "abc",
                DeniedUsers = "abc",
                UsePassword = false,
                UserAuthentication = true
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("api/settings/authentication", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<ApiModel<string>>(result.Body.AsString());
            Assert.That(body.Data, Is.Not.Null.Or.Empty);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null.Or.Empty);
        }

        [Test]
        public void GetPlexSettings()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/settings/plex", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
            });

            var body = JsonConvert.DeserializeObject<ApiModel<PlexSettings>>(result.Body.AsString());

            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }

        [Test]
        public void SavePlexSettings()
        {
            var model = new PlexSettings()
            {
                Port = 231,
                Ip = "192",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/plex", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(true));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }
        [Test]
        public void SaveBadPlexSettings()
        {
            var model = new PlexSettings
            {
                Ip = "q",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/plex", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(false));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.EqualTo("Could not update the settings"));
        }

        [Test]
        public void GetCpSettings()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/settings/couchpotato", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
            });

            var body = JsonConvert.DeserializeObject<ApiModel<CouchPotatoSettings>>(result.Body.AsString());

            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }

        [Test]
        public void SaveCpSettings()
        {
            var model = new CouchPotatoSettings
            {
                Port = 231,
                Ip = "192",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/couchpotato", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(true));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }
        [Test]
        public void SaveBadCpSettings()
        {
            var model = new CouchPotatoSettings
            {
                Ip = "q",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/couchpotato", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(false));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.EqualTo("Could not update the settings"));
        }

        [Test]
        public void GetSonarrSettings()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/settings/sonarr", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
            });

            var body = JsonConvert.DeserializeObject<ApiModel<SonarrSettings>>(result.Body.AsString());

            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }

        [Test]
        public void SaveSonarrSettings()
        {
            var model = new SonarrSettings
            {
                Port = 231,
                Ip = "192",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/sonarr", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(true));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }
        [Test]
        public void SaveBadSonarrSettings()
        {
            var model = new SonarrSettings
            {
                Ip = "q",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/sonarr", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(false));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.EqualTo("Could not update the settings"));
        }

        [Test]
        public void GetSickRageSettings()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/settings/sickrage", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
            });

            var body = JsonConvert.DeserializeObject<ApiModel<SickRageSettings>>(result.Body.AsString());

            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }

        [Test]
        public void SaveSickRageSettings()
        {
            var model = new SickRageSettings
            {
                Port = 231,
                Ip = "192",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/sickrage", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(true));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }
        [Test]
        public void SaveBadSickRageSettings()
        {
            var model = new SickRageSettings
            {
                Ip = "q",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/sickrage", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(false));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.EqualTo("Could not update the settings"));
        }

        [Test]
        public void GetHeadphonesSettings()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/api/settings/headphones", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
            });

            var body = JsonConvert.DeserializeObject<ApiModel<HeadphonesSettings>>(result.Body.AsString());

            Assert.That(body.Data, Is.Not.Null);
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }

        [Test]
        public void SaveHeadphonesSettings()
        {
            var model = new HeadphonesSettings
            {
                Port = 231,
                Ip = "192",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/headphones", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(true));
            Assert.That(body.Error, Is.False);
            Assert.That(body.ErrorMessage, Is.Null);
        }
        [Test]
        public void SaveBadHeadphonesSettings()
        {
            var model = new HeadphonesSettings
            {
                Ip = "q",
                Ssl = true,
            };
            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/api/settings/headphones", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            Assert.That(body.Data, Is.EqualTo(false));
            Assert.That(body.Error, Is.True);
            Assert.That(body.ErrorMessage, Is.EqualTo("Could not update the settings"));
        }



        [TestCaseSource(nameof(AuthSettingsData))]
        public object SaveNewAuthSettings(object model)
        {

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("api/settings/authentication", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.Query("apikey", "api");
                with.JsonBody(model);
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));
            var body = JsonConvert.DeserializeObject<ApiModel<bool>>(result.Body.AsString());

            var retVal = new List<string> { body.ErrorMessage, body.Error.ToString(), body.Data.ToString() };
            return retVal;
        }

        private static IEnumerable<TestCaseData> AuthSettingsData
        {
            get
            {
                yield return
                    new TestCaseData(new AuthenticationSettings { Id = 1, PlexAuthToken = "abc", DeniedUsers = "abc", UsePassword = false, UserAuthentication = true })
                        .Returns(new List<string> { null, false.ToString(), true.ToString() });
            }
        }
    }
}