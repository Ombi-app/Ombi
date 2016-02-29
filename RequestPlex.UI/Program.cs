using System;

using Microsoft.Owin.Hosting;

using Mono.Data.Sqlite;

using Nancy.Hosting.Self;

using RequestPlex.Core;
using RequestPlex.Core.SettingModels;
using RequestPlex.Helpers;
using RequestPlex.Store;
using RequestPlex.Store.Repository.NZBDash.DataAccessLayer.Repository;

namespace RequestPlex.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = "http://localhost:3579/";
            var s = new Setup();
            s.SetupDb();

            var service = new SettingsServiceV2<RequestPlexSettings>(new JsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var settings = service.GetSettings();

            if (settings != null)
            {
                uri = $"http://localhost:{settings.Port}";
            }

            using (WebApp.Start<Startup>(uri))
            {
                Console.WriteLine("Running on {0}", uri);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
