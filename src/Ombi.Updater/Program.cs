using System;
using System.Diagnostics;
using System.Linq;

namespace Ombi.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" =======================================");
            Console.WriteLine("        Starting the Ombi Updater"       );
            Console.WriteLine(" =======================================");


            var options = CheckArgs(args);
            var install = new Installer();
            install.Start(options);

        }

        private static StartupOptions CheckArgs(string[] args)
        {
            if(args.Length <= 1)
            {
                Console.WriteLine("No Args Provided... Exiting");
                Environment.Exit(1);
            }
            var startup = new StartupOptions
            {
                ApplicationPath = args[0],
                ProcessName = args[1],
            };

            var p = new ProcessProvider();
            var ombiProc = p.FindProcessByName(startup.ProcessName).FirstOrDefault();

            startup.OmbiProcessId = ombiProc?.Id ?? -1;

            return startup;
        }
    }

    public class StartupOptions
    {
        public string ProcessName { get; set; }
        public string ApplicationPath { get; set; }
        public int OmbiProcessId { get; set; }
    }
}