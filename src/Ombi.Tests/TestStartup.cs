//using System;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Features.Authentication;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using Moq;
//using Ombi.Api.Emby;
//using Ombi.Api.Plex;
//using Ombi.Core.Authentication;
//using Ombi.Core.Settings;
//using Ombi.Core.Settings.Models.External;
//using Ombi.Models.Identity;
//using Ombi.Store.Context;
//using Ombi.Store.Entities;
//using Ombi.Store.Repository;

//namespace Ombi.Tests
//{
//    public class TestStartup
//    {
//        public IServiceProvider ConfigureServices(IServiceCollection services)
//        {
//            var _plexApi = new Mock<IPlexApi>();
//            var _embyApi = new Mock<IEmbyApi>();
//            var _tokenSettings = new Mock<IOptions<TokenAuthentication>>();
//            var _embySettings = new Mock<ISettingsService<EmbySettings>>();
//            var _plexSettings = new Mock<ISettingsService<PlexSettings>>();
//            var audit = new Mock<IAuditRepository>();
//            var tokenRepo = new Mock<ITokenRepository>();

//            services.AddEntityFrameworkInMemoryDatabase()
//                .AddDbContext<OmbiContext>();
//            services.AddIdentity<OmbiUser, IdentityRole>()
//                .AddEntityFrameworkStores<OmbiContext>().AddUserManager<OmbiUserManager>();

//            services.AddTransient(x => _plexApi.Object);
//            services.AddTransient(x => _embyApi.Object);
//            services.AddTransient(x => _tokenSettings.Object);
//            services.AddTransient(x => _embySettings.Object);
//            services.AddTransient(x => _plexSettings.Object);
//            services.AddTransient(x => audit.Object);
//            services.AddTransient(x => tokenRepo.Object);
//            // Taken from https://github.com/aspnet/MusicStore/blob/dev/test/MusicStore.Test/ManageControllerTest.cs (and modified)
//            var context = new DefaultHttpContext();
//            context.Features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature());
//            services.AddSingleton<IHttpContextAccessor>(h => new HttpContextAccessor { HttpContext = context });


//            services.Configure<IdentityOptions>(options =>
//            {
//                options.Password.RequireDigit = false;
//                options.Password.RequiredLength = 1;
//                options.Password.RequireLowercase = false;
//                options.Password.RequireNonAlphanumeric = false;
//                options.Password.RequireUppercase = false;
//                options.User.AllowedUserNameCharacters = string.Empty;
//            });

//            return services.BuildServiceProvider();

//        }

//        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
//        {
            
//        }
//    }
//}