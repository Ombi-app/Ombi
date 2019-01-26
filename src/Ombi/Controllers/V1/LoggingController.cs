using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Models;

namespace Ombi.Controllers.V1
{
    [Authorize]
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        public LoggingController(ILogger logger)
        {
            Logger = logger;
        }
        
        private ILogger Logger { get; }
        private const string Message = "Exception: {0} at {1}. Stacktrace {2}";

        [HttpPost]
        public IActionResult Log([FromBody]UiLoggingModel l)
        {
            l.DateTime = DateTime.UtcNow;
            switch (l.Level)
            {
                case LogLevel.Trace:
                    Logger.LogTrace(new EventId(l.Id), Message, l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Debug:
                    Logger.LogDebug(new EventId(l.Id), Message, l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Information:
                    Logger.LogInformation(new EventId(l.Id), Message, l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Warning:
                    Logger.LogWarning(new EventId(l.Id), Message, l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Error:
                    Logger.LogError(new EventId(l.Id), Message, l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Critical:
                    Logger.LogCritical(new EventId(l.Id), Message, l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.None:
                    break;
            }

            return Ok();
        }
    }
}
