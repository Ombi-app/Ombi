
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using AutoMapper;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Features.Authentication;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using Moq;
//using NUnit.Framework;
//using Ombi.Api.Emby;
//using Ombi.Api.Plex;
//using Ombi.Config;
//using Ombi.Controllers;
//using Ombi.Core.Authentication;
//using Ombi.Core.Settings;
//using Ombi.Core.Settings.Models.External;
//using Ombi.Models;
//using Ombi.Notifications;
//using Ombi.Schedule.Jobs.Ombi;
//using Ombi.Settings.Settings.Models;
//using Ombi.Settings.Settings.Models.Notifications;
//using Ombi.Store.Context;
//using Ombi.Store.Entities;

//namespace Ombi.Tests
//{
//    [TestFixture]
//    [Ignore("Need to sort out the DB, looks like it's using the real one...")]
//    public class IdentityControllerTests
//    {
//        [SetUp]
//        public void Setup()
//        {
//            _plexApi = new Mock<IPlexApi>();
//            _embyApi = new Mock<IEmbyApi>();
//            _mapper = new Mock<IMapper>();
//            _emailProvider = new Mock<IEmailProvider>();
//            _emailSettings = new Mock<ISettingsService<EmailNotificationSettings>>();
//            _customizationSettings = new Mock<ISettingsService<CustomizationSettings>>();
//            _welcomeEmail = new Mock<IWelcomeEmail>();
//            _embySettings = new Mock<ISettingsService<EmbySettings>>();
//            _plexSettings = new Mock<ISettingsService<PlexSettings>>();

//            var services = new ServiceCollection();
//            services.AddEntityFrameworkInMemoryDatabase()
//                .AddDbContext<OmbiContext>();
//            services.AddIdentity<OmbiUser, IdentityRole>()
//                .AddEntityFrameworkStores<OmbiContext>().AddUserManager<OmbiUserManager>();

//            services.AddTransient(x => _plexApi.Object);
//            services.AddTransient(x => _embyApi.Object);
//            services.AddTransient(x => _customizationSettings.Object);
//            services.AddTransient(x => _welcomeEmail.Object);
//            services.AddTransient(x => _emailSettings.Object);
//            services.AddTransient(x => _emailProvider.Object);
//            services.AddTransient(x => _mapper.Object);
//            services.AddTransient(x => _embySettings.Object);
//            services.AddTransient(x => _plexSettings.Object);

//            services.Configure<IdentityOptions>(options =>
//            {
//                options.Password.RequireDigit = false;
//                options.Password.RequiredLength = 1;
//                options.Password.RequireLowercase = false;
//                options.Password.RequireNonAlphanumeric = false;
//                options.Password.RequireUppercase = false;
//                options.User.AllowedUserNameCharacters = string.Empty;
//            });

//            // Taken from https://github.com/aspnet/MusicStore/blob/dev/test/MusicStore.Test/ManageControllerTest.cs (and modified)
//            var context = new DefaultHttpContext();
//            context.Features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature());
//            services.AddSingleton<IHttpContextAccessor>(h => new HttpContextAccessor { HttpContext = context });
//            _serviceProvider = services.BuildServiceProvider();
//            _userManager = _serviceProvider.GetRequiredService<OmbiUserManager>();

//            Controller = new IdentityController(_userManager, _mapper.Object,
//                _serviceProvider.GetService<RoleManager<IdentityRole>>(), _emailProvider.Object,
//                _emailSettings.Object, _customizationSettings.Object, _welcomeEmail.Object, null, null, null, null,
//                null, null, null, null, null);
//        }

//        private OmbiUserManager _userManager;
//        private Mock<IEmailProvider> _emailProvider;
//        private Mock<ISettingsService<EmailNotificationSettings>> _emailSettings;
//        private Mock<ISettingsService<CustomizationSettings>> _customizationSettings;
//        private Mock<ISettingsService<EmbySettings>> _embySettings;
//        private Mock<ISettingsService<PlexSettings>> _plexSettings;
//        private Mock<IWelcomeEmail> _welcomeEmail;
//        private Mock<IMapper> _mapper;
//        private Mock<IPlexApi> _plexApi;
//        private Mock<IEmbyApi> _embyApi;
//        private ServiceProvider _serviceProvider;

//        private IdentityController Controller { get; set; }

//        [Test]
//        public async Task CreateWizardUser_Should_CreateUser_WhenThereAreNoOtherUsers()
//        {
//            var model = new CreateUserWizardModel()
//            {
//                Password = "a",
//                Username = "b"
//            };

//            var result = await Controller.CreateWizardUser(model);

//            Assert.That(result, Is.True);
//        }

//        [Test]
//        public async Task CreateWizardUser_ShouldNot_CreateUser_WhenThereAreOtherUsers()
//        {
//            var um = _serviceProvider.GetService<OmbiUserManager>();
//            var r  = await um.CreateAsync(new OmbiUser { UserName = "aaaa",UserType = UserType.LocalUser}, "bbb");
//            var model = new CreateUserWizardModel
//            {
//                Password = "a",
//                Username = "b"
//            };

//            var result = await Controller.CreateWizardUser(model);

//            Assert.That(result, Is.False);
//        }
//    }
//}
