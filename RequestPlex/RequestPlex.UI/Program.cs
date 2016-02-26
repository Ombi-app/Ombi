using System;

using Microsoft.Owin.Hosting;

using Nancy.Hosting.Self;

using RequestPlex.Core;

namespace RequestPlex.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new SettingsService();
            var settings = service.GetSettings();

            var uri = "http://localhost:3579/";
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
