using System;

namespace Ombi.Core.Update
{
    public class UpdateOptions
    {
        public UpdateStatus Status { get; set; }
        public string DownloadUrl { get; set; }
        public string Version { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}