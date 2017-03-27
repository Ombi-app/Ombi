#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: InstallService.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using NLog;
using Ombi.Common;
using Ombi.Common.EnvironmentInfo;
using Ombi.Common.Processes;

namespace Ombi.Updater
{
    public class InstallService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IProcessProvider _processProvider = new ProcessProvider();
        private string BackupPath { get; set; }
        private string TempPath { get; set; }
        public void Start(UpdateStartupContext ctx)
        {
            var dector = new DetectApplicationType();

            var processId = _processProvider.FindProcessByName(ProcessProvider.OmbiProcessName)?.FirstOrDefault()?.Id ?? -1;

            // Log if process is -1

            var dir = CreateTempPath();
            TempPath = Path.Combine(dir.FullName, "OmbiUpdate.zip");
            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) =>
                {
                    Console.WriteLine($"{e.ProgressPercentage}%");
                };
                client.DownloadFile(ctx.DownloadPath, TempPath);
            }

            var appType = dector.GetAppType();
            _processProvider.FindProcessByName(ProcessProvider.OmbiProcessName);
            var installationFolder = GetInstallationDirectory(ctx);
            var terminator = new TerminateOmbi(new ServiceProvider(_processProvider), _processProvider);
            if (OsInfo.IsWindows)
            {
                terminator.Terminate(processId);
            }
            try
            {
                BackupCurrentVersion();
                EmptyInstallationFolder();

                using (var archive = ZipFile.OpenRead(TempPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fullname = string.Empty;
                        if (entry.FullName.Contains("Release/")) // Don't extract the release folder, we are already in there
                        {
                            fullname = entry.FullName.Replace("Release/", string.Empty);
                        }
                        if (entry.Name.Contains("UpdateService"))
                        {
                            fullname = entry.FullName.Replace("UpdateService", "UpdateService_New");
                        }

                        var fullPath = Path.Combine(PathUp(Path.GetDirectoryName(Application.ExecutablePath),1),fullname);



                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            if (entry.Name.Contains("Updater"))
                            {
                                continue;
                            }

                            entry.ExtractToFile(fullPath, true);
                            Console.WriteLine("Restored {0}", entry.FullName);
                        }
                    }
                }

                // Need to install here 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                RestoreBackup();
                throw;
            }
            finally
            {
                var startOmbi = new StartOmbi(new ServiceProvider(_processProvider), _processProvider);
                if (OsInfo.IsWindows)
                {
                    startOmbi.Start(appType, installationFolder);
                }
                else
                {
                    terminator.Terminate(processId);

                    Logger.Info("Waiting for external auto-restart.");
                    for (int i = 0; i < 5; i++)
                    {
                        System.Threading.Thread.Sleep(1000);

                        if (_processProvider.Exists(ProcessProvider.OmbiProcessName))
                        {
                            Logger.Info("Ombi was restarted by external process.");
                            break;
                        }
                    }

                    if (!_processProvider.Exists(ProcessProvider.OmbiProcessName))
                    {
                        startOmbi.Start(appType, installationFolder, ctx.StartupArgs);
                    }
                }
            }

        }

        private DirectoryInfo CreateTempPath()
        {
            try
            {
                var location = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Updater)).Location ?? string.Empty);
                var path = Path.Combine(location, "UpdateTemp");
                return Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Environment.Exit(1);
                return null;
            }
        }
        public void RestoreBackup()
        {
            Console.WriteLine("Update failed, restoring backup");
            using (var archive = ZipFile.OpenRead(BackupPath))
            {
                foreach (var entry in archive.Entries)
                {
                    var fullPath = Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)), entry.FullName);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    if (entry.Name.Contains("UpdateService"))
                    {
                        continue;
                    }
                    else
                    {
                        if (entry.Name.Contains("Ombi.Updater"))
                        {
                            entry.ExtractToFile(fullPath + "_Updated", true);
                            continue;
                        }

                        entry.ExtractToFile(fullPath, true);
                        Console.WriteLine("Update failed, restoring backup");
                    }
                }
            }
        }
        private string GetInstallationDirectory(UpdateStartupContext startupContext)
        {

                Logger.Debug("Using process ID to find installation directory: {0}", startupContext.ProcessId);
                var exeFileInfo = new FileInfo(_processProvider.GetProcessById(startupContext.ProcessId).StartPath);
                Logger.Debug("Executable location: {0}", exeFileInfo.FullName);

                return exeFileInfo.DirectoryName;
            

        }

        private void BackupCurrentVersion()
        {
            Console.WriteLine("Backing up the current version");
            try
            {
                var applicationPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(InstallService)).Location ?? string.Empty) ?? string.Empty;

                var dir = Directory.CreateDirectory(Path.Combine(applicationPath, "BackupSystem"));

                var allfiles = Directory.GetFiles(applicationPath, "*.*", SearchOption.AllDirectories);
                BackupPath = Path.Combine(dir.FullName, "OmbiBackup.zip");

                CheckAndDelete(BackupPath);
                using (var fileStream = new FileStream(BackupPath, FileMode.CreateNew))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in allfiles)
                    {
                        if (file.Contains("BackupSystem"))
                            continue;
                        var info = Path.GetFileName(file);
                        archive.CreateEntryFromFile(file, info);
                    }
                }
                Console.WriteLine("All backed up!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Environment.Exit(1);
            }
        }
        private void CheckAndDelete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private void EmptyInstallationFolder()
        {
            var applicationPath = PathUp(Path.GetDirectoryName(Assembly.GetAssembly(typeof(InstallService)).Location ?? string.Empty) ?? string.Empty,1);
            var allfiles = Directory.GetFiles(applicationPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allfiles)
            {
                if(file.Contains("BackupSystem") || file.Contains("UpdateService") || file.Contains(".sqlite")) continue;
                CheckAndDelete(file);
            }
        }
        static string PathUp(string path, int up)
        {
            if (up == 0)
                return path;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == Path.DirectorySeparatorChar)
                {
                    up--;
                    if (up == 0)
                        return path.Substring(0, i);
                }
            }
            return null;
        }
    }

    public class UpdateStartupContext
    {
        public int ProcessId { get; set; }
        public string DownloadPath { get; set; }
        public string StartupArgs { get; set; }
    }
}