using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

using Ombi.Api.Service;
using Ombi.Api.Service.Models;
using Ombi.Helpers;
using Ombi.Schedule.Ombi;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class OmbiAutomaticUpdater : IOmbiAutomaticUpdater
    {
        public OmbiAutomaticUpdater(ILogger<OmbiAutomaticUpdater> log, IOmbiService service)
        {
            Logger = log;
            OmbiService = service;
        }

        private ILogger<OmbiAutomaticUpdater> Logger { get; }
        private IOmbiService OmbiService { get; }

        public async Task Update(PerformContext c)
        {
            c.WriteLine("Starting the updater");
            // IF AutoUpdateEnabled =>
            // ELSE Return;
            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            c.WriteLine("Path: {0}", currentLocation);

            var productVersion = AssemblyHelper.GetRuntimeVersion();
            Logger.LogInformation(LoggingEvents.Updater, "Product Version {0}", productVersion);
            c.WriteLine("Product Version {0}", productVersion);

            var productArray = productVersion.Split('-');
            var version = productArray[0];
            var branch = productArray[1];


            Logger.LogInformation(LoggingEvents.Updater, "Version {0}", version);
            Logger.LogInformation(LoggingEvents.Updater, "Branch {0}", branch);

            try
            {
                var updates = await OmbiService.GetUpdates(branch);
                var serverVersion = updates.UpdateVersionString;


                Logger.LogInformation(LoggingEvents.Updater, "Service Version {0}", updates.UpdateVersionString);
                c.WriteLine("Service Version {0}", updates.UpdateVersionString);

                if (!serverVersion.Equals(version, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Let's download the correct zip
                    var desc = RuntimeInformation.OSDescription;
                    var proce = RuntimeInformation.ProcessArchitecture;

                    Logger.LogInformation(LoggingEvents.Updater, "OS Information: {0} {1}", desc, proce);
                    Download download;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Logger.LogInformation(LoggingEvents.Updater, "We are Windows");
                        download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("windows", CompareOptions.IgnoreCase));
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Logger.LogInformation(LoggingEvents.Updater, "We are OSX");
                        download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("osx", CompareOptions.IgnoreCase));

                    }
                    else
                    {
                        // Linux
                        if (desc.Contains("ubuntu", CompareOptions.IgnoreCase))
                        {
                            // Ubuntu
                            Logger.LogInformation(LoggingEvents.Updater, "We are ubuntu");
                            download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("ubuntu", CompareOptions.IgnoreCase));

                        }
                        else if (desc.Contains("debian", CompareOptions.IgnoreCase))
                        {
                            // Debian
                            Logger.LogInformation(LoggingEvents.Updater, "We are debian");
                            download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("debian", CompareOptions.IgnoreCase));
                        }
                        else if (desc.Contains("centos", CompareOptions.IgnoreCase))
                        {
                            // Centos
                            Logger.LogInformation(LoggingEvents.Updater, "We are centos");
                            download = updates.Downloads.FirstOrDefault(x => x.Name.Contains("centos",
                                CompareOptions.IgnoreCase));
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (download == null)
                    {
                        return;
                    }

                    // Download it
                    Logger.LogInformation(LoggingEvents.Updater, "Downloading the file {0} from {1}", download.Name, download.Url);
                    var extension = download.Name.Split('.').Last();
                    var zipDir = Path.Combine(currentLocation, $"Ombi.{extension}");
                    try
                    {
                        if (File.Exists(zipDir))
                        {
                            File.Delete(zipDir);
                        }

                        await DownloadAsync(download.Url, zipDir);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(LoggingEvents.Updater, e, "Error when downloading the zip");
                        throw;
                    }

                    var tempPath = Path.Combine(currentLocation, "TempUpdate");
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                    // Extract it
                    using (var files = ZipFile.OpenRead(zipDir))
                    {
                        // Temp Path
                        Directory.CreateDirectory(tempPath);
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

                    // There must be an update
                    var start = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "Ombi.Updater",
                        Arguments = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + " " + extension,
                    };
                    using (var proc = new Process { StartInfo = start })
                    {
                        proc.Start();
                    }

                }
            }
            catch (Exception e)
            {
                c.WriteLine(e);
                throw;
            }
        }

        public static async Task DownloadAsync(string requestUri, string filename)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
            }
        }
    }
}