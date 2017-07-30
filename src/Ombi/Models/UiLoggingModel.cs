using Microsoft.Extensions.Logging;
using System;

namespace Ombi.Models
{
    public class UiLoggingModel
    {
        public LogLevel Level { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public string Location { get; set; }
        public string StackTrace { get; set; }
        public DateTime DateTime { get; set; }
    }
}
