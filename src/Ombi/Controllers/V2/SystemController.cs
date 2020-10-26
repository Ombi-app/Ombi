using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;

namespace Ombi.Controllers.V2
{
    [Admin]
    public class SystemController : V2Controller
    {
        private readonly IWebHostEnvironment _hosting;
        private readonly HttpClient _client;

        public SystemController(IWebHostEnvironment hosting, IHttpClientFactory httpClientFactory)
        {
            _hosting = hosting;
            _client = httpClientFactory.CreateClient();
        }

        [HttpGet("news")]
        public async Task<IActionResult> GetNews()
        {
            var result = await _client.GetAsync("https://raw.githubusercontent.com/tidusjar/Ombi.News/main/README.md");
            var content = await result.Content.ReadAsStringAsync();
            var md = Markdown.ToHtml(content);
            return Ok(md);
        }

        [HttpGet("logs")]
        public IActionResult GetLogFiles()
        {
            var logsFolder = Path.Combine(string.IsNullOrEmpty(Ombi.Helpers.StartupSingleton.Instance.StoragePath) ? _hosting.ContentRootPath : Helpers.StartupSingleton.Instance.StoragePath, "Logs");
            var files = Directory
                .EnumerateFiles(logsFolder, "*.txt", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderByDescending(name => name);

            return Ok(files);
        }

        [HttpGet("logs/{logFileName}")]
        public async Task<IActionResult> ReadLogFile(string logFileName, CancellationToken token)
        {
            var logFile = Path.Combine(string.IsNullOrEmpty(Ombi.Helpers.StartupSingleton.Instance.StoragePath) ? _hosting.ContentRootPath : Helpers.StartupSingleton.Instance.StoragePath, "Logs", logFileName);
            using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                return Ok(await reader.ReadToEndAsync());
            }
        }

        [HttpGet("logs/download/{logFileName}")]
        public IActionResult Download(string logFileName, CancellationToken token)
        {
            var logFile = Path.Combine(string.IsNullOrEmpty(Ombi.Helpers.StartupSingleton.Instance.StoragePath) ? _hosting.ContentRootPath : Helpers.StartupSingleton.Instance.StoragePath, "Logs", logFileName);
            using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                return File(reader.BaseStream, "application/octet-stream", logFileName);
            }
        }
    }
}