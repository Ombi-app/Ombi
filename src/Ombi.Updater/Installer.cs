using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ombi.Updater
{
    public class Installer
    {
        public void Start(StartupOptions opt)
        {
            // Kill Ombi Process
            var p = new ProcessProvider();
            p.Kill(opt.OmbiProcessId);
            
            // Make sure the process has been killed
            while (p.FindProcessByName(opt.ProcessName).Any())
            {
                Thread.Sleep(500);
                Console.WriteLine("Found another process called Ombi, KILLING!");
                var proc = p.FindProcessByName(opt.ProcessName).FirstOrDefault();
                if (proc != null)
                {
                    Console.WriteLine($"[{proc.Id}] - {proc.Name} - Path: {proc.StartPath}");
                    p.Kill(proc.Id);
                }
            }

            MoveFiles(opt);
            
            // Start Ombi
            StartOmbi(opt);
        }

        private void StartOmbi(StartupOptions options)
        {
            var fileName = "Ombi.exe";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "Ombi";
            }

            var start = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = Path.Combine(options.ApplicationPath,fileName),
                WorkingDirectory = options.ApplicationPath
            };
            using (var proc = new Process { StartInfo = start })
            {
                proc.Start();
            }
            
            Environment.Exit(0);
        }

        private void MoveFiles(StartupOptions options)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            location = Path.GetDirectoryName(location);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(location, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(location, options.ApplicationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(location, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(location, options.ApplicationPath), true);
        }
    }
}
