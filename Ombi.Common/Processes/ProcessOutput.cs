
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Common.Processes
{
    public class ProcessOutput
    {
        public int ExitCode { get; set; }
        public List<ProcessOutputLine> Lines { get; set; }

        public ProcessOutput()
        {
            Lines = new List<ProcessOutputLine>();
        }

        public List<ProcessOutputLine> Standard
        {
            get
            {
                return Lines.Where(c => c.Level == ProcessOutputLevel.Standard).ToList();
            }
        }

        public List<ProcessOutputLine> Error
        {
            get
            {
                return Lines.Where(c => c.Level == ProcessOutputLevel.Error).ToList();
            }
        }
    }

    public class ProcessOutputLine
    {
        public ProcessOutputLevel Level { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public ProcessOutputLine(ProcessOutputLevel level, string content)
        {
            Level = level;
            Content = content;
            Time = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", Time, Level, Content);
        }
    }

    public enum ProcessOutputLevel
    {
        Standard = 0,
        Error = 1
    }
}