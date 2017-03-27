using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Common.Processes
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartPath { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1} [{2}]", Id, Name ?? "Unknown", StartPath ?? "Unknown");
        }
    }
}
