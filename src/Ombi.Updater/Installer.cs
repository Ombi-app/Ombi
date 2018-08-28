using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Ombi.Updater
{
    public class Installer : IInstaller
    {
        public Installer(ILogger<Installer> log)
        {
            _log = log;
        }

        private readonly ILogger<Installer> _log;

        public void Start(StartupOptions opt)
        {
            // Kill Ombi Process
            var p = new ProcessProvider();
            bool killed = false;
            try
            {


                killed = p.Kill(opt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (!killed)
            {

                _log.LogDebug("Couldn't kill the ombi process");
                return;
            }

            _log.LogDebug("Starting to move the files");
            MoveFiles(opt);
            _log.LogDebug("Files replaced");
            // Start Ombi
            StartOmbi(opt);
        }

        private void StartOmbi(StartupOptions options)
        {
            var startupArgsBuilder = new StringBuilder();
            _log.LogDebug("Starting ombi");
            var fileName = "Ombi.exe";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "Ombi";
            }
            if (options.IsWindowsService)
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        Arguments = $"/C net start \"{options.WindowsServiceName}\""
                    };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(options.Host))
                {
                    startupArgsBuilder.Append($"--host {options.Host} ");
                }
                if (!string.IsNullOrEmpty(options.Storage))
                {
                    startupArgsBuilder.Append($"--storage {options.Storage}");
                }

                var start = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = Path.Combine(options.ApplicationPath, fileName),
                    WorkingDirectory = options.ApplicationPath,
                    Arguments = startupArgsBuilder.ToString()
                };
                using (var proc = new Process { StartInfo = start })
                {
                    proc.Start();
                }
            }

            _log.LogDebug($"Ombi started, now exiting");
            _log.LogDebug($"Working dir: {options.ApplicationPath} (Application Path)");
            _log.LogDebug($"Filename: {Path.Combine(options.ApplicationPath, fileName)}");
            _log.LogDebug($"Startup Args: {startupArgsBuilder.ToString()}");
            Environment.Exit(0);
        }

        private void MoveFiles(StartupOptions options)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            location = Path.GetDirectoryName(location);
            _log.LogDebug("We are currently in dir {0}", location);
            var updatedLocation = Directory.GetParent(location).FullName;
            _log.LogDebug("The files are in {0}", updatedLocation); // Since the updater is a folder deeper
            _log.LogDebug("Ombi is installed at {0}", options.ApplicationPath);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(updatedLocation, "*",
                SearchOption.AllDirectories))
            {
                var newDir = dirPath.Replace(updatedLocation, options.ApplicationPath);
                Directory.CreateDirectory(newDir);
                _log.LogDebug("Created dir {0}", newDir);
            }
            //Copy all the files & Replaces any files with the same name
            foreach (string currentPath in Directory.GetFiles(updatedLocation, "*.*",
                SearchOption.AllDirectories))
            {
                var newFile = currentPath.Replace(updatedLocation, options.ApplicationPath);
                File.Copy(currentPath, newFile, true);
                _log.LogDebug("Replaced file {0}", newFile);
            }
        }
    }
}
