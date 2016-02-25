using System;

using Microsoft.Owin.Hosting;

using Nancy.Hosting.Self;

namespace RequestPlex.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri =
                "http://localhost:3579";

            using (WebApp.Start<Startup>(uri))
            {
                Console.WriteLine("Running on {0}", uri);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
