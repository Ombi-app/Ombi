using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Ombi.Updater
{
    public class Installer
    {
        public void Start(StartupOptions options)
        {
            // Kill Ombi Process
            var p = new ProcessProvider();
            p.Kill(options.OmbiProcessId);


            // Make sure the process has been killed
            if (p.FindProcessByName("Ombi").Any())
            {
                // throw
            }

            MoveFiles(options);
            
            // Start Ombi
            StartOmbi(options);
        }

        private void StartOmbi(StartupOptions options)
        {
            var start = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = Path.Combine(options.ApplicationPath,"Ombi.exe")
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
