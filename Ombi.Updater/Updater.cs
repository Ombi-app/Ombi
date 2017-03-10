#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Updater.cs
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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace Ombi.Updater
{
    public class Updater
    {
        private string BackupPath { get; set; }
        private bool Error { get; set; }
        private string TempPath { get; set; }

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

        public void Start(string downloadPath, string launchOptions)
        {
            try
            {
                BackupCurrentVersion();
                var dir = CreateTempPath();
                TempPath = Path.Combine(dir.FullName, "OmbiUpdate.zip");

                CheckAndDelete(TempPath);
                Console.WriteLine("Downloading new version");
                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        Console.WriteLine($"{e.ProgressPercentage}%");
                    };
                    client.DownloadFile(downloadPath, TempPath);
                }
                Console.WriteLine("Downloaded!");


                // Replace files
                using (var archive = ZipFile.OpenRead(TempPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fullname = string.Empty;
                        if (entry.FullName.Contains("Release/")) // Don't extract the release folder, we are already in there
                        {
                            fullname = entry.FullName.Replace("Release/", string.Empty);
                        }

                        var fullPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), fullname);

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            if (entry.Name.Contains("Ombi.Updater"))
                            {
                                entry.ExtractToFile(fullPath + "_Updated", true);
                                continue;
                            }

                            entry.ExtractToFile(fullPath, true);
                            Console.WriteLine("Restored {0}", entry.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Oops... Looks like we cannot update!");
                Console.ReadLine();
                Error = true;
            }
            finally
            {
                File.Delete(TempPath);
                if (Error)
                {
                    RestoreBackup();
                }

                FinishUpdate(launchOptions);
            }
        }

        private void BackupCurrentVersion()
        {
            Console.WriteLine("Backing up the current version");
            try
            {
                var applicationPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Updater)).Location ?? string.Empty) ?? string.Empty;

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

        private DirectoryInfo CreateTempPath()
        {
            try            {
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

        private void FinishUpdate(string launchOptions)
        {
            var args = Error ? "-u 2" : "-u 1";
            var startInfo = new ProcessStartInfo($"{launchOptions}Ombi.exe") { Arguments = args, UseShellExecute = true };

            Process.Start(startInfo);

            Environment.Exit(0);
        }
    }
}