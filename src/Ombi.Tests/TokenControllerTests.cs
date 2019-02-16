//using System.Net.Http;
//using System.Threading.Tasks;
//using AutoMapper;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Features.Authentication;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using Moq;
//using NUnit.Framework;
//using Ombi.Api.Emby;
//using Ombi.Api.Plex;
//using Ombi.Controllers;
//using Ombi.Core.Authentication;
//using Ombi.Core.Settings;
//using Ombi.Core.Settings.Models.External;
//using Ombi.Models.Identity;
//using Ombi.Notifications;
//using Ombi.Schedule.Jobs.Ombi;
//using Ombi.Settings.Settings.Models;
//using Ombi.Settings.Settings.Models.Notifications;
//using Ombi.Store.Context;
//using Ombi.Store.Entities;
//using Ombi.Store.Repository;
//using Microsoft.AspNetCore.Hosting.Server;
//using Microsoft.AspNetCore.TestHost;
//using Newtonsoft.Json;
//using Ombi.Models;

//namespace Ombi.Tests
//{
//    [TestFixture]
//    [Ignore("TODO")]
//    public class TokenControllerTests
//    {
//        [SetUp]
//        public void Setup()
//        {
//            _testServer = new TestServer(new WebHostBuilder()
//                .UseStartup<TestStartup>());
//            _client = _testServer.CreateClient();
//        }

//        private TestServer _testServer;
//        private HttpClient _client;
        

//        [Test]
//        public async Task GetToken_FromValid_LocalUser()
//        {
//            var model = new UserAuthModel
//            {
//                Password = "a",
//                Username = "a"
//            };
//            HttpResponseMessage response = await _client.PostAsync("/api/v1/token", new StringContent(JsonConvert.SerializeObject(model)) );
//        }
//    }
//}