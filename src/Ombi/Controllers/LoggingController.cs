using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Models;
using System;

namespace Ombi.Controllers
{
    [Authorize]
    public class LoggingController : BaseV1ApiController
    {
        public LoggingController(ILogger logger)
        {
            Logger = logger;
        }
        
        private ILogger Logger { get; }

        [HttpPost]
        public void Log([FromBody]UiLoggingModel l)
        {
            l.DateTime = DateTime.UtcNow;
            var exception = new Exception(l.Description, new Exception(l.StackTrace));

            switch (l.Level)
            {
                case LogLevel.Trace:
                    Logger.LogTrace(new EventId(l.Id), "Exception: {0} at {1}. Stacktrade {2}", l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Debug:
                    Logger.LogDebug(new EventId(l.Id), "Exception: {0} at {1}. Stacktrade {2}", l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Information:
                    Logger.LogInformation(new EventId(l.Id), "Exception: {0} at {1}. Stacktrade {2}", l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Warning:
                    Logger.LogWarning(new EventId(l.Id), "Exception: {0} at {1}. Stacktrade {2}", l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Error:
                    Logger.LogError(new EventId(l.Id), "Exception: {0} at {1}. Stacktrade {2}", l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.Critical:
                    Logger.LogCritical(new EventId(l.Id), "Exception: {0} at {1}. Stacktrade {2}", l.Description, l.Location, l.StackTrace);
                    break;
                case LogLevel.None:
                    break;
            }
        }
    }
}
