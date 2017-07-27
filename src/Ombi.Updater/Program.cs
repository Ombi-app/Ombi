using System;
using System.Diagnostics;
using System.Linq;

namespace Ombi.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine("       Starting the Ombi Updater"       );
            Console.WriteLine("=======================================");


            var options = CheckArgs(args);
            var install = new Installer();
            install.Start(options);

        }

        private static StartupOptions CheckArgs(string[] args)
        {
            if(args.Length <= 0)
            {
                Console.WriteLine("No Args Provided... Exiting");
                Environment.Exit(1);
            }

            var p = new ProcessProvider();
            var ombiProc = p.FindProcessByName("Ombi").FirstOrDefault();

            return new StartupOptions
            {
                ApplicationPath = args[0],
                OmbiProcessId = ombiProc?.Id ?? -1
            };
        }
    }

    public class StartupOptions
    {
        public string ApplicationPath { get; set; }
        public int OmbiProcessId { get; set; }
    }
}