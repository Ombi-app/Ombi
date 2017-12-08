using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

using Ombi.Api.Service;
using Ombi.Api.Service.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Updater;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class OmbiAutomaticUpdater : IOmbiAutomaticUpdater
    {
        public OmbiAutomaticUpdater(ILogger<OmbiAutomaticUpdater> log, IOmbiService service,
            ISettingsService<UpdateSettings> s, IProcessProvider proc, IRepository<ApplicationConfiguration> appConfig)
        {
            Logger = log;
            OmbiService = service;
            Settings = s;
            _processProvider = proc;
            _appConfig = appConfig;
            Settings.ClearCache();
        }

        private ILogger<OmbiAutomaticUpdater> Logger { get; }
        private IOmbiService OmbiService { get; }
        private ISettingsService<UpdateSettings> Settings { get; }
        private readonly IProcessProvider _processProvider;
        private static PerformContext Ctx { get; set; }
        private readonly IRepository<ApplicationConfiguration> _appConfig;

        public string[] GetVersion()
        {
            var productVersion = AssemblyHelper.GetRuntimeVersion();
            var productArray = productVersion.Split('-');
            return productArray;
        }
        public async Task<bool> UpdateAvailable(string branch, string currentVersion)
        {

            var updates = await OmbiService.GetUpdates(branch);
            var serverVersion = updates.UpdateVersionString;
            return !serverVersion.Equals(currentVersion, StringComparison.CurrentCultureIgnoreCase);

        }

        [AutomaticRetry(Attempts = 1)]
        public async Task Update(PerformContext c)
        {
            Ctx = c;
            Ctx.WriteLine("Starting the updater");

            var settings = await Settings.GetSettingsAsync();
            if (!settings.AutoUpdateEnabled)
            {
                Ctx.WriteLine("Auto update is not enabled");
                return;
            }

            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Ctx.WriteLine("Path: {0}", currentLocation);

            var productVersion = AssemblyHelper.GetRuntimeVersion();
            Logger.LogInformation(LoggingEvents.Updater, "Product Version {0}", productVersion);
            Ctx.WriteLine("Product Version {0}", productVersion);

            try
            {
                var productArray = GetVersion();
                var version = productArray[0];
                Ctx.WriteLine("Version {0}", version);
                var branch = productArray[1];
                Ctx.WriteLine("Branch Version {0}", branch);

                Logger.LogInformation(LoggingEvents.Updater, "Version {0}", version);
                Logger.LogInformation(LoggingEvents.Updater, "Branch {0}", branch);

                Ctx.WriteLine("Looking for updates now");
                var updates = await OmbiService.GetUpdates(branch);
                Ctx.WriteLine("Updates: {0}", updates);
                var serverVersion = updates.UpdateVersionString;

                Logger.LogInformation(LoggingEvents.Updater, "Service Version {0}", updates.UpdateVersionString);
                Ctx.WriteLine("Service Version {0}", updates.UpdateVersionString);

                if (!serverVersion.Equals(version, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Let's download the correct zip
                    var desc = RuntimeInformation.OSDescription;
                    var proce = RuntimeInformation.ProcessArchitecture;

                    Logger.LogInformation(LoggingEvents.Updater, "OS Information: {0} {1}", desc, proce);
                    Ctx.WriteLine("OS Information: {0} {1}", desc, proce);
                    Download download;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Logger.LogInformation(LoggingEvents.Updater, "We are Windows");
                        download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("windows.zip", CompareOptions.IgnoreCase));
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Logger.LogInformation(LoggingEvents.Updater, "We are OSX");
                        download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("osx", CompareOptions.IgnoreCase));
                    }
                    else
                    {
                        Logger.LogInformation(LoggingEvents.Updater, "We are linux");
                        download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("linux", CompareOptions.IgnoreCase));
                    }
                    if (download == null)
                    {
                        Ctx.WriteLine("There were no downloads");
                        return;
                    }

                    Ctx.WriteLine("Found the download! {0}", download.Name);
                    Ctx.WriteLine("URL {0}", download.Url);

                    Ctx.WriteLine("Clearing out Temp Path");
                    var tempPath = Path.Combine(currentLocation, "TempUpdate");
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }

                    // Temp Path
                    Directory.CreateDirectory(tempPath);


                    if (settings.UseScript && !settings.WindowsService)
                    {
                        RunScript(settings, download.Url);
                        return;
                    }

                    // Download it
                    Logger.LogInformation(LoggingEvents.Updater, "Downloading the file {0} from {1}", download.Name, download.Url);
                    var extension = download.Name.Split('.').Last();
                    var zipDir = Path.Combine(currentLocation, $"Ombi.{extension}");
                    Ctx.WriteLine("Zip Dir: {0}", zipDir);
                    try
                    {
                        if (File.Exists(zipDir))
                        {
                            File.Delete(zipDir);
                        }

                        Ctx.WriteLine("Starting Download");
                        await DownloadAsync(download.Url, zipDir, c);
                        Ctx.WriteLine("Finished Download");
                    }
                    catch (Exception e)
                    {
                        Ctx.WriteLine("Error when downloading");
                        Ctx.WriteLine(e.Message);
                        Logger.LogError(LoggingEvents.Updater, e, "Error when downloading the zip");
                        throw;
                    }

                    // Extract it
                    Ctx.WriteLine("Extracting ZIP");
                    Extract(zipDir, tempPath);

                    Ctx.WriteLine("Finished Extracting files");
                    Ctx.WriteLine("Starting the Ombi.Updater process");
                    var updaterExtension = string.Empty;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        updaterExtension = ".exe";
                    }
                    var updaterFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        "TempUpdate", $"Ombi.Updater{updaterExtension}");

                    // There must be an update
                    var start = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = updaterFile,
                        Arguments = GetArgs(settings),
                        WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "TempUpdate"),
                    };
                    if (settings.Username.HasValue())
                    {
                        start.UserName = settings.Username;
                    }
                    if (settings.Password.HasValue())
                    {
                        start.Password = settings.Password.ToSecureString();
                    }
                    using (var proc = new Process { StartInfo = start })
                    {
                        proc.Start();
                    }
                    Ctx.WriteLine("Bye bye");
                }
            }
            catch (Exception e)
            {
                Ctx.WriteLine(e);

                throw;
            }
        }

        private string GetArgs(UpdateSettings settings)
        {
            var config = _appConfig.GetAll();
            var url = config.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
            var storage = config.FirstOrDefault(x => x.Type == ConfigurationTypes.StoragePath);

            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var processName = (settings.ProcessName.HasValue() ? settings.ProcessName : "Ombi");

            var sb = new StringBuilder();
            sb.Append($"--applicationPath \"{currentLocation}\" --processname \"{processName}\" " );
            if (settings.WindowsService)
            {
                sb.Append($"--windowsServiceName \"{settings.WindowsServiceName}\" ");
            }
            var sb2 = new StringBuilder();
            var hasStartupArgs = false;
            if (url?.Value.HasValue() ?? false)
            {
                hasStartupArgs = true;
                sb2.Append(url.Value);
            }
            if (storage?.Value.HasValue() ?? false)
            {
                hasStartupArgs = true;
                sb2.Append(storage.Value);
            }
            if (hasStartupArgs)
            {
                sb.Append($"--startupArgs {sb2.ToString()}");
            }

            return sb.ToString();
            //return string.Join(" ", currentLocation, processName, url?.Value ?? string.Empty, storage?.Value ?? string.Empty);
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

            Logger.LogInformation("Script started");
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

        public async Task DownloadAsync(string requestUri, string filename, PerformContext ctx)
        {
            Logger.LogDebug("Starting the DownloadAsync");
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(requestUri, filename);
            }
        }
    }
}