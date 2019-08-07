using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;

namespace Ombi.Controllers.V2
{
    [Admin]
    public class SystemController : V2Controller
    {
        private readonly IHostingEnvironment _hosting;

        public SystemController(IHostingEnvironment hosting)
        {
            _hosting = hosting;
        }

        [HttpGet("logs")]
        public IActionResult GetLogFiles()
        {
            var logsFolder = Path.Combine(_hosting.ContentRootPath, "Logs");
            var files = Directory
                .EnumerateFiles(logsFolder, "*.txt", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName);

            return Ok(files);
        }

        [HttpGet("logs/{logFileName}")]
        public async Task<IActionResult> ReadLogFile(string logFileName, CancellationToken token)
        {
            var logFile = Path.Combine(_hosting.ContentRootPath, "Logs", logFileName);
            using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                return Ok(await reader.ReadToEndAsync());
            }
        }
        
        [HttpGet("logs/download/{logFileName}")]
        public IActionResult Download(string logFileName, CancellationToken token)
        {
            var logFile = Path.Combine(_hosting.ContentRootPath, "Logs", logFileName);
            using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                return File(reader.BaseStream, "application/octet-stream", logFileName);
            }
        }
    }
}