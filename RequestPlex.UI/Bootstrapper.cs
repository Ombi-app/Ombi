using Mono.Data.Sqlite;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

using RequestPlex.Core;
using RequestPlex.Core.SettingModels;
using RequestPlex.Helpers;
using RequestPlex.Store;
using RequestPlex.Store.Models;
using RequestPlex.Store.Repository.NZBDash.DataAccessLayer.Repository;

using FormsAuthentication = Nancy.Authentication.Forms.FormsAuthentication;

namespace RequestPlex.UI
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            
            container.Register<IUserMapper, UserMapper>();
            base.ConfigureRequestContainer(container, context);

            container.Register<ISqliteConfiguration, DbConfiguration>(new DbConfiguration(new SqliteFactory()));
            
            container.Register<ISettingsRepository, JsonRepository>();
            container.Register<ICacheProvider, MemoryCacheProvider>();


            container.Register<ISettingsService<RequestPlexSettings>, SettingsServiceV2<RequestPlexSettings>>();
            container.Register<IRepository<RequestedModel>, GenericRepository<RequestedModel>>();

        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            StaticConfiguration.DisableErrorTraces = false;
            base.ApplicationStartup(container, pipelines);

            // Enable forms auth
            var formsAuthConfiguration = new FormsAuthenticationConfiguration
            {
                RedirectUrl = "~/login",
                UserMapper = container.Resolve<IUserMapper>()
            };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }
    }
}