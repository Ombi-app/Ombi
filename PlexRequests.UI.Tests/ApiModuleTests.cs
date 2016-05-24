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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

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
using PlexRequests.Helpers;
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

            var userModels = fixture.CreateMany<UsersModel>().ToList();
            userModels.Add(new UsersModel
            {
                UserName = "user1"
            });

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings {ApiKey = "api"});
            requestMock.Setup(x => x.GetAll()).Returns(requests);
            requestMock.Setup(x => x.Get(1)).Returns(requests.FirstOrDefault());
            requestMock.Setup(x => x.Get(99)).Returns(new RequestedModel());
            requestMock.Setup(x => x.DeleteRequest(It.IsAny<RequestedModel>()));

            userRepoMock.Setup(x => x.GetAll()).Returns(userModels);
            userRepoMock.Setup(x => x.Update(It.IsAny<UsersModel>())).Returns(true);

            mapperMock.Setup(x => x.ValidateUser("user1", It.IsAny<string>())).Returns(Guid.NewGuid());
            mapperMock.Setup(x => x.UpdatePassword("user1", "password", "newpassword")).Returns(true);

            authSettingsMock.Setup(x => x.SaveSettings(It.Is<AuthenticationSettings>(c => c.PlexAuthToken.Equals("abc")))).Returns(true);

            Bootstrapper = new ConfigurableBootstrapper(with =>
            {
                with.Module<ApiRequestModule>();
                with.Module<ApiUserModule>();
                with.Module<ApiSettingsModule>();

                with.Dependency(requestMock.Object);
                with.Dependency(settingsMock.Object);
                with.Dependency(userRepoMock.Object);
                with.Dependency(mapperMock.Object);
                with.Dependency(authSettingsMock.Object);


                with.RootPathProvider<TestRootPathProvider>();
                with.ModelValidatorLocator(
                    new DefaultValidatorLocator(
                        new List<IModelValidatorFactory>
                        {
                                new FluentValidationValidatorFactory(
                                    new DefaultFluentAdapterFactory(new List<IFluentAdapter>()),
                                    new List<IValidator> { new RequestedModelValidator(), new UserViewModelValidator() })
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
                with.Query("apikey","a");
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
                with.Query("username","user1");
                with.Query("password","password");
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