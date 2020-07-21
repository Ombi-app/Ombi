using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Processor;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Schedule.Processor;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Updater;
using Quartz;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class OmbiAutomaticUpdater : IOmbiAutomaticUpdater
    {
        public OmbiAutomaticUpdater(ILogger<OmbiAutomaticUpdater> log, IChangeLogProcessor service,
            ISettingsService<UpdateSettings> s, IProcessProvider proc, IApplicationConfigRepository appConfig)
        {
            Logger = log;
            Processor = service;
            Settings = s;
            _processProvider = proc;
            _appConfig = appConfig;
        }

        private ILogger<OmbiAutomaticUpdater> Logger { get; }
        private IChangeLogProcessor Processor { get; }
        private ISettingsService<UpdateSettings> Settings { get; }
        private readonly IProcessProvider _processProvider;
        private readonly IApplicationConfigRepository _appConfig;

        public string[] GetVersion()
        {
            var productVersion = AssemblyHelper.GetRuntimeVersion();
            var productArray = productVersion.Split('-');
            return productArray;
        }
        public async Task<bool> UpdateAvailable(string branch, string currentVersion)
        {

            var updates = await Processor.Process(branch);
            var serverVersion = updates.UpdateVersionString;
            return !serverVersion.Equals(currentVersion, StringComparison.CurrentCultureIgnoreCase);

        }

        public async Task Execute(IJobExecutionContext job)
        {
            Logger.LogDebug(LoggingEvents.Updater, "Starting Update job");

            var settings = await Settings.GetSettingsAsync();
            if (!settings.AutoUpdateEnabled && !settings.TestMode)
            {
                Logger.LogDebug(LoggingEvents.Updater, "Auto update is not enabled");
                return;
            }

            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Logger.LogDebug(LoggingEvents.Updater, "Path: {0}", currentLocation);

            var productVersion = AssemblyHelper.GetRuntimeVersion();
            Logger.LogDebug(LoggingEvents.Updater, "Product Version {0}", productVersion);
            var serverVersion = string.Empty;
            try
            {
                var productArray = GetVersion();
                var version = productArray[0];
                Logger.LogDebug(LoggingEvents.Updater, "Version {0}", version);
                var branch = productArray[1];
                Logger.LogDebug(LoggingEvents.Updater, "Branch Version {0}", branch);

                Logger.LogDebug(LoggingEvents.Updater, "Version {0}", version);
                Logger.LogDebug(LoggingEvents.Updater, "Branch {0}", branch);

                Logger.LogDebug(LoggingEvents.Updater, "Looking for updates now");
                //TODO this fails because the branch = featureupdater when it should be feature/updater
                var updates = await Processor.Process(branch);
                Logger.LogDebug(LoggingEvents.Updater, "Updates: {0}", updates);


                serverVersion = updates.UpdateVersionString;

                Logger.LogDebug(LoggingEvents.Updater, "Service Version {0}", updates.UpdateVersionString);


                if (!serverVersion.Equals(version, StringComparison.CurrentCultureIgnoreCase) || settings.TestMode)
                {
                    // Let's download the correct zip
                    var desc = RuntimeInformation.OSDescription;
                    var process = RuntimeInformation.ProcessArchitecture;

                    Logger.LogDebug(LoggingEvents.Updater, "OS Information: {0} {1}", desc, process);
                    Downloads download;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Logger.LogDebug(LoggingEvents.Updater, "We are Windows");
                        if (process == Architecture.X64)
                        {
                            download = updates.Downloads.FirstOrDefault(x =>
                                x.Name.Contains("windows.", CompareOptions.IgnoreCase));
                        }
                        else
                        {
                            download = updates.Downloads.FirstOrDefault(x =>
                                x.Name.Contains("windows-32bit", CompareOptions.IgnoreCase));
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Logger.LogDebug(LoggingEvents.Updater, "We are OSX");
                        download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("osx", CompareOptions.IgnoreCase));
                    }
                    else
                    {
                        Logger.LogDebug(LoggingEvents.Updater, "We are linux");
                        if (process == Architecture.Arm)
                        {
                            download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("arm.", CompareOptions.IgnoreCase));
                        }
                        else if (process == Architecture.Arm64)
                        {
                            download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("arm64.", CompareOptions.IgnoreCase));
                        }
                        else
                        {
                            download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("linux.", CompareOptions.IgnoreCase));
                        }
                    }
                    if (download == null)
                    {
                        Logger.LogDebug(LoggingEvents.Updater, "There were no downloads");
                        return;
                    }

                    Logger.LogDebug(LoggingEvents.Updater, "Found the download! {0}", download.Name);
                    Logger.LogDebug(LoggingEvents.Updater, "URL {0}", download.Url);

                    Logger.LogDebug(LoggingEvents.Updater, "Clearing out Temp Path");
                    var tempPath = Path.Combine(currentLocation, "TempUpdate");
                    if (Directory.Exists(tempPath))
                    {
                        DeleteDirectory(tempPath);
                    }

                    // Temp Path
                    Directory.CreateDirectory(tempPath);


                    if (settings.UseScript && !settings.WindowsService)
                    {
                        RunScript(settings, download.Url);
                        return;
                    }

                    // Download it
                    Logger.LogDebug(LoggingEvents.Updater, "Downloading the file {0} from {1}", download.Name, download.Url);
                    var extension = download.Name.Split('.').Last();
                    var zipDir = Path.Combine(currentLocation, $"Ombi.{extension}");
                    Logger.LogDebug(LoggingEvents.Updater, "Zip Dir: {0}", zipDir);
                    try
                    {
                        if (File.Exists(zipDir))
                        {
                            File.Delete(zipDir);
                        }

                        Logger.LogDebug(LoggingEvents.Updater, "Starting Download");
                        await DownloadAsync(download.Url, zipDir);
                        Logger.LogDebug(LoggingEvents.Updater, "Finished Download");
                    }
                    catch (Exception e)
                    {
                        Logger.LogDebug(LoggingEvents.Updater, "Error when downloading");
                        Logger.LogDebug(LoggingEvents.Updater, e.Message);
                        Logger.LogError(LoggingEvents.Updater, e, "Error when downloading the zip");
                        throw;
                    }

                    // Extract it
                    Logger.LogDebug(LoggingEvents.Updater, "Extracting ZIP");
                    Extract(zipDir, tempPath);

                    Logger.LogDebug(LoggingEvents.Updater, "Finished Extracting files");
                    Logger.LogDebug(LoggingEvents.Updater, "Starting the Ombi.Updater process");
                    var updaterExtension = string.Empty;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        updaterExtension = ".exe";
                    }
                    var updaterFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        "TempUpdate", "updater", $"Ombi.Updater{updaterExtension}");

                    // Make sure the file is an executable
                    //ExecLinuxCommand($"chmod +x {updaterFile}");


                    // There must be an update
                    var start = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true, // Ignored if UseShellExecute is set to true
                        FileName = updaterFile,
                        Arguments = GetArgs(settings),
                        WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "TempUpdate"),
                    };
                    //if (settings.Username.HasValue())
                    //{
                    //    start.UserName = settings.Username;
                    //}
                    //if (settings.Password.HasValue())
                    //{
                    //    start.Password = settings.Password.ToSecureString();
                    //}
                    using (var proc = new Process { StartInfo = start })
                    {
                        proc.Start();
                    }


                    Logger.LogDebug(LoggingEvents.Updater, "Bye bye");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Exception thrown in the OmbiUpdater, see previous messages");
                throw;
            }
        }

        private string GetArgs(UpdateSettings settings)
        {
            var url = _appConfig.Get(ConfigurationTypes.Url);
            var storage = _appConfig.Get(ConfigurationTypes.StoragePath);

            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var processName = (settings.ProcessName.HasValue() ? settings.ProcessName : "Ombi");

            var sb = new StringBuilder();
            sb.Append($"--applicationPath \"{currentLocation}\" --processname \"{processName}\" ");
            //if (settings.WindowsService)
            //{
            //    sb.Append($"--windowsServiceName \"{settings.WindowsServiceName}\" ");
            //}
            var sb2 = new StringBuilder();
            if (url?.Value.HasValue() ?? false)
            {
                sb2.Append($" --host {url.Value}");
            }
            if (storage?.Value.HasValue() ?? false)
            {
                sb2.Append($" --storage {storage.Value}");
            }

            return sb.ToString();
        }

        private void RunScript(UpdateSettings settings, string downloadUrl)
        {
            var scriptToRun = settings?.ScriptLocation ?? string.Empty;
            if (scriptToRun.IsNullOrEmpty())
            {
                Logger.LogError("Use Script is enabled but there is no script to run");
                return;
            }

            if (!File.Exists(scriptToRun))
            {
                Logger.LogError("Cannot find the file {0}", scriptToRun);
                return;
            }

            _processProvider.Start(scriptToRun, downloadUrl + " " + GetArgs(settings));

            Logger.LogDebug(LoggingEvents.Updater, "Script started");
        }

        private void Extract(string zipDir, string tempPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var files = ZipFile.OpenRead(zipDir))
                {
                    foreach (var entry in files.Entries)
                    {
                        if (entry.FullName.Contains("/"))
                        {
                            var path = Path.GetDirectoryName(Path.Combine(tempPath, entry.FullName));
                            Directory.CreateDirectory(path);
                        }

                        entry.ExtractToFile(Path.Combine(tempPath, entry.FullName));
                    }
                }
            }
            else
            {
                // Something else!
                using (var stream = File.Open(zipDir, FileMode.Open))
                using (var files = TarReader.Open(stream))
                {
                    Directory.CreateDirectory(tempPath);
                    files.WriteAllToDirectory(tempPath, new ExtractionOptions { Overwrite = true });
                }
            }
        }

        public async Task DownloadAsync(string requestUri, string filename)
        {
            Logger.LogDebug(LoggingEvents.Updater, "Starting the DownloadAsync");
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(requestUri, filename);
            }
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //Settings?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        public static void ExecLinuxCommand(string cmd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}