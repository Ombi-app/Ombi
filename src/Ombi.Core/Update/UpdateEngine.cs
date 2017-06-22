using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ombi.Core.Update
{
    public class UpdateEngine
    {
        public async Task Update(UpdateOptions options)
        {
            if (options.Status == UpdateStatus.UptoDate)
            {
                // We don't need to update...
                return;
            }
            // Download zip into temp location
            var path = await Download(options);

            Extract(path);

            
            // TODO Run the Update.exe and pass in the args
        }

        private void Extract(string path)
        {
            using (var zip = ZipFile.OpenRead(path))
            {
                path = Path.GetDirectoryName(path);
                foreach (var entry in zip.Entries.Skip(1))
                {
                    var fullname = string.Empty;
                    if (entry.FullName.Contains("publish/")) // Don't extract the publish folder, we are already in there
                    {
                        fullname = entry.FullName.Replace("publish/", string.Empty);
                    }
                    
                    var fullPath = Path.Combine(path, fullname);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        entry.ExtractToFile(fullPath, true);
                        Console.WriteLine("Restored {0}", entry.FullName);
                    }
                }
            }
        }

        /// <summary>
        /// Downloads the specified zip from the options and returns the zip path.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private async Task<string> Download(UpdateOptions options)
        {

            // Create temp path
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var current = Path.GetDirectoryName(location);
            var tempDir = Directory.CreateDirectory(Path.Combine(current, "UpdateTemp"));
            var tempZip = Path.Combine(tempDir.FullName, "Ombi.zip");

            if (File.Exists(tempZip))
            {
                return tempZip;
            }
            
            using (var httpClient = new HttpClient())
            using (var contentStream = await httpClient.GetStreamAsync(options.DownloadUrl))
            using (var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, true))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            return tempZip;
        }


        public UpdateOptions CheckForUpdate()
        {
            return new UpdateOptions
            {
                Status = UpdateStatus.Available,
                DownloadUrl = "https://ci.appveyor.com/api/buildjobs/tsghsfcaoqin2wbk/artifacts/Ombi_windows.zip",
                UpdateDate = DateTime.Now,
                Version = "3.0.0"
            };
        }
    }
}