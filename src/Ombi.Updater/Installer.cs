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
            try
            {


                p.Kill(opt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            // Make sure the process has been killed
            while (p.FindProcessByName(opt.ProcessName).Any())
            {
                Thread.Sleep(500);
                _log.LogDebug("Found another process called {0}, KILLING!", opt.ProcessName);
                var proc = p.FindProcessByName(opt.ProcessName).FirstOrDefault();
                if (proc != null)
                {
                    _log.LogDebug($"[{proc.Id}] - {proc.Name} - Path: {proc.StartPath}");
                    opt.OmbiProcessId = proc.Id;
                    p.Kill(opt);
                }
            }

            _log.LogDebug("Starting to move the files");
            MoveFiles(opt);
            _log.LogDebug("Files replaced");
            // Start Ombi
            StartOmbi(opt);
        }

        private void StartOmbi(StartupOptions options)
        {
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
                var startupArgsBuilder = new StringBuilder();
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

            _log.LogDebug("Ombi started, now exiting");
            Environment.Exit(0);
        }

        private void MoveFiles(StartupOptions options)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            location = Path.GetDirectoryName(location);
            _log.LogDebug("We are currently in dir {0}", location);
            _log.LogDebug("Ombi is installed at {0}", options.ApplicationPath);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(location, "*",
                SearchOption.AllDirectories))
            {
                var newDir = dirPath.Replace(location, options.ApplicationPath);
                Directory.CreateDirectory(newDir);
                _log.LogDebug("Created dir {0}", newDir);
            }
            //Copy all the files & Replaces any files with the same name
            foreach (string currentPath in Directory.GetFiles(location, "*.*",
                SearchOption.AllDirectories))
            {
                var newFile = currentPath.Replace(location, options.ApplicationPath);
                File.Copy(currentPath, newFile, true);
                _log.LogDebug("Replaced file {0}", newFile);
            }
        }
    }
}
